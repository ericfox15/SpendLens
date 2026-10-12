using System.ComponentModel.DataAnnotations;

namespace Budget.Application.Contracts.Options;

/// <summary>
/// Strongly-typed options bound from appsettings.json "ConnectionStrings" section.
/// Validated at startup so the app fails fast if the connection string is missing.
/// </summary>
public class RepositoryOptions
{
    [Required]
    public string BudgetDatabaseConnectionString { get; set; } = string.Empty;
}
