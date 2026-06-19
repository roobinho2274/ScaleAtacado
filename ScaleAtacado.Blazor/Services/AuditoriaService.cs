using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Blazor.Services;

public class AuditLogService
{
    private readonly ApiHttpClient _api;
    public AuditLogService(ApiHttpClient api) => _api = api;

    public Task<ApiResponse<IEnumerable<AuditLogResponseDto>>?> GetAllAsync(
        DateTime? from = null, DateTime? to = null)
    {
        var q = "api/auditlog";
        var queryParams = new List<string>();
        if (from.HasValue) queryParams.Add($"from={from.Value:yyyy-MM-dd}");
        if (to.HasValue) queryParams.Add($"to={to.Value:yyyy-MM-dd}");
        if (queryParams.Any()) q += "?" + string.Join("&", queryParams);
        return _api.GetAsync<IEnumerable<AuditLogResponseDto>>(q);
    }
}
