using VendorManagementSystem.Shared.DTOs.Products;

namespace VendorManagementSystem.Server.Repositories;

public interface IProductRepository
{
    Task<ProductListResponse> GetProductsAsync(int page, int pageSize, string? searchTerm, int? categoryId);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request);
    Task<ProductDto?> UpdateProductAsync(int id, UpdateProductRequest request);
    Task<bool> DeleteProductAsync(int id);
    Task<List<ProductDto>> ImportFromExcelAsync(Stream fileStream);
}
