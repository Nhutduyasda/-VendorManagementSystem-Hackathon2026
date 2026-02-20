using VendorManagementSystem.Shared.DTOs.Products;
using VendorManagementSystem.Shared.DTOs.Vendors;

namespace VendorManagementSystem.Server.Repositories;

public interface IPriceListRepository
{
    Task<List<PriceListDto>> GetPriceHistoryAsync(int supplierId, int? productId = null);
    Task<PriceListDto> AddPriceAsync(int supplierId, int productId, decimal price);
    Task<List<PriceComparisonDto>> GetPriceComparisonAsync(int? productId = null);
}
