using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Application.Services;

public class ClienteAppService
{
    private readonly IClienteRepository _repository;

    public ClienteAppService(IClienteRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<ClienteResponseDto>> CreateAsync(CreateClienteDto dto, Guid companyId)
    {
        var existing = await _repository.GetByDocumentoAsync(dto.Documento, companyId);
        if (existing != null)
            return ApiResponse<ClienteResponseDto>.Fail("Já existe um cliente com este CPF/CNPJ.");

        var cliente = new Cliente
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            NomeRazaoSocial = dto.NomeRazaoSocial,
            Documento = dto.Documento,
            Endereco = dto.Endereco,
            Telefone = dto.Telefone,
            Observacoes = dto.Observacoes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(cliente);
        await _repository.SaveChangesAsync();

        return ApiResponse<ClienteResponseDto>.Ok(ToDto(cliente));
    }

    public async Task<ApiResponse<ClienteResponseDto>> UpdateAsync(Guid id, UpdateClienteDto dto, Guid companyId)
    {
        var cliente = await _repository.GetByIdAsync(id, companyId);
        if (cliente == null)
            return ApiResponse<ClienteResponseDto>.Fail("Cliente não encontrado.");

        var existing = await _repository.GetByDocumentoAsync(dto.Documento, companyId);
        if (existing != null && existing.Id != id)
            return ApiResponse<ClienteResponseDto>.Fail("Já existe outro cliente com este CPF/CNPJ.");

        cliente.NomeRazaoSocial = dto.NomeRazaoSocial;
        cliente.Documento = dto.Documento;
        cliente.Endereco = dto.Endereco;
        cliente.Telefone = dto.Telefone;
        cliente.Observacoes = dto.Observacoes;
        cliente.IsActive = dto.IsActive;

        await _repository.UpdateAsync(cliente);
        await _repository.SaveChangesAsync();

        return ApiResponse<ClienteResponseDto>.Ok(ToDto(cliente));
    }

    public async Task<ApiResponse<ClienteResponseDto>> GetByIdAsync(Guid id, Guid companyId)
    {
        var cliente = await _repository.GetByIdAsync(id, companyId);
        if (cliente == null)
            return ApiResponse<ClienteResponseDto>.Fail("Cliente não encontrado.");

        return ApiResponse<ClienteResponseDto>.Ok(ToDto(cliente));
    }

    public async Task<ApiResponse<IEnumerable<ClienteResponseDto>>> SearchAsync(string? term, Guid companyId)
    {
        IEnumerable<Cliente> clientes = string.IsNullOrWhiteSpace(term)
            ? await _repository.GetAllActiveAsync(companyId)
            : await _repository.SearchAsync(term, companyId);

        return ApiResponse<IEnumerable<ClienteResponseDto>>.Ok(clientes.Select(ToDto));
    }

    public async Task<ApiResponse> DeactivateAsync(Guid id, Guid companyId)
    {
        var cliente = await _repository.GetByIdAsync(id, companyId);
        if (cliente == null)
            return ApiResponse.Fail("Cliente não encontrado.");

        cliente.IsActive = false;
        await _repository.UpdateAsync(cliente);
        await _repository.SaveChangesAsync();

        return ApiResponse.Ok();
    }

    private static ClienteResponseDto ToDto(Cliente c) => new(
        c.Id, c.CompanyId, c.NomeRazaoSocial, c.Documento,
        c.Endereco, c.Telefone, c.Observacoes, c.IsActive, c.CreatedAt
    );
}
