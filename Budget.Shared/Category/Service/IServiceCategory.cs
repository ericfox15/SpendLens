using Budget.Shared.Category.Model;

namespace Budget.Shared.Category.Service;

public interface IServiceCategory
{
    Task<IEnumerable<FCategory>> GetCategoriesAsync();
    Task<FCategory?> GetCategoryByIdAsync(int categoryId);
    Task<int> CreateCategoryAsync(FCategory category);
    Task UpdateCategoryAsync(FCategory category);
    Task DeleteCategoryAsync(int categoryId);
}
