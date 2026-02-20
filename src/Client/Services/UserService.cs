using System.Net.Http.Json;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.Users;

namespace VendorManagementSystem.Client.Services;

public interface IUserService
{
    Task<UserListResponse?> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null, string? role = null);
    Task<UserDto?> GetUserByIdAsync(string id);
    Task<ApiResponse<UserDto>?> CreateUserAsync(CreateUserRequest request);
    Task<ApiResponse<UserDto>?> UpdateUserAsync(string id, UpdateUserRequest request);
    Task<ApiResponse<bool>?> DeleteUserAsync(string id);
    Task<ApiResponse<bool>?> ToggleUserStatusAsync(string id);
    Task<List<SupplierLookupDto>> GetSuppliersForLookupAsync();
}

public class UserService : IUserService
{
    private readonly HttpClient _http;

    public UserService(HttpClient http) => _http = http;

    public async Task<UserListResponse?> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null, string? role = null)
    {
        var url = $"api/users?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";
        if (!string.IsNullOrWhiteSpace(role))
            url += $"&role={Uri.EscapeDataString(role)}";

        var response = await _http.GetFromJsonAsync<ApiResponse<UserListResponse>>(url);
        return response?.Data;
    }

    public async Task<UserDto?> GetUserByIdAsync(string id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<UserDto>>($"api/users/{id}");
        return response?.Data;
    }

    public async Task<ApiResponse<UserDto>?> CreateUserAsync(CreateUserRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/users", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
    }

    public async Task<ApiResponse<UserDto>?> UpdateUserAsync(string id, UpdateUserRequest request)
    {
        var response = await _http.PutAsJsonAsync($"api/users/{id}", request);
        return await response.Content.ReadFromJsonAsync<ApiResponse<UserDto>>();
    }

    public async Task<ApiResponse<bool>?> DeleteUserAsync(string id)
    {
        var response = await _http.DeleteAsync($"api/users/{id}");
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    public async Task<ApiResponse<bool>?> ToggleUserStatusAsync(string id)
    {
        var response = await _http.PostAsync($"api/users/{id}/toggle-status", null);
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
    }

    public async Task<List<SupplierLookupDto>> GetSuppliersForLookupAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<SupplierLookupDto>>>("api/users/suppliers-lookup");
        return response?.Data ?? new List<SupplierLookupDto>();
    }
}
