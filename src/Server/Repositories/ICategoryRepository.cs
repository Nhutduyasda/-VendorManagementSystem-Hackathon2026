using VendorManagementSystem.Shared.DTOs.Products;

namespace VendorManagementSystem.Server.Repositories;

public interface ICategoryRepository
{
    Task<List<CategoryDto>> GetAllCategoriesAsync();
    Task<CategoryDto?> GetCategoryByIdAsync(int id);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request);
    Task<CategoryDto?> UpdateCategoryAsync(int id, CreateCategoryRequest request);
    Task<bool> DeleteCategoryAsync(int id);
}
