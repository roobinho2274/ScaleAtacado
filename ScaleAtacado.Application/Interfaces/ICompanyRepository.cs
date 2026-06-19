using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Application.Interfaces;

public interface ICompanyRepository
{
    Task<Company?> GetByIdAsync(Guid companyId);
    Task UpdateAsync(Company company);
    Task SaveChangesAsync();
}
