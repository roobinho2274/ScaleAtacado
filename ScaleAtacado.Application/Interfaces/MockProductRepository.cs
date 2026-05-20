using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Application.Interfaces;

public class MockProductRepository : IProductRepository
{
    public Task<Product?> GetProductByIdAsync(Guid id) => Task.FromResult<Product?>(null);

    public Task<Product?> GetProductByCodeAsync(string code, Guid companyId)
    {
       if(code == "123")
        {
            return Task.FromResult<Product?>(new Product
            {
               Code = "123"
            });
        }
       return Task.FromResult<Product?>(null);
    }

    public Task AddAsync(Product product)=>Task.CompletedTask;

    public Task<bool> SaveChangeAsync() => Task.FromResult(true);
    public Task UpdateAsync(Product product)=> Task.CompletedTask;
}
