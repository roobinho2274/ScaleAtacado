using System.Security.Claims;

namespace ScaleAtacado.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetCompanyId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue("companyId")!);

    public static Guid GetUserId(this ClaimsPrincipal user)
        => Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? user.FindFirstValue("sub")!);

    public static string GetProfile(this ClaimsPrincipal user)
        => user.FindFirstValue("profile") ?? string.Empty;

    public static string GetFullName(this ClaimsPrincipal user)
        => user.FindFirstValue("fullName") ?? user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
}
