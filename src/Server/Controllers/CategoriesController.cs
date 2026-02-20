using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VendorManagementSystem.Server.Repositories;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.Products;

namespace VendorManagementSystem.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoriesController(ICategoryRepository categoryRepository) => _categoryRepository = categoryRepository;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories()
    {
        var result = await _categoryRepository.GetAllCategoriesAsync();
        return Ok(ApiResponse<List<CategoryDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        var category = await _categoryRepository.CreateCategoryAsync(request);
        return Ok(ApiResponse<CategoryDto>.Ok(category));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(int id, [FromBody] CreateCategoryRequest request)
    {
        var category = await _categoryRepository.UpdateCategoryAsync(id, request);
        if (category == null)
            return NotFound(ApiResponse<CategoryDto>.Fail("Category not found"));

        return Ok(ApiResponse<CategoryDto>.Ok(category));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCategory(int id)
    {
        try
        {
            var result = await _categoryRepository.DeleteCategoryAsync(id);
            if (!result)
                return NotFound(ApiResponse<bool>.Fail("Category not found"));

            return Ok(ApiResponse<bool>.Ok(true, "Category deleted"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.Fail(ex.Message));
        }
    }
}
