using Budget.Shared.Setting.Model;

namespace Budget.Shared.Setting.Repository;

public interface IRepositorySetting
{
    Task<IEnumerable<FSetting>> SelectAllSettingsAsync();
    Task UpsertSettingAsync(FSetting setting);
}
