using System.Net.Http.Headers;
using System.Net.Http.Json;
using ScaleAtacado.Blazor.Auth;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Blazor.Services;

public class ApiHttpClient
{
    private readonly HttpClient _http;
    private readonly JwtAuthStateProvider _authState;

    public ApiHttpClient(HttpClient http, JwtAuthStateProvider authState)
    {
        _http = http;
        _authState = authState;
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _authState.GetTokenAsync();
        _http.DefaultRequestHeaders.Authorization =
            string.IsNullOrEmpty(token) ? null : new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<ApiResponse<T>?> GetAsync<T>(string url)
    {
        try
        {
            await SetAuthHeaderAsync();
            var r = await _http.GetAsync(url);
            return await r.Content.ReadFromJsonAsync<ApiResponse<T>>();
        }
        catch { return null; }
    }

    public async Task<ApiResponse<T>?> PostAsync<T>(string url, object body)
    {
        try
        {
            await SetAuthHeaderAsync();
            var r = await _http.PostAsJsonAsync(url, body);
            return await r.Content.ReadFromJsonAsync<ApiResponse<T>>();
        }
        catch { return null; }
    }

    public async Task<ApiResponse?> PostAsync(string url, object? body = null)
    {
        try
        {
            await SetAuthHeaderAsync();
            var r = await _http.PostAsJsonAsync(url, body ?? new { });
            return await r.Content.ReadFromJsonAsync<ApiResponse>();
        }
        catch { return null; }
    }

    public async Task<ApiResponse<T>?> PutAsync<T>(string url, object body)
    {
        try
        {
            await SetAuthHeaderAsync();
            var r = await _http.PutAsJsonAsync(url, body);
            return await r.Content.ReadFromJsonAsync<ApiResponse<T>>();
        }
        catch { return null; }
    }

    public async Task<ApiResponse?> PutAsync(string url, object body)
    {
        try
        {
            await SetAuthHeaderAsync();
            var r = await _http.PutAsJsonAsync(url, body);
            return await r.Content.ReadFromJsonAsync<ApiResponse>();
        }
        catch { return null; }
    }

    public async Task<ApiResponse<T>?> PatchAsync<T>(string url, object? body = null)
    {
        try
        {
            await SetAuthHeaderAsync();
            var r = await _http.PatchAsJsonAsync(url, body ?? new { });
            return await r.Content.ReadFromJsonAsync<ApiResponse<T>>();
        }
        catch { return null; }
    }

    public async Task<ApiResponse?> PatchAsync(string url, object body)
    {
        try
        {
            await SetAuthHeaderAsync();
            var r = await _http.PatchAsJsonAsync(url, body);
            return await r.Content.ReadFromJsonAsync<ApiResponse>();
        }
        catch { return null; }
    }

    public async Task<ApiResponse?> DeleteAsync(string url)
    {
        try
        {
            await SetAuthHeaderAsync();
            var r = await _http.DeleteAsync(url);
            return await r.Content.ReadFromJsonAsync<ApiResponse>();
        }
        catch { return null; }
    }
}
