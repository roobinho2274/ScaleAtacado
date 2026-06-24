using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Application.Services;

public class CompanyAppService
{
    private readonly ICompanyRepository _repo;

    public CompanyAppService(ICompanyRepository repo) => _repo = repo;

    public async Task<ApiResponse<CompanyResponseDto>> GetAsync(Guid companyId)
    {
        var company = await _repo.GetByIdAsync(companyId);
        if (company == null)
            return ApiResponse<CompanyResponseDto>.Fail("Empresa não encontrada.");

        return ApiResponse<CompanyResponseDto>.Ok(
            new CompanyResponseDto(company.Id, company.Name, company.CNPJ, company.Address, company.Phone, company.IsActive, company.LogoBase64));
    }

    public async Task<ApiResponse> UpdateAsync(Guid companyId, UpdateCompanyDto dto)
    {
        var company = await _repo.GetByIdAsync(companyId);
        if (company == null)
            return ApiResponse.Fail("Empresa não encontrada.");

        company.Name    = dto.Name;
        company.CNPJ    = dto.CNPJ;
        company.Address = dto.Address;
        company.Phone   = dto.Phone;

        await _repo.UpdateAsync(company);
        await _repo.SaveChangesAsync();

        return ApiResponse.Ok();
    }

    public async Task<ApiResponse> UpdateLogoAsync(Guid companyId, string? logoBase64)
    {
        var company = await _repo.GetByIdAsync(companyId);
        if (company == null)
            return ApiResponse.Fail("Empresa não encontrada.");

        company.LogoBase64 = logoBase64;
        await _repo.UpdateAsync(company);
        await _repo.SaveChangesAsync();

        return ApiResponse.Ok();
    }
}
