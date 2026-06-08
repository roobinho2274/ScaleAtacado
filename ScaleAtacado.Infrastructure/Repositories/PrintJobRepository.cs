using Microsoft.EntityFrameworkCore;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Domain.Enums;
using ScaleAtacado.Infrastructure.Persistence;

namespace ScaleAtacado.Infrastructure.Repositories;

public class PrintJobRepository : IPrintJobRepository
{
    private readonly AppDbContext _context;

    public PrintJobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PrintJobs?> GetByIdAsync(Guid id)
        => await _context.PrintJobs.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<PrintJobs>> GetPendingAsync()
        => await _context.PrintJobs
            .Where(p => p.Status == PrintStatus.OnHoldem)
            .OrderBy(p => p.OnCreated)
            .ToListAsync();

    public async Task<IEnumerable<PrintJobs>> GetFailedAsync()
        => await _context.PrintJobs
            .Where(p => p.Status == PrintStatus.Canceled && p.TryCount < 3)
            .OrderBy(p => p.OnCreated)
            .ToListAsync();

    public async Task AddAsync(PrintJobs printJob)
        => await _context.PrintJobs.AddAsync(printJob);

    public Task UpdateAsync(PrintJobs printJob)
    {
        _context.PrintJobs.Update(printJob);
        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangesAsync()
        => await _context.SaveChangesAsync() > 0;
}
