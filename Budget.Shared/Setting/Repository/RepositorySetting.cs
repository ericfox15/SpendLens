using Budget.Application.Contracts.Options;
using Budget.Shared.Setting.Model;
using Dapper;
using Microsoft.Extensions.Options;

namespace Budget.Shared.Setting.Repository;

public class RepositorySetting(IOptions<RepositoryOptions> options)
    : FRepositoryBase(options.Value.BudgetDatabaseConnectionString), IRepositorySetting
{
    public async Task<IEnumerable<FSetting>> SelectAllSettingsAsync()
    {
        const string sql = "SELECT Path, Value FROM dbo.Setting";
        using var connection = GetConnection();
        return await connection.QueryAsync<FSetting>(sql);
    }

    public async Task UpsertSettingAsync(FSetting setting)
    {
        const string sql = """
            MERGE dbo.Setting AS target
            USING (SELECT @Path AS Path, @Value AS Value) AS source
            ON target.Path = source.Path
            WHEN MATCHED THEN UPDATE SET Value = source.Value
            WHEN NOT MATCHED THEN INSERT (Path, Value) VALUES (source.Path, source.Value);
            """;
        using var connection = GetConnection();
        await connection.ExecuteAsync(sql, setting);
    }
}
