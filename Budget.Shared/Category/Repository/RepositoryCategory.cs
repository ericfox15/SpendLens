using Budget.Application.Contracts.Options;
using Budget.Shared.Category.Model;
using Dapper;
using Microsoft.Extensions.Options;

namespace Budget.Shared.Category.Repository;

public class RepositoryCategory(IOptions<RepositoryOptions> options)
    : FRepositoryBase(options.Value.BudgetDatabaseConnectionString), IRepositoryCategory
{
    public async Task<IEnumerable<FCategory>> SelectCategoriesAsync()
    {
        const string sql = "SELECT CategoryId, Name, MonthlyLimit FROM dbo.Category ORDER BY Name";
        using var connection = GetConnection();
        return await connection.QueryAsync<FCategory>(sql);
    }

    public async Task<FCategory?> SelectCategoryByIdAsync(int categoryId)
    {
        const string sql = "SELECT CategoryId, Name, MonthlyLimit FROM dbo.Category WHERE CategoryId = @CategoryId";
        using var connection = GetConnection();
        return await connection.QueryFirstOrDefaultAsync<FCategory>(sql, new { CategoryId = categoryId });
    }

    public async Task<int> InsertCategoryAsync(FCategory category)
    {
        const string sql = """
            INSERT INTO dbo.Category (Name, MonthlyLimit)
            OUTPUT INSERTED.CategoryId
            VALUES (@Name, @MonthlyLimit)
            """;
        using var connection = GetConnection();
        return await connection.ExecuteScalarAsync<int>(sql, category);
    }

    public async Task UpdateCategoryAsync(FCategory category)
    {
        const string sql = "UPDATE dbo.Category SET Name = @Name, MonthlyLimit = @MonthlyLimit WHERE CategoryId = @CategoryId";
        using var connection = GetConnection();
        await connection.ExecuteAsync(sql, category);
    }

    public async Task DeleteCategoryAsync(int categoryId)
    {
        const string sql = "DELETE FROM dbo.Category WHERE CategoryId = @CategoryId";
        using var connection = GetConnection();
        await connection.ExecuteAsync(sql, new { CategoryId = categoryId });
    }
}
