using Budget.Application.Contracts.Options;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Budget.Api.Budget.Core;

public class CategorySpendReport
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public decimal? MonthlyLimit { get; set; }
    public decimal Percentage { get; set; }
}

/// <summary>
/// Spending report — category breakdown for a calendar month or a custom date range.
/// </summary>
[Route("api/report")]
public class ControllerReport(IOptions<RepositoryOptions> options) : FControllerBase
{
    [HttpGet("category-spend")]
    public async Task<IActionResult> GetCategorySpend([FromQuery] int year, [FromQuery] int month)
    {
        var start = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero);
        var end = start.AddMonths(1);
        return Ok(await QueryAsync(start, end));
    }

    [HttpGet("category-spend/range")]
    public async Task<IActionResult> GetCategorySpendRange(
        [FromQuery] DateTimeOffset start,
        [FromQuery] DateTimeOffset end)
        => Ok(await QueryAsync(start, end));

    private async Task<List<CategorySpendReport>> QueryAsync(DateTimeOffset start, DateTimeOffset end)
    {
        const string sql = """
            SELECT c.CategoryId, c.Name AS CategoryName,
                   ISNULL(SUM(p.Amount), 0) AS TotalSpent,
                   c.MonthlyLimit
            FROM dbo.Category c
            LEFT JOIN dbo.Payment p ON p.CategoryId = c.CategoryId
                AND p.PaidAtUTC >= @Start AND p.PaidAtUTC < @End
            GROUP BY c.CategoryId, c.Name, c.MonthlyLimit
            ORDER BY TotalSpent DESC
            """;

        using var connection = new SqlConnection(options.Value.BudgetDatabaseConnectionString);
        var rows = (await connection.QueryAsync<CategorySpendReport>(sql, new { Start = start, End = end })).ToList();

        var total = rows.Sum(r => r.TotalSpent);
        if (total > 0)
            rows.ForEach(r => r.Percentage = Math.Round(r.TotalSpent / total * 100, 1));

        return rows;
    }
}
