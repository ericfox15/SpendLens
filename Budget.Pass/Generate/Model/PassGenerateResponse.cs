using Budget.Shared.Account.Model;
using Budget.Shared.Bill.Model;
using Budget.Shared.Payment.Model;

namespace Budget.Pass.Generate.Model;

/// <summary>
/// Payload returned by pass/generate. Split into Standard/Extra/Optional to mirror eMAR's
/// medpass response structure — the frontend maps each section to a different NGXS state slice.
/// </summary>
public class PassGenerateResponse
{
    public PassStandard Standard { get; set; } = new();
    public PassExtra Extra { get; set; } = new();
    public PassOptional Optional { get; set; } = new();
}

public class PassStandard
{
    public IEnumerable<FAccount> Accounts { get; set; } = Enumerable.Empty<FAccount>();
    public IEnumerable<FBill> Bills { get; set; } = Enumerable.Empty<FBill>();
    public IEnumerable<FPayment> Payments { get; set; } = Enumerable.Empty<FPayment>();
}

public class PassExtra
{
    public IEnumerable<AdherenceFlag> AdherenceFlags { get; set; } = Enumerable.Empty<AdherenceFlag>();
    public int OverdueCount { get; set; }
}

public class PassOptional
{
    public IEnumerable<string> Notes { get; set; } = Enumerable.Empty<string>();
}

/// <summary>Mirrors eMAR's compliance flag — which bill has been skipped too many times.</summary>
public class AdherenceFlag
{
    public int BillId { get; set; }
    public string BillName { get; set; } = string.Empty;
    public int SkippedCount { get; set; }
}
