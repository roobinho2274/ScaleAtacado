using Microsoft.EntityFrameworkCore;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Infrastructure.Persistence;

namespace ScaleAtacado.Infrastructure.Repositories;

public class ClienteRepository : IClienteRepository
{
    private readonly AppDbContext _context;

    public ClienteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Cliente?> GetByIdAsync(Guid id, Guid companyId)
        => await _context.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.CompanyId == companyId);

    public async Task<Cliente?> GetByDocumentoAsync(string documento, Guid companyId)
        => await _context.Clientes.FirstOrDefaultAsync(c => c.Documento == documento && c.CompanyId == companyId);

    public async Task<IEnumerable<Cliente>> SearchAsync(string term, Guid companyId)
        => await _context.Clientes
            .Where(c => c.CompanyId == companyId && c.IsActive &&
                       (c.NomeRazaoSocial.Contains(term) ||
                        c.Documento.Contains(term) ||
                        c.Telefone.Contains(term)))
            .OrderBy(c => c.NomeRazaoSocial)
            .ToListAsync();

    public async Task<IEnumerable<Cliente>> GetAllActiveAsync(Guid companyId)
        => await _context.Clientes
            .Where(c => c.CompanyId == companyId && c.IsActive)
            .OrderBy(c => c.NomeRazaoSocial)
            .ToListAsync();

    public async Task AddAsync(Cliente cliente)
        => await _context.Clientes.AddAsync(cliente);

    public Task UpdateAsync(Cliente cliente)
    {
        _context.Clientes.Update(cliente);
        return Task.CompletedTask;
    }

    public async Task<bool> SaveChangesAsync()
        => await _context.SaveChangesAsync() > 0;
}
