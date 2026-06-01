using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Application.Interfaces;

public interface IAuditoriaRepository
{
    Task AddAsync(Auditoria auditoria);
    Task<IEnumerable<Auditoria>> GetByCompanyAsync(Guid companyId, DateTime? from = null, DateTime? to = null);
    Task<bool> SaveChangesAsync();
}
