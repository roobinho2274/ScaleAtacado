using ScaleAtacado.Domain.Entities;

namespace ScaleAtacado.Application.Interfaces;

public interface IClienteRepository
{
    Task<Cliente?> GetByIdAsync(Guid id, Guid companyId);
    Task<Cliente?> GetByDocumentoAsync(string documento, Guid companyId);
    Task<IEnumerable<Cliente>> SearchAsync(string term, Guid companyId);
    Task<IEnumerable<Cliente>> GetAllActiveAsync(Guid companyId);
    Task AddAsync(Cliente cliente);
    Task UpdateAsync(Cliente cliente);
    Task<bool> SaveChangesAsync();
}
