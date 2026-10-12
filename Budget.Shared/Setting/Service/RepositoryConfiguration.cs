using Budget.Shared.Setting.Repository;

namespace Budget.Shared.Setting.Service;

/// <summary>
/// Loaded once at startup via GET api/core/initialization/settings. Provides typed getters
/// for DB-driven settings — mirrors eMAR's RepositoryConfiguration class exactly.
/// The frontend fetches this same data to configure action routes and UI behavior.
/// </summary>
public class RepositoryConfiguration(IRepositorySetting repositorySetting)
{
    private Dictionary<string, string> _settings = new();

    public async Task LoadAsync()
    {
        var rows = await repositorySetting.SelectAllSettingsAsync();
        _settings = rows.ToDictionary(s => s.Path, s => s.Value);
    }

    public bool IsAutoPayShown() => TryParseBool("Pass:ShowAutoPay", defaultValue: true);
    public int GetOverBudgetWarnPercent() => TryParseInt("Budget:OverBudgetWarnPercent", defaultValue: 100);
    public string GetActionRoutes() => _settings.TryGetValue("Pass:ActionRoutes", out var val) ? val : "{}";

    public Dictionary<string, string> GetAll() => new(_settings);

    private bool TryParseBool(string path, bool defaultValue = false)
        => _settings.TryGetValue(path, out var val) && bool.TryParse(val, out var result) ? result : defaultValue;

    private int TryParseInt(string path, int defaultValue = 0)
        => _settings.TryGetValue(path, out var val) && int.TryParse(val, out var result) ? result : defaultValue;
}
