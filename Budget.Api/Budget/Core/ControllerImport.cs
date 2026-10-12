using Budget.Application.Contracts.Options;
using Budget.Shared.Category.Repository;
using Budget.Shared.Payment.Model;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Budget.Api.Budget.Core;

public class ImportedTransaction
{
    public string Date { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool AutoCategorized { get; set; }
}

public class ImportResult
{
    public List<ImportedTransaction> Transactions { get; set; } = new();
    public List<CategorySpendReport> SpendByCategory { get; set; } = new();
    public int ImportedCount { get; set; }
    public int SkippedCount { get; set; }
}

/// <summary>
/// Accepts a CSV bank statement, auto-categorizes transactions by keyword matching,
/// stores them as ad-hoc payments, and returns a spending breakdown with percentages.
///
/// EXPECTED CSV FORMAT (most banks export this):
///   Date,Description,Amount  (header row required)
///   10/01/2024,NETFLIX,-15.99
///   10/02/2024,WHOLE FOODS,-82.45
///   Negative amounts = expenses, positive = income/deposits (skipped).
///
/// The user can also export from Chase, Bank of America, Wells Fargo — all use a similar format.
/// </summary>
[Route("api/import")]
public class ControllerImport(IOptions<RepositoryOptions> options, IRepositoryCategory repositoryCategory) : FControllerBase
{
    // Keyword → category name mapping. The user can add more in the Setting table later.
    private static readonly Dictionary<string, string> KeywordMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "netflix", "Entertainment" },
        { "hulu", "Entertainment" },
        { "spotify", "Entertainment" },
        { "amazon prime", "Entertainment" },
        { "disney", "Entertainment" },
        { "youtube", "Entertainment" },
        { "whole foods", "Groceries" },
        { "kroger", "Groceries" },
        { "walmart", "Groceries" },
        { "target", "Groceries" },
        { "trader joe", "Groceries" },
        { "safeway", "Groceries" },
        { "costco", "Groceries" },
        { "mcdonald", "Dining" },
        { "chipotle", "Dining" },
        { "starbucks", "Dining" },
        { "doordash", "Dining" },
        { "uber eats", "Dining" },
        { "grubhub", "Dining" },
        { "restaurant", "Dining" },
        { "shell", "Gas" },
        { "bp ", "Gas" },
        { "chevron", "Gas" },
        { "exxon", "Gas" },
        { "mobil", "Gas" },
        { "gas station", "Gas" },
        { "electric", "Utilities" },
        { "water bill", "Utilities" },
        { "internet", "Utilities" },
        { "comcast", "Utilities" },
        { "at&t", "Utilities" },
        { "verizon", "Utilities" },
        { "t-mobile", "Utilities" },
        { "rent", "Rent" },
        { "mortgage", "Rent" },
        { "pharmacy", "Health" },
        { "cvs", "Health" },
        { "walgreens", "Health" },
        { "doctor", "Health" },
        { "hospital", "Health" },
        { "gym", "Health" },
        { "uber", "Transportation" },
        { "lyft", "Transportation" },
        { "parking", "Transportation" },
        { "transit", "Transportation" }
    };

    [HttpPost("bank-statement")]
    public async Task<IActionResult> ImportBankStatement(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { Message = "Please upload a CSV file." });

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { Message = "File must be a .csv" });

        // Load categories from DB so we can match category names to IDs
        var categories = (await repositoryCategory.SelectCategoriesAsync()).ToList();

        var transactions = new List<ImportedTransaction>();
        var skipped = 0;

        using var reader = new StreamReader(file.OpenReadStream());
        var isFirstLine = true;
        string? line;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (isFirstLine) { isFirstLine = false; continue; } // skip header
            if (string.IsNullOrWhiteSpace(line)) continue;

            var cols = ParseCsvLine(line);
            if (cols.Length < 3) { skipped++; continue; }

            if (!decimal.TryParse(cols[2].Trim().Replace("$", ""), out var amount))
            {
                skipped++;
                continue;
            }

            // Skip income / deposits (positive amounts)
            if (amount >= 0) { skipped++; continue; }

            var description = cols[1].Trim();
            var category = AutoCategorize(description);
            var autoCategorized = category != "Uncategorized";

            transactions.Add(new ImportedTransaction
            {
                Date = cols[0].Trim(),
                Description = description,
                Amount = Math.Abs(amount),
                Category = category,
                AutoCategorized = autoCategorized
            });
        }

        // Persist as ad-hoc payments (BillId = NULL, like PRN in eMAR)
        var userIdClaim = HttpContext.User.FindFirst("sub")?.Value;
        int userId = userIdClaim != null && int.TryParse(userIdClaim, out var uid) ? uid : 0;

        int importedCount = 0;
        using var connection = new SqlConnection(options.Value.BudgetDatabaseConnectionString);

        foreach (var tx in transactions)
        {
            var categoryEntity = categories.FirstOrDefault(c =>
                string.Equals(c.Name, tx.Category, StringComparison.OrdinalIgnoreCase))
                ?? categories.FirstOrDefault(); // fallback to first category

            if (categoryEntity == null) continue;

            // Get first account — user can reassign later
            const string accountSql = "SELECT TOP 1 AccountId FROM dbo.Account WHERE IsActive = 1";
            var accountId = await connection.ExecuteScalarAsync<int>(accountSql);

            if (!DateTimeOffset.TryParse(tx.Date, out var parsedDate))
                parsedDate = DateTimeOffset.UtcNow;

            const string insertSql = """
                INSERT INTO dbo.Payment (BillId, AccountId, CategoryId, DueAtUTC, PaidAtUTC, Amount, Status, Note, ModifiedBy)
                VALUES (NULL, @AccountId, @CategoryId, @PaidAtUTC, @PaidAtUTC, @Amount, 1, @Note, @ModifiedBy)
                """;
            await connection.ExecuteAsync(insertSql, new
            {
                AccountId = accountId,
                CategoryId = categoryEntity.CategoryId,
                PaidAtUTC = parsedDate,
                Amount = tx.Amount,
                Note = $"Imported: {tx.Description}",
                ModifiedBy = userId
            });

            importedCount++;
        }

        // Build spend-by-category with percentages
        var grouped = transactions
            .GroupBy(t => t.Category)
            .Select(g => new CategorySpendReport
            {
                CategoryName = g.Key,
                TotalSpent = g.Sum(t => t.Amount),
                MonthlyLimit = categories.FirstOrDefault(c =>
                    string.Equals(c.Name, g.Key, StringComparison.OrdinalIgnoreCase))?.MonthlyLimit
            })
            .OrderByDescending(g => g.TotalSpent)
            .ToList();

        var total = grouped.Sum(g => g.TotalSpent);
        if (total > 0)
            grouped.ForEach(g => g.Percentage = Math.Round(g.TotalSpent / total * 100, 1));

        return Ok(new ImportResult
        {
            Transactions = transactions,
            SpendByCategory = grouped,
            ImportedCount = importedCount,
            SkippedCount = skipped
        });
    }

    private static string AutoCategorize(string description)
    {
        foreach (var (keyword, category) in KeywordMap)
            if (description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                return category;
        return "Uncategorized";
    }

    private static string[] ParseCsvLine(string line)
    {
        // Handles quoted fields (e.g. "WHOLE FOODS, MARKET" → single field)
        var result = new List<string>();
        var inQuotes = false;
        var current = new System.Text.StringBuilder();
        foreach (var ch in line)
        {
            if (ch == '"') { inQuotes = !inQuotes; continue; }
            if (ch == ',' && !inQuotes) { result.Add(current.ToString()); current.Clear(); continue; }
            current.Append(ch);
        }
        result.Add(current.ToString());
        return result.ToArray();
    }
}
