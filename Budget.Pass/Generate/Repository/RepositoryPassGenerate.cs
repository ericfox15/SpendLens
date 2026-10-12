using Budget.Application.Contracts.Options;
using Budget.Pass.Generate.Model;
using Budget.Shared;
using Budget.Shared.Account.Model;
using Budget.Shared.Bill.Model;
using Budget.Shared.Payment.Model;
using Dapper;
using Microsoft.Extensions.Options;

namespace Budget.Pass.Generate.Repository;

public class RepositoryPassGenerate(IOptions<RepositoryOptions> options)
    : FRepositoryBase(options.Value.BudgetDatabaseConnectionString), IRepositoryPassGenerate
{
    public async Task<IEnumerable<FBill>> SelectActiveBillsAsync()
    {
        const string sql = """
            SELECT b.BillId, b.AccountId, b.CategoryId, b.Name, b.ExpectedAmount,
                   b.ScheduleType, b.DayOfMonth, b.AnchorDateUTC, b.IsAutoPay, b.IsActive,
                   a.Name AS AccountName, c.Name AS CategoryName
            FROM dbo.Bill b
            JOIN dbo.Account a ON a.AccountId = b.AccountId
            JOIN dbo.Category c ON c.CategoryId = b.CategoryId
            WHERE b.IsActive = 1
            """;
        using var connection = GetConnection();
        return await connection.QueryAsync<FBill>(sql);
    }

    public async Task<IEnumerable<FAccount>> SelectActiveAccountsAsync()
    {
        const string sql = "SELECT AccountId, Name, AccountType, IsActive FROM dbo.Account WHERE IsActive = 1";
        using var connection = GetConnection();
        return await connection.QueryAsync<FAccount>(sql);
    }

    public async Task<IEnumerable<FPayment>> SelectExistingPaymentsAsync(DateTimeOffset startUTC, DateTimeOffset endUTC)
    {
        const string sql = """
            SELECT p.PaymentId, p.BillId, p.AccountId, p.CategoryId, p.DueAtUTC, p.PaidAtUTC,
                   p.Amount, p.Status, p.Note, p.ModifiedBy, p.ModifiedUTC,
                   b.Name AS BillName, c.Name AS CategoryName,
                   b.ExpectedAmount, b.IsAutoPay
            FROM dbo.Payment p
            LEFT JOIN dbo.Bill b ON b.BillId = p.BillId
            JOIN dbo.Category c ON c.CategoryId = p.CategoryId
            WHERE p.DueAtUTC >= @StartUTC AND p.DueAtUTC < @EndUTC
            """;
        using var connection = GetConnection();
        return await connection.QueryAsync<FPayment>(sql, new { StartUTC = startUTC, EndUTC = endUTC });
    }

    public async Task<int> InsertPaymentAsync(FPayment payment)
    {
        const string sql = """
            INSERT INTO dbo.Payment (BillId, AccountId, CategoryId, DueAtUTC, Amount, Status, ModifiedBy, ModifiedUTC)
            OUTPUT INSERTED.PaymentId
            VALUES (@BillId, @AccountId, @CategoryId, @DueAtUTC, @Amount, @Status, @ModifiedBy, @ModifiedUTC)
            """;
        using var connection = GetConnection();
        return await connection.ExecuteScalarAsync<int>(sql, payment);
    }

    public async Task<IEnumerable<AdherenceFlag>> SelectAdherenceFlagsAsync()
    {
        // Bills skipped 3+ times in the last 90 days — mirrors eMAR compliance "N doses missed"
        const string sql = """
            SELECT b.BillId, b.Name AS BillName, COUNT(*) AS SkippedCount
            FROM dbo.Payment p
            JOIN dbo.Bill b ON b.BillId = p.BillId
            WHERE p.Status = 2 AND p.DueAtUTC >= DATEADD(DAY, -90, SYSDATETIMEOFFSET())
            GROUP BY b.BillId, b.Name
            HAVING COUNT(*) >= 3
            ORDER BY SkippedCount DESC
            """;
        using var connection = GetConnection();
        return await connection.QueryAsync<AdherenceFlag>(sql);
    }

    public async Task<int> SelectOverdueCountAsync()
    {
        const string sql = """
            SELECT COUNT(*)
            FROM dbo.Payment
            WHERE Status = 0 AND DueAtUTC < SYSDATETIMEOFFSET()
            """;
        using var connection = GetConnection();
        return await connection.ExecuteScalarAsync<int>(sql);
    }
}
