using Budget.Application.Contracts.Options;
using Budget.Shared.Bill.Model;
using Dapper;
using Microsoft.Extensions.Options;

namespace Budget.Shared.Bill.Repository;

public class RepositoryBill(IOptions<RepositoryOptions> options)
    : FRepositoryBase(options.Value.BudgetDatabaseConnectionString), IRepositoryBill
{
    private const string SelectColumns = """
        b.BillId, b.AccountId, b.CategoryId, b.Name, b.ExpectedAmount,
        b.ScheduleType, b.DayOfMonth, b.AnchorDateUTC, b.IsAutoPay, b.IsActive,
        a.Name AS AccountName, c.Name AS CategoryName
        """;

    public async Task<IEnumerable<FBill>> SelectBillsAsync()
    {
        var sql = $"""
            SELECT {SelectColumns}
            FROM dbo.Bill b
            JOIN dbo.Account a ON a.AccountId = b.AccountId
            JOIN dbo.Category c ON c.CategoryId = b.CategoryId
            ORDER BY b.Name
            """;
        using var connection = GetConnection();
        return await connection.QueryAsync<FBill>(sql);
    }

    public async Task<IEnumerable<FBill>> SelectActiveBillsAsync()
    {
        var sql = $"""
            SELECT {SelectColumns}
            FROM dbo.Bill b
            JOIN dbo.Account a ON a.AccountId = b.AccountId
            JOIN dbo.Category c ON c.CategoryId = b.CategoryId
            WHERE b.IsActive = 1
            ORDER BY b.Name
            """;
        using var connection = GetConnection();
        return await connection.QueryAsync<FBill>(sql);
    }

    public async Task<FBill?> SelectBillByIdAsync(int billId)
    {
        var sql = $"""
            SELECT {SelectColumns}
            FROM dbo.Bill b
            JOIN dbo.Account a ON a.AccountId = b.AccountId
            JOIN dbo.Category c ON c.CategoryId = b.CategoryId
            WHERE b.BillId = @BillId
            """;
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<FBill>(sql, new { BillId = billId });
    }

    public async Task<int> InsertBillAsync(FBill bill)
    {
        const string sql = """
            INSERT INTO dbo.Bill (AccountId, CategoryId, Name, ExpectedAmount, ScheduleType, DayOfMonth, AnchorDateUTC, IsAutoPay, IsActive)
            OUTPUT INSERTED.BillId
            VALUES (@AccountId, @CategoryId, @Name, @ExpectedAmount, @ScheduleType, @DayOfMonth, @AnchorDateUTC, @IsAutoPay, @IsActive)
            """;
        using var connection = GetConnection();
        return await connection.ExecuteScalarAsync<int>(sql, bill);
    }

    public async Task UpdateBillAsync(FBill bill)
    {
        const string sql = """
            UPDATE dbo.Bill
            SET AccountId = @AccountId, CategoryId = @CategoryId, Name = @Name,
                ExpectedAmount = @ExpectedAmount, ScheduleType = @ScheduleType,
                DayOfMonth = @DayOfMonth, AnchorDateUTC = @AnchorDateUTC, IsAutoPay = @IsAutoPay
            WHERE BillId = @BillId
            """;
        using var connection = GetConnection();
        await connection.ExecuteAsync(sql, bill);
    }

    public async Task DeleteBillAsync(int billId)
    {
        const string sql = "UPDATE dbo.Bill SET IsActive = 0 WHERE BillId = @BillId";
        using var connection = GetConnection();
        await connection.ExecuteAsync(sql, new { BillId = billId });
    }
}
