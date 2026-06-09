using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Blazor.Services;

public class AuditoriaService
{
    private readonly ApiHttpClient _api;
    public AuditoriaService(ApiHttpClient api) => _api = api;

    public Task<ApiResponse<IEnumerable<AuditoriaResponseDto>>?> GetAllAsync(
        DateTime? from = null, DateTime? to = null)
    {
        var q = "api/auditoria";
        var params_ = new List<string>();
        if (from.HasValue) params_.Add($"from={from.Value:yyyy-MM-dd}");
        if (to.HasValue) params_.Add($"to={to.Value:yyyy-MM-dd}");
        if (params_.Any()) q += "?" + string.Join("&", params_);
        return _api.GetAsync<IEnumerable<AuditoriaResponseDto>>(q);
    }
}
