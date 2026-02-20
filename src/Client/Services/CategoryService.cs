using System.Net.Http.Json;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.Products;

namespace VendorManagementSystem.Client.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetCategoriesAsync();
    Task<ApiResponse<CategoryDto>?> CreateCategoryAsync(CreateCategoryRequest request);
    Task<ApiResponse<CategoryDto>?> UpdateCategoryAsync(int id, CreateCategoryRequest request);
    Task<ApiResponse<bool>?> DeleteCategoryAsync(int id);
}

public class CategoryService : ICategoryService
{
    private readonly HttpClient _http;

    public CategoryService(HttpClient http) => _http = http;

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<CategoryDto>>>("api/categories");
        return response?.Data ?? [];
    }

    public async Task<ApiResponse<CategoryDto>?> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/categories", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();
    }

    public async Task<ApiResponse<CategoryDto>?> UpdateCategoryAsync(int id, CreateCategoryRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/categories/{id}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();
    }

    public async Task<ApiResponse<bool>?> DeleteCategoryAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/categories/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }
}
