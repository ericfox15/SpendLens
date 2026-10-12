namespace Budget.Shared.Category.Model;

/// <summary>
/// A spending category (Rent, Food, Entertainment, etc.) with an optional monthly spending limit.
/// Maps to dbo.Category. When MonthlyLimit is set the app shows over-budget warnings.
/// </summary>
public class FCategory
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? MonthlyLimit { get; set; }
}
