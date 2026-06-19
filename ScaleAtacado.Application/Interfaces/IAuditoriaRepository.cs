using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Application.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog auditLog);
    Task<IEnumerable<AuditLog>> GetByCompanyAsync(Guid companyId, DateTime? from = null, DateTime? to = null);
    Task<bool> SaveChangesAsync();
}
