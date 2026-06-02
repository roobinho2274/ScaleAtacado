using Microsoft.EntityFrameworkCore;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Infrastructure.Persistence;

namespace ScaleAtacado.Infrastructure.Repositories;

public class PaymentMethodRepository : IPaymentMethodRepository
{
    private readonly AppDbContext _context;

    public PaymentMethodRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentMethod?> GetByIdAsync(Guid id, Guid companyId)
        => await _context.PaymentMethods
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

    public async Task<PaymentMethod?> GetByNameAsync(string name, Guid companyId)
        => await _context.PaymentMethods
            .FirstOrDefaultAsync(p => p.Name == name && p.CompanyId == companyId);

    public async Task<IEnumerable<PaymentMethod>> GetAllActiveAsync(Guid companyId)
        => await _context.PaymentMethods
            .Where(p => p.CompanyId == companyId && p.IsActive)
            .OrderBy(p => p.DeadlineDays)
            .ToListAsync();

    public async Task<IEnumerable<PaymentMethod>> GetAllAsync(Guid companyId)
        => await _context.PaymentMethods
            .Where(p => p.CompanyId == companyId)
            .OrderBy(p => p.DeadlineDays)
            .ToListAsync();

    public async Task AddAsync(PaymentMethod paymentMethod)
        => await _context.PaymentMethods.AddAsync(paymentMethod);

    public Task UpdateAsync(PaymentMethod paymentMethod)
    {
        _context.PaymentMethods.Update(paymentMethod);
        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangesAsync()
        => await _context.SaveChangesAsync() > 0;
}
