using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Application.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(Guid id, Guid companyId);
    Task<Category?> GetByNameAsync(string name, Guid companyId);
    Task<IEnumerable<Category>> GetAllAsync(Guid companyId);
    Task<bool> HasProductsAsync(Guid categoryId);
    Task AddAsync(Category category);
    Task UpdateAsync(Category category);
    Task RemoveAsync(Category category);
    Task<bool> SaveChangesAsync();
}
