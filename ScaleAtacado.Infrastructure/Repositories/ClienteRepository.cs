using Microsoft.EntityFrameworkCore;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Infrastructure.Persistence;

namespace ScaleAtacado.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly AppDbContext _context;

    public CustomerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Customer?> GetByIdAsync(Guid id, Guid companyId)
        => await _context.Customers.FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);

    public async Task<Customer?> GetByTaxIdAsync(string taxId, Guid companyId)
        => await _context.Customers.FirstOrDefaultAsync(c => c.TaxId == taxId && c.CompanyId == companyId);

    public async Task<IEnumerable<Customer>> SearchAsync(string term, Guid companyId)
        => await _context.Customers
            .Where(c => c.CompanyId == companyId && c.IsActive &&
                       (c.LegalName.Contains(term) ||
                        c.TaxId.Contains(term) ||
                        c.Phone.Contains(term)))
            .OrderBy(c => c.LegalName)
            .ToListAsync();

    public async Task<IEnumerable<Customer>> GetAllActiveAsync(Guid companyId)
        => await _context.Customers
            .Where(c => c.CompanyId == companyId && c.IsActive)
            .OrderBy(c => c.LegalName)
            .ToListAsync();

    public async Task AddAsync(Customer customer)
        => await _context.Customers.AddAsync(customer);

    public Task UpdateAsync(Customer customer)
    {
        _context.Customers.Update(customer);
        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangesAsync()
        => await _context.SaveChangesAsync() > 0;
}
