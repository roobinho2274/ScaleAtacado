using Microsoft.EntityFrameworkCore;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Infrastructure.Persistence;

namespace ScaleAtacado.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(Guid id, Guid companyId)
        => await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);

    public async Task<Category?> GetByNameAsync(string name, Guid companyId)
        => await _context.Categories
            .FirstOrDefaultAsync(c => c.Name == name && c.CompanyId == companyId);

    public async Task<IEnumerable<Category>> GetAllAsync(Guid companyId)
        => await _context.Categories
            .Where(c => c.CompanyId == companyId)
            .OrderBy(c => c.Name)
            .ToListAsync();

    public async Task<bool> HasProductsAsync(Guid categoryId)
        => await _context.Products.AnyAsync(p => p.CategoryId == categoryId);

    public async Task AddAsync(Category category)
        => await _context.Categories.AddAsync(category);

    public Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangesAsync()
        => await _context.SaveChangesAsync() > 0;
}
