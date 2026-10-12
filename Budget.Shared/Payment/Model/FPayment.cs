using Budget.Application.Contracts.Enums;

namespace Budget.Shared.Payment.Model;

/// <summary>
/// One due-instance of a bill (or an ad-hoc expense). Mirrors an Administration in eMAR.
/// BillId is NULL for ad-hoc payments (like PRN medications).
/// Temp negative IDs are used by the frontend before the server assigns real ones.
/// </summary>
public class FPayment
{
    public int PaymentId { get; set; }
    public int? BillId { get; set; }
    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public DateTimeOffset? DueAtUTC { get; set; }
    public DateTimeOffset? PaidAtUTC { get; set; }
    public decimal? Amount { get; set; }
    public EPaymentStatus Status { get; set; }
    public string? Note { get; set; }
    public int ModifiedBy { get; set; }
    public DateTimeOffset ModifiedUTC { get; set; }

    // Joined display fields
    public string? BillName { get; set; }
    public string? CategoryName { get; set; }
    public decimal? ExpectedAmount { get; set; }
    public bool IsAutoPay { get; set; }
}
