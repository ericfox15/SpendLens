using Budget.Application.Contracts.Enums;

namespace Budget.Shared.Bill.Model;

/// <summary>
/// A recurring bill (like Netflix, Rent, Car Payment). Analogous to a Medication Order in eMAR.
/// ScheduleType + DayOfMonth/AnchorDateUTC drive when Payment rows are generated.
/// </summary>
public class FBill
{
    public int BillId { get; set; }
    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal ExpectedAmount { get; set; }
    public EScheduleType ScheduleType { get; set; }
    public byte? DayOfMonth { get; set; }
    public DateTimeOffset? AnchorDateUTC { get; set; }
    public bool IsAutoPay { get; set; }
    public bool IsActive { get; set; } = true;

    // Joined for display — not stored on the Bill row
    public string? AccountName { get; set; }
    public string? CategoryName { get; set; }
}
