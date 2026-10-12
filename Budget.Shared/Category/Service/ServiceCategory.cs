using Budget.Shared.Category.Model;
using Budget.Shared.Category.Repository;

namespace Budget.Shared.Category.Service;

public class ServiceCategory(IRepositoryCategory repositoryCategory) : IServiceCategory
{
    public Task<IEnumerable<FCategory>> GetCategoriesAsync() => repositoryCategory.SelectCategoriesAsync();
    public Task<FCategory?> GetCategoryByIdAsync(int categoryId) => repositoryCategory.SelectCategoryByIdAsync(categoryId);
    public Task<int> CreateCategoryAsync(FCategory category) => repositoryCategory.InsertCategoryAsync(category);
    public Task UpdateCategoryAsync(FCategory category) => repositoryCategory.UpdateCategoryAsync(category);
    public Task DeleteCategoryAsync(int categoryId) => repositoryCategory.DeleteCategoryAsync(categoryId);
}
