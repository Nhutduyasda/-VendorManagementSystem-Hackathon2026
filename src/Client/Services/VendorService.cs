using System.Net.Http.Json;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.PurchaseOrders;
using VendorManagementSystem.Shared.DTOs.Vendors;

namespace VendorManagementSystem.Client.Services;

public interface IVendorService
{
    Task<VendorDashboardDto?> GetDashboardAsync();
    Task<List<PurchaseOrderDto>> GetPurchaseOrdersAsync();
    Task<SupplierDto?> GetMySupplierInfoAsync();
}

public class VendorService : IVendorService
{
    private readonly HttpClient _http;

    public VendorService(HttpClient http)
    {
        _http = http;
    }

    public async Task<VendorDashboardDto?> GetDashboardAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<VendorDashboardDto>>("api/vendor/dashboard");
        return response?.Data;
    }

    public async Task<List<PurchaseOrderDto>> GetPurchaseOrdersAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<PurchaseOrderDto>>>("api/vendor/purchase-orders");
        return response?.Data ?? new List<PurchaseOrderDto>();
    }

    public async Task<SupplierDto?> GetMySupplierInfoAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<SupplierDto>>("api/vendor/my-supplier-info");
            return response?.Data;
        }
        catch
        {
            return null;
        }
    }
}
