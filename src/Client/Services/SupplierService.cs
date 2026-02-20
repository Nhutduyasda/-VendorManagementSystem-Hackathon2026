using System.Net.Http.Headers;
using System.Net.Http.Json;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.Products;
using VendorManagementSystem.Shared.DTOs.Vendors;
using VendorManagementSystem.Shared.Enums;

namespace VendorManagementSystem.Client.Services;

public interface ISupplierService
{
    Task<SupplierListResponse?> GetSuppliersAsync(int page = 1, int pageSize = 10, string? search = null, SupplierStatus? status = null, SupplierRank? rank = null, bool? isBlacklisted = null);
    Task<SupplierDto?> GetByIdAsync(int id);
    Task<SupplierOverviewDto?> GetOverviewAsync(int id);
    Task<ApiResponse<SupplierDto>?> CreateAsync(CreateSupplierRequest request);
    Task<ApiResponse<SupplierDto>?> UpdateAsync(int id, UpdateSupplierRequest request);
    Task<ApiResponse<bool>?> DeleteAsync(int id);
    Task<ApiResponse<SupplierDto>?> BlacklistAsync(int id, BlacklistRequest request);
    Task<ApiResponse<SupplierDto>?> UnblacklistAsync(int id);
    Task<ApiResponse<bool>?> LinkProductsAsync(int id, LinkSupplierProductsRequest request);
    Task<List<PriceListDto>> GetPriceHistoryAsync(int supplierId, int? productId = null);
    Task<List<PriceComparisonDto>> GetPriceComparisonAsync(int? productId = null);
    Task<ApiResponse<string>?> UploadLogoAsync(int supplierId, Stream stream, string fileName, string contentType);
    Task<string> UploadSupplierLogoAsync(int supplierId, Microsoft.AspNetCore.Components.Forms.IBrowserFile file);
    Task<ApiResponse<List<SupplierDto>>?> ImportFromExcelAsync(Stream stream, string fileName);
    Task<byte[]?> ExportToExcelAsync(string? search = null, SupplierStatus? status = null);
}

public class SupplierService : ISupplierService
{
    private readonly HttpClient _http;

    public SupplierService(HttpClient http) => _http = http;

    public async Task<SupplierListResponse?> GetSuppliersAsync(int page = 1, int pageSize = 10, string? search = null, SupplierStatus? status = null, SupplierRank? rank = null, bool? isBlacklisted = null)
    {
        var url = $"api/suppliers?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";
        if (status.HasValue)
            url += $"&status={status.Value}";
        if (rank.HasValue)
            url += $"&rank={rank.Value}";
        if (isBlacklisted.HasValue)
            url += $"&isBlacklisted={isBlacklisted.Value}";

        var response = await _http.GetFromJsonAsync<ApiResponse<SupplierListResponse>>(url);
        return response?.Data;
    }

    public async Task<SupplierDto?> GetByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<SupplierDto>>($"api/suppliers/{id}");
        return response?.Data;
    }

    public async Task<SupplierOverviewDto?> GetOverviewAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<SupplierOverviewDto>>($"api/suppliers/{id}/overview");
        return response?.Data;
    }

    public async Task<ApiResponse<SupplierDto>?> CreateAsync(CreateSupplierRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/suppliers", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<SupplierDto>>();
    }

    public async Task<ApiResponse<SupplierDto>?> UpdateAsync(int id, UpdateSupplierRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/suppliers/{id}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<SupplierDto>>();
    }

    public async Task<ApiResponse<bool>?> DeleteAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/suppliers/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    public async Task<ApiResponse<SupplierDto>?> BlacklistAsync(int id, BlacklistRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/suppliers/{id}/blacklist", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<SupplierDto>>();
    }

    public async Task<ApiResponse<SupplierDto>?> UnblacklistAsync(int id)
    {
        var response = await _http.PostAsync($"api/suppliers/{id}/unblacklist", null);
        return await response.Content.ReadFromJsonAsync<ApiResponse<SupplierDto>>();
    }

    public async Task<ApiResponse<bool>?> LinkProductsAsync(int id, LinkSupplierProductsRequest request)
    {
        var response = await _http.PostAsJsonAsync($"api/suppliers/{id}/products", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    public async Task<List<PriceListDto>> GetPriceHistoryAsync(int supplierId, int? productId = null)
    {
        var url = $"api/suppliers/{supplierId}/prices";
        if (productId.HasValue)
            url += $"?productId={productId.Value}";

        var response = await _http.GetFromJsonAsync<ApiResponse<List<PriceListDto>>>(url);
        return response?.Data ?? [];
    }

    public async Task<List<PriceComparisonDto>> GetPriceComparisonAsync(int? productId = null)
    {
        var url = "api/suppliers/price-comparison";
        if (productId.HasValue)
            url += $"?productId={productId.Value}";

        var response = await _http.GetFromJsonAsync<ApiResponse<List<PriceComparisonDto>>>(url);
        return response?.Data ?? [];
    }

    public async Task<ApiResponse<string>?> UploadLogoAsync(int supplierId, Stream stream, string fileName, string contentType)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);

        var response = await _http.PostAsync($"api/suppliers/{supplierId}/upload-logo", content);
        return await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
    }

    public async Task<ApiResponse<List<SupplierDto>>?> ImportFromExcelAsync(Stream stream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(streamContent, "file", fileName);

        var response = await _http.PostAsync("api/suppliers/import", content);
        return await response.Content.ReadFromJsonAsync<ApiResponse<List<SupplierDto>>>();
    }

    public async Task<byte[]?> ExportToExcelAsync(string? search = null, SupplierStatus? status = null)
    {
        var url = "api/suppliers/export";
        var queryParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
            queryParts.Add($"search={Uri.EscapeDataString(search)}");
        if (status.HasValue)
            queryParts.Add($"status={status.Value}");
        if (queryParts.Any())
            url += "?" + string.Join("&", queryParts);

        var response = await _http.GetAsync(url);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadAsByteArrayAsync();
        return null;
    }
    public async Task<string> UploadSupplierLogoAsync(int supplierId, Microsoft.AspNetCore.Components.Forms.IBrowserFile file)
    {
        try
        {
             var maxFileSize = 5 * 1024 * 1024; // 5MB
             using var stream = file.OpenReadStream(maxFileSize);
             var response = await UploadLogoAsync(supplierId, stream, file.Name, file.ContentType);

             if (response != null && response.Success)
             {
                 return response.Data!;
             }

             throw new Exception(response?.Message ?? "Upload failed");
        }
        catch (Exception ex)
        {
             Console.WriteLine($"Error uploading logo: {ex.Message}");
             return string.Empty;
        }
    }
}
