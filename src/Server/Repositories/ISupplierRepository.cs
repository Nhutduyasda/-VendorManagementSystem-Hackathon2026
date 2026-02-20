using VendorManagementSystem.Shared.DTOs.Vendors;
using VendorManagementSystem.Shared.Enums;

namespace VendorManagementSystem.Server.Repositories;

public interface ISupplierRepository
{
    Task<SupplierListResponse> GetSuppliersAsync(int page = 1, int pageSize = 10, string? search = null, SupplierStatus? status = null, SupplierRank? rank = null, bool? isBlacklisted = null);
    Task<SupplierDto?> GetByIdAsync(int id);
    Task<SupplierOverviewDto?> GetOverviewAsync(int id);
    Task<SupplierDto> CreateAsync(CreateSupplierRequest request);
    Task<SupplierDto?> UpdateAsync(int id, UpdateSupplierRequest request);
    Task<bool> DeleteAsync(int id);
    Task<SupplierDto?> BlacklistAsync(int id, string reason);
    Task<SupplierDto?> UnblacklistAsync(int id);
    Task<bool> LinkProductsAsync(int supplierId, List<SupplierProductItemDto> products);
    Task<List<SupplierDto>> ImportFromExcelAsync(Stream stream);
    Task<byte[]> ExportToExcelAsync(string? search = null, SupplierStatus? status = null);
    Task<string?> UploadLogoAsync(int supplierId, Stream stream, string fileName, string contentType);
}
