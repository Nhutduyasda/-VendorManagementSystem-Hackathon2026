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

    private const string TokenKey = "authToken";
    private const string RefreshTokenKey = "refreshToken";

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
            await StoreTokens(result.Data);
            ((JwtAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Data.Token);
            SetAuthHeader(result.Data.Token);
        }

        return result?.Data ?? new AuthResponse { Errors = ["Login failed"] };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/auth/register", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();

        if (result?.Success == true && result.Data?.Token != null)
        {
            await StoreTokens(result.Data);
            ((JwtAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Data.Token);
            SetAuthHeader(result.Data.Token);
        }

        return result?.Data ?? new AuthResponse { Errors = ["Registration failed"] };
    }

    public async Task LogoutAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                SetAuthHeader(token);
                await _http.PostAsync("api/auth/logout", null);
            }
        }
        catch { }

        await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        await _js.InvokeVoidAsync("localStorage.removeItem", RefreshTokenKey);
        ((JwtAuthStateProvider)_authStateProvider).NotifyUserLogout();
        _http.DefaultRequestHeaders.Authorization = null;
    }

    public async Task<string?> GetTokenAsync()
    {
        var token = await _js.InvokeAsync<string?>("localStorage.getItem", TokenKey);
        if (string.IsNullOrWhiteSpace(token))
            return null;

        if (!JwtAuthStateProvider.IsTokenExpired(token))
            return token;

        var refreshToken = await _js.InvokeAsync<string?>("localStorage.getItem", RefreshTokenKey);
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var refreshRequest = new RefreshTokenRequest { Token = token, RefreshToken = refreshToken };
        var response = await _http.PostAsJsonAsync("api/auth/refresh-token", refreshRequest);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthResponse>>();

        if (result?.Success == true && result.Data?.Token != null)
        {
            await StoreTokens(result.Data);
            ((JwtAuthStateProvider)_authStateProvider).NotifyUserAuthentication(result.Data.Token);
            SetAuthHeader(result.Data.Token);
            return result.Data.Token;
        }

        await LogoutAsync();
        return null;
    }

    private async Task StoreTokens(AuthResponse authResponse)
    {
        await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, authResponse.Token);
        if (!string.IsNullOrEmpty(authResponse.RefreshToken))
            await _js.InvokeVoidAsync("localStorage.setItem", RefreshTokenKey, authResponse.RefreshToken);
    }

    private void SetAuthHeader(string token)
    {
        _http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}
