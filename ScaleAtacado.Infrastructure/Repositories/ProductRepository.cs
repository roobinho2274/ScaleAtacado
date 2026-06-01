using Microsoft.EntityFrameworkCore;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Infrastructure.Persistence;

namespace ScaleAtacado.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Product?> GetProductByIdAsync(Guid id)
        => await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Product?> GetProductByCodeAsync(string code, Guid companyId)
        => await _context.Products.FirstOrDefaultAsync(p => p.Code == code && p.CompanyId == companyId);

    public async Task AddAsync(Product product)
        => await _context.Products.AddAsync(product);

    public Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangeAsync()
        => await _context.SaveChangesAsync() > 0;
}
