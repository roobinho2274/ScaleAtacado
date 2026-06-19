using Microsoft.EntityFrameworkCore;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Infrastructure.Persistence;

namespace ScaleAtacado.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog auditLog)
        => await _context.AuditLogs.AddAsync(auditLog);

    public async Task<IEnumerable<AuditLog>> GetByCompanyAsync(Guid companyId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.AuditLogs.Where(a => a.CompanyId == companyId);

        if (from.HasValue)
            query = query.Where(a => a.Timestamp >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Timestamp <= to.Value);

        return await query.OrderByDescending(a => a.Timestamp).ToListAsync();
    }

    public async Task<bool> SaveChangesAsync()
        => await _context.SaveChangesAsync() > 0;
}
