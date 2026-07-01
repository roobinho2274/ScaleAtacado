using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetProductByIdAsync(Guid id, Guid companyId);
    Task<Product?> GetProductByCodeAsync(string code, Guid companyId);
    Task<(IEnumerable<Product> Items, int TotalCount)> GetAllAsync(Guid companyId, int page, int pageSize, string? search, bool? isActive, Guid? categoryId = null);
    Task<IEnumerable<Product>> GetAllActiveAsync(Guid companyId);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task<bool> SaveChangeAsync();
}
