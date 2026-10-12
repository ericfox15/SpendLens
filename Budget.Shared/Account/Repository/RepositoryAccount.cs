using Budget.Application.Contracts.Options;
using Budget.Shared.Account.Model;
using Dapper;
using Microsoft.Extensions.Options;

namespace Budget.Shared.Account.Repository;

/// <summary>
/// Dapper-based account repository. All SQL is inline strings — no stored procs for simple CRUD.
/// One connection per method (using var = auto-dispose). Matches eMAR's repository pattern exactly.
/// </summary>
public class RepositoryAccount(IOptions<RepositoryOptions> options)
    : FRepositoryBase(options.Value.BudgetDatabaseConnectionString), IRepositoryAccount
{
    public async Task<IEnumerable<FAccount>> SelectAccountsAsync()
    {
        const string sql = """
            SELECT AccountId, Name, AccountType, IsActive
            FROM dbo.Account
            WHERE IsActive = 1
            ORDER BY Name
            """;
        using var connection = GetConnection();
        return await connection.QueryAsync<FAccount>(sql);
    }

    public async Task<FAccount?> SelectAccountByIdAsync(int accountId)
    {
        const string sql = """
            SELECT AccountId, Name, AccountType, IsActive
            FROM dbo.Account
            WHERE AccountId = @AccountId
            """;
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<FAccount>(sql, new { AccountId = accountId });
    }

    public async Task<int> InsertAccountAsync(FAccount account)
    {
        const string sql = """
            INSERT INTO dbo.Account (Name, AccountType, IsActive)
            OUTPUT INSERTED.AccountId
            VALUES (@Name, @AccountType, @IsActive)
            """;
        using var connection = GetConnection();
        return await connection.ExecuteScalarAsync<int>(sql, account);
    }

    public async Task UpdateAccountAsync(FAccount account)
    {
        const string sql = """
            UPDATE dbo.Account
            SET Name = @Name, AccountType = @AccountType
            WHERE AccountId = @AccountId
            """;
        using var connection = GetConnection();
        await connection.ExecuteAsync(sql, account);
    }

    public async Task DeleteAccountAsync(int accountId)
    {
        // Soft delete — never hard-delete financial data
        const string sql = "UPDATE dbo.Account SET IsActive = 0 WHERE AccountId = @AccountId";
        using var connection = GetConnection();
        await connection.ExecuteAsync(sql, new { AccountId = accountId });
    }
}
