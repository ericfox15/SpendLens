using Budget.Shared.Category.Model;

namespace Budget.Shared.Category.Repository;

public interface IRepositoryCategory
{
    Task<IEnumerable<FCategory>> SelectCategoriesAsync();
    Task<FCategory?> SelectCategoryByIdAsync(int categoryId);
    Task<int> InsertCategoryAsync(FCategory category);
    Task UpdateCategoryAsync(FCategory category);
    Task DeleteCategoryAsync(int categoryId);
}
