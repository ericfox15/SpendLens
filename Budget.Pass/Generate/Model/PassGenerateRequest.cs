namespace Budget.Pass.Generate.Model;

/// <summary>
/// Input to POST api/pass/generate. StartUTC and EndUTC define the billing window.
/// Mirrors eMAR's medpass generate request (time window → show what's due).
/// </summary>
public class PassGenerateRequest
{
    public DateTimeOffset StartUTC { get; set; }
    public DateTimeOffset EndUTC { get; set; }
}
