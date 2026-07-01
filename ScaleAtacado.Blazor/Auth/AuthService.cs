using System.Net.Http.Json;
using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Blazor.Services;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Blazor.Auth;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly JwtAuthStateProvider _authState;
    private readonly ApiHttpClient _api;

    public AuthService(HttpClient http, JwtAuthStateProvider authState, ApiHttpClient api)
    {
        _http = http;
        _authState = authState;
        _api = api;
    }

    public async Task<ApiResponse<AuthTokenDto>?> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new LoginDto(email, password));
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<AuthTokenDto>>();

            if (result?.Success == true && result.Data?.Token != null)
                await _authState.NotifyLoginAsync(result.Data.Token);

            return result;
        }
        catch (Exception ex)
        {
            var msg = ex is HttpRequestException
                ? "Não foi possível conectar à API. Verifique se o servidor está rodando."
                : "Erro inesperado ao tentar fazer login.";
            return ApiResponse<AuthTokenDto>.Fail(msg);
        }
    }

    public async Task LogoutAsync()
    {
        // registra logoff na auditoria antes de remover o token (fire-and-forget: falha não bloqueia)
        await _api.PostAsync("api/auth/logout");
        await _authState.NotifyLogoutAsync();
    }
}
