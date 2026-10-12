namespace Budget.Application.Contracts.Enums;

/// <summary>Maps to Payment.Status column. Mirrors EMAr's administration status enum.</summary>
public enum EPaymentStatus : byte
{
    Pending = 0,
    Paid    = 1,
    Skipped = 2
}
