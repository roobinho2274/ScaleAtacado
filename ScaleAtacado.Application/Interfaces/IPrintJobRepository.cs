using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Domain.Enums;

namespace ScaleAtacado.Application.Interfaces;

public interface IPrintJobRepository
{
    Task<PrintJobs?> GetByIdAsync(Guid id);
    Task<IEnumerable<PrintJobs>> GetPendingAsync();
    Task<IEnumerable<PrintJobs>> GetFailedAsync();
    Task AddAsync(PrintJobs printJob);
    Task UpdateAsync(PrintJobs printJob);
    Task<bool> SaveChangesAsync();
}
