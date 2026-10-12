-- RunAlways scripts re-run on every deploy (used for views and stored procedures).
-- Use CREATE OR ALTER so they're idempotent.

-- View: monthly spend summary used by the report endpoint
CREATE OR ALTER VIEW dbo.vw_MonthlySpend AS
SELECT
    YEAR(p.PaidAtUTC)  AS [Year],
    MONTH(p.PaidAtUTC) AS [Month],
    c.CategoryId,
    c.Name             AS CategoryName,
    c.MonthlyLimit,
    SUM(p.Amount)      AS TotalSpent
FROM dbo.Payment p
JOIN dbo.Category c ON c.CategoryId = p.CategoryId
WHERE p.Status = 1  -- Paid only
GROUP BY YEAR(p.PaidAtUTC), MONTH(p.PaidAtUTC), c.CategoryId, c.Name, c.MonthlyLimit;
GO

-- Stored proc for pass generation (heavy logic)
CREATE OR ALTER PROCEDURE dbo.usp_GeneratePass
    @StartUTC DATETIMEOFFSET,
    @EndUTC   DATETIMEOFFSET
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        b.BillId, b.AccountId, b.CategoryId, b.Name, b.ExpectedAmount,
        b.ScheduleType, b.DayOfMonth, b.AnchorDateUTC, b.IsAutoPay,
        a.Name AS AccountName, c.Name AS CategoryName
    FROM dbo.Bill b
    JOIN dbo.Account a  ON a.AccountId  = b.AccountId
    JOIN dbo.Category c ON c.CategoryId = b.CategoryId
    WHERE b.IsActive = 1;

    SELECT
        p.PaymentId, p.BillId, p.AccountId, p.CategoryId,
        p.DueAtUTC, p.PaidAtUTC, p.Amount, p.Status, p.Note,
        p.ModifiedBy, p.ModifiedUTC,
        b.Name AS BillName, c.Name AS CategoryName
    FROM dbo.Payment p
    LEFT JOIN dbo.Bill b ON b.BillId = p.BillId
    JOIN dbo.Category c ON c.CategoryId = p.CategoryId
    WHERE p.DueAtUTC >= @StartUTC AND p.DueAtUTC < @EndUTC;
END;
GO
