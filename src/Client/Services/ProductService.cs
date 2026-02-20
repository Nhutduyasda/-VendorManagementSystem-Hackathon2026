using System.Net.Http.Json;
using System.Net.Http.Headers;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.Products;

namespace VendorManagementSystem.Client.Services;

public interface IProductService
{
    Task<ProductListResponse?> GetProductsAsync(int page = 1, int pageSize = 10, string? search = null, int? categoryId = null);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ApiResponse<ProductDto>?> CreateProductAsync(CreateProductRequest request);
    Task<ApiResponse<ProductDto>?> UpdateProductAsync(int id, UpdateProductRequest request);
    Task<ApiResponse<bool>?> DeleteProductAsync(int id);
    Task<ApiResponse<List<ProductDto>>?> ImportFromExcelAsync(Stream fileStream, string fileName);
    Task<ApiResponse<string>?> UploadImageAsync(int productId, Stream fileStream, string fileName, string contentType);
    Task<string> UploadProductImageAsync(int productId, Microsoft.AspNetCore.Components.Forms.IBrowserFile file);
}

public class ProductService : IProductService
{
    private readonly HttpClient _http;

    public ProductService(HttpClient http) => _http = http;

    public async Task<ProductListResponse?> GetProductsAsync(int page = 1, int pageSize = 10, string? search = null, int? categoryId = null)
    {
        var url = $"api/products?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";
        if (categoryId.HasValue && categoryId.Value > 0)
            url += $"&categoryId={categoryId.Value}";

        var response = await _http.GetFromJsonAsync<ApiResponse<ProductListResponse>>(url);
        return response?.Data;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<ProductDto>>($"api/products/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<ProductDto>?> CreateProductAsync(CreateProductRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/products", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
    }

    public async Task<ApiResponse<ProductDto>?> UpdateProductAsync(int id, UpdateProductRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/products/{id}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
    }

    public async Task<ApiResponse<bool>?> DeleteProductAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/products/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    public async Task<ApiResponse<List<ProductDto>>?> ImportFromExcelAsync(Stream fileStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(streamContent, "file", fileName);

        var response = await _http.PostAsync("api/products/import", content);
        return await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductDto>>>();
    }

    public async Task<ApiResponse<string>?> UploadImageAsync(int productId, Stream fileStream, string fileName, string contentType)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        var response = await _http.PostAsync($"api/products/{productId}/upload-image", content);
        return await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
    }
    public async Task<string> UploadProductImageAsync(int productId, Microsoft.AspNetCore.Components.Forms.IBrowserFile file)
    {
        try
        {
            var maxFileSize = 5 * 1024 * 1024; // 5MB
            using var stream = file.OpenReadStream(maxFileSize);
            var response = await UploadImageAsync(productId, stream, file.Name, file.ContentType);
            
            if (response != null && response.Success)
            {
                return response.Data!;
            }
            
            throw new Exception(response?.Message ?? "Upload failed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading image: {ex.Message}");
            return string.Empty;
        }
    }
}
