using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Application.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetProductByIdAsync(Guid id);
    Task<Product?> GetProductByCodeAsync(string code, Guid companyId);
    Task AddAsync(Product product);
    Task UpdateAsync(Product product);
    Task<bool> SaveChangeAsync();
}
