using Budget.Application.Contracts.Enums;
using Budget.Application.Contracts.Options;
using Budget.Shared.User.Model;
using Dapper;
using Microsoft.Extensions.Options;

namespace Budget.Shared.User.Repository;

public class RepositoryUser(IOptions<RepositoryOptions> options)
    : FRepositoryBase(options.Value.BudgetDatabaseConnectionString), IRepositoryUser
{
    public async Task<FUser?> SelectUserByUsernameAsync(string username)
    {
        const string sql = "SELECT UserId, Username, PasswordHash FROM dbo.[User] WHERE Username = @Username";
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<FUser>(sql, new { Username = username });
    }

    public async Task<FUser?> SelectUserByIdAsync(int userId)
    {
        const string sql = "SELECT UserId, Username, PasswordHash FROM dbo.[User] WHERE UserId = @UserId";
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<FUser>(sql, new { UserId = userId });
    }

    public async Task<int> InsertUserAsync(FUser user)
    {
        const string sql = """
            INSERT INTO dbo.[User] (Username, PasswordHash)
            OUTPUT INSERTED.UserId
            VALUES (@Username, @PasswordHash)
            """;
        using var connection = GetConnection();
        return await connection.ExecuteScalarAsync<int>(sql, user);
    }

    public async Task<IEnumerable<EPermission>> SelectPermissionsAsync(int userId)
    {
        const string sql = "SELECT Permission FROM dbo.Permission WHERE UserId = @UserId";
        using var connection = GetConnection();
        return await connection.QueryAsync<EPermission>(sql, new { UserId = userId });
    }

    public async Task InsertPermissionAsync(int userId, EPermission permission)
    {
        const string sql = """
            IF NOT EXISTS (SELECT 1 FROM dbo.Permission WHERE UserId = @UserId AND Permission = @Permission)
                INSERT INTO dbo.Permission (UserId, Permission) VALUES (@UserId, @Permission)
            """;
        using var connection = GetConnection();
        await connection.ExecuteAsync(sql, new { UserId = userId, Permission = (long)permission });
    }
}
