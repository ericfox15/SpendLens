using Budget.Application.Contracts.Enums;

namespace Budget.Pass.Submit.Model;

/// <summary>
/// Body for POST api/pass/submit/payment.
/// IsOfflineSync = true when the frontend is draining its Dexie queue (offline → back online).
/// In that mode, conflicts write to PaymentHistory instead of overwriting.
/// </summary>
public class SubmitPaymentRequest
{
    public List<SubmitPayment> Payments { get; set; } = new();
    public bool IsOfflineSync { get; set; }
}

public class SubmitPayment
{
    public SubmitPaymentStandard Standard { get; set; } = new();
}

public class SubmitPaymentStandard
{
    /// <summary>Positive = existing DB row. Negative = temp ID assigned by the frontend store.</summary>
    public int PaymentId { get; set; }
    public int BillId { get; set; }
    public int AccountId { get; set; }
    public int CategoryId { get; set; }
    public DateTimeOffset? DueAtUTC { get; set; }
    public DateTimeOffset? PaidAtUTC { get; set; }
    public decimal? Amount { get; set; }
    public EPaymentStatus Status { get; set; }
    public string? Note { get; set; }
    public int ModifiedBy { get; set; }
}

/// <summary>
/// Returned by submit endpoint. Maps temp frontend IDs to the real DB IDs.
/// The frontend dispatches UpdatePaymentTempIdAction with these bindings.
/// </summary>
public class PaymentBindingId
{
    public PaymentBindingId(int tempId, int id)
    {
        TempId = tempId;
        Id = id;
    }

    public int TempId { get; set; }
    public int Id { get; set; }
}
