using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Application.Interfaces;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, Guid companyId);
    Task<Customer?> GetByTaxIdAsync(string taxId, Guid companyId);
    Task<IEnumerable<Customer>> SearchAsync(string term, Guid companyId);
    Task<IEnumerable<Customer>> GetAllActiveAsync(Guid companyId);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task RemoveAsync(Customer customer);
    Task<bool> HasOrdersAsync(Guid customerId);
    Task<bool> SaveChangesAsync();
}
