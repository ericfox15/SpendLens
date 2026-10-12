using Budget.Api.Budget.Attribute;
using Budget.Application.Contracts.Enums;
using Budget.Shared.Category.Model;
using Budget.Shared.Category.Repository;
using Budget.Shared.Category.Service;
using Microsoft.AspNetCore.Mvc;

namespace Budget.Api.Budget.Core;

[Route("api/category")]
public class ControllerCategory(IServiceCategory serviceCategory, IRepositoryCategory repositoryCategory) : FControllerBase
{
    [HttpGet("")]
    public async Task<IActionResult> GetCategories() => Ok(await repositoryCategory.SelectCategoriesAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var category = await repositoryCategory.SelectCategoryByIdAsync(id);
        return category == null ? NotFound() : Ok(category);
    }

    [HttpPost("")]
    [Permission(EPermission.Write)]
    public async Task<IActionResult> CreateCategory([FromBody] FCategory category)
    {
        var id = await serviceCategory.CreateCategoryAsync(category);
        return Ok(new { CategoryId = id });
    }

    [HttpPut("{id:int}")]
    [Permission(EPermission.Write)]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] FCategory category)
    {
        category.CategoryId = id;
        await serviceCategory.UpdateCategoryAsync(category);
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [Permission(EPermission.Write)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        await serviceCategory.DeleteCategoryAsync(id);
        return Ok();
    }
}
