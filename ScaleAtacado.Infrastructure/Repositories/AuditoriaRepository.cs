using Microsoft.EntityFrameworkCore;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Infrastructure.Persistence;

namespace ScaleAtacado.Infrastructure.Repositories;

public class AuditoriaRepository : IAuditoriaRepository
{
    private readonly AppDbContext _context;

    public AuditoriaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Auditoria auditoria)
        => await _context.Auditorias.AddAsync(auditoria);

    public async Task<IEnumerable<Auditoria>> GetByCompanyAsync(Guid companyId, DateTime? from = null, DateTime? to = null)
    {
        var query = _context.Auditorias.Where(a => a.CompanyId == companyId);

        if (from.HasValue)
            query = query.Where(a => a.DataHora >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.DataHora <= to.Value);

        return await query.OrderByDescending(a => a.DataHora).ToListAsync();
    }

    public async Task<bool> SaveChangesAsync()
        => await _context.SaveChangesAsync() > 0;
}
