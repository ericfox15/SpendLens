namespace Budget.Shared.Setting.Model;

/// <summary>
/// Key-value configuration row from dbo.Setting. Mirrors eMAR's Setting.General table.
/// Paths use colon notation: "Pass:ShowAutoPay", "Budget:OverBudgetWarnPercent".
/// </summary>
public class FSetting
{
    public string Path { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
