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

    public async Task<Product?> GetProductByIdAsync(Guid id, Guid companyId)
        => await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

    public async Task<Product?> GetProductByCodeAsync(string code, Guid companyId)
        => await _context.Products
            .FirstOrDefaultAsync(p => p.Code == code && p.CompanyId == companyId);

    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetAllAsync(
        Guid companyId, int page, int pageSize, string? search, bool? isActive, Guid? categoryId = null)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Where(p => p.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search) || (p.Code != null && p.Code.Contains(search)));

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        if (categoryId.HasValue)
            query = query.Where(p => p.CategoryId == categoryId.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IEnumerable<Product>> GetAllActiveAsync(Guid companyId)
        => await _context.Products
            .Where(p => p.CompanyId == companyId && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

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
