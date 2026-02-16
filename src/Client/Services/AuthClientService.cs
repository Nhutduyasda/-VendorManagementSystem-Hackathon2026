using System.Net.Http.Json;
using VendorManagementSystem.Shared.DTOs;
using VendorManagementSystem.Shared.DTOs.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace VendorManagementSystem.Client.Services;

public interface IAuthClientService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
}

public class AuthClientService : IAuthClientService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private readonly AuthenticationStateProvider _authStateProvider;

    private const string TokenKey = "auth_token";

    public AuthClientService(HttpClient http, IJSRuntime js, AuthenticationStateProvider authStateProvider)
    {
        _http = http;
        _js = js;
        _authStateProvider = authStateProvider;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();

        if (result?.Success == true && result.Data?.Token != null)
        {
            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, result.Data.Token);
            ((JwtAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Data.Token);
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Data.Token);
        }

        return result?.Data ?? new AuthResponse { Errors = ["Login failed"] };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();

        if (result?.Success == true && result.Data?.Token != null)
        {
            await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, result.Data.Token);
            ((JwtAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Data.Token);
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Data.Token);
        }

        return result?.Data ?? new AuthResponse { Errors = ["Registration failed"] };
    }

    public async Task LogoutAsync()
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        ((JwtAuthStateProvider)_authStateProvider).NotifyUserLogout();
        _http.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
    }
}
