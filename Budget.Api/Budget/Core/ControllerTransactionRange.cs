using Budget.Application.Contracts.Options;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Budget.Api.Budget.Core;

public class TransactionRow
{
    public int PaymentId { get; set; }
    public string Merchant { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTimeOffset Date { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
}

public class TransactionRangeResult
{
    public List<CategorySpendReport> Categories { get; set; } = new();
    public List<TransactionRow> Transactions { get; set; } = new();
    public decimal TotalSpent { get; set; }
}

/// <summary>
/// Transaction Range — mirrors eMAR's medpass generate endpoint.
/// Instead of generating bills due in a window, this queries imported transactions
/// in a date range and returns a category breakdown with percentages.
/// </summary>
[Route("api/transaction-range")]
public class ControllerTransactionRange(IOptions<RepositoryOptions> options) : FControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] DateTimeOffset start,
        [FromQuery] DateTimeOffset end,
        [FromQuery] int accountId = 0)
    {
        const string categorySql = """
            SELECT c.CategoryId, c.Name AS CategoryName,
                   ISNULL(SUM(p.Amount), 0) AS TotalSpent,
                   c.MonthlyLimit
            FROM dbo.Payment p
            JOIN dbo.Category c ON c.CategoryId = p.CategoryId
            WHERE p.PaidAtUTC >= @Start AND p.PaidAtUTC < @End
              AND (@AccountId = 0 OR p.AccountId = @AccountId)
            GROUP BY c.CategoryId, c.Name, c.MonthlyLimit
            HAVING ISNULL(SUM(p.Amount), 0) > 0
            ORDER BY TotalSpent DESC
            """;

        const string txSql = """
            SELECT p.PaymentId, p.Note AS Merchant, p.Amount,
                   p.PaidAtUTC AS Date,
                   c.Name AS CategoryName,
                   a.Name AS AccountName
            FROM dbo.Payment p
            JOIN dbo.Category c ON c.CategoryId = p.CategoryId
            JOIN dbo.Account a ON a.AccountId = p.AccountId
            WHERE p.PaidAtUTC >= @Start AND p.PaidAtUTC < @End
              AND (@AccountId = 0 OR p.AccountId = @AccountId)
            ORDER BY p.PaidAtUTC DESC
            """;

        using var connection = new SqlConnection(options.Value.BudgetDatabaseConnectionString);
        var args = new { Start = start, End = end, AccountId = accountId };

        var categories = (await connection.QueryAsync<CategorySpendReport>(categorySql, args)).ToList();
        var transactions = (await connection.QueryAsync<TransactionRow>(txSql, args)).ToList();

        var total = categories.Sum(c => c.TotalSpent);
        if (total > 0)
            categories.ForEach(c => c.Percentage = Math.Round(c.TotalSpent / total * 100, 1));

        return Ok(new TransactionRangeResult
        {
            Categories = categories,
            Transactions = transactions,
            TotalSpent = total
        });
    }
}
