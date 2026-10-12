using Budget.Application.Contracts.Enums;

namespace Budget.Shared.Account.Model;

/// <summary>
/// Represents a financial account (Checking, Credit Card, Cash).
/// Maps 1-to-1 with dbo.Account. The F* prefix follows eMAR's naming convention for core domain models.
/// </summary>
public class FAccount
{
    public int AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public EAccountType AccountType { get; set; }
    public bool IsActive { get; set; } = true;
}
