using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace ScaleAtacado.Blazor.Auth;

public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _js;
    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public JwtAuthStateProvider(IJSRuntime js) => _js = js;

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
                return Anonymous;

            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            return Anonymous;
        }
    }

    public async Task NotifyLoginAsync(string token)
    {
        await _js.InvokeVoidAsync("sessionStorage.setItem", "authToken", token);
        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt");
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
    }

    public async Task NotifyLogoutAsync()
    {
        await _js.InvokeVoidAsync("sessionStorage.removeItem", "authToken");
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    public async Task<string?> GetTokenAsync()
        => await _js.InvokeAsync<string?>("sessionStorage.getItem", "authToken");

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = ParseBase64(payload);
        var kvs = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes)!;

        return kvs.Select(kvp => new Claim(
            kvp.Key == "role" ? ClaimTypes.Role : kvp.Key,
            kvp.Value.ValueKind == JsonValueKind.String
                ? kvp.Value.GetString()!
                : kvp.Value.ToString()));
    }

    private static byte[] ParseBase64(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        return Convert.FromBase64String(base64);
    }
}
