using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Blazor.Services;

public class CompanyService
{
    private readonly ApiHttpClient _api;
    public CompanyService(ApiHttpClient api) => _api = api;

    public Task<ApiResponse<CompanyResponseDto>?> GetAsync()
        => _api.GetAsync<CompanyResponseDto>("api/company");

    public Task<ApiResponse?> UpdateAsync(UpdateCompanyDto dto)
        => _api.PutAsync("api/company", dto);
}
