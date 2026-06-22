using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Application.Services;

public class CustomerAppService
{
    private readonly ICustomerRepository _repository;
    private readonly AuditLogAppService _auditLog;

    public CustomerAppService(ICustomerRepository repository, AuditLogAppService auditLog)
    {
        _repository = repository;
        _auditLog = auditLog;
    }

    public async Task<ApiResponse<CustomerResponseDto>> CreateAsync(
        CreateCustomerDto dto, Guid companyId, Guid userId, string userName)
    {
        var existing = await _repository.GetByTaxIdAsync(dto.TaxId, companyId);
        if (existing != null)
            return ApiResponse<CustomerResponseDto>.Fail("Já existe um cliente com este CPF/CNPJ.");

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            LegalName = dto.LegalName,
            TaxId = dto.TaxId,
            Address = dto.Address,
            Phone = dto.Phone,
            Notes = dto.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(customer);
        await _repository.SaveChangesAsync();

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "CriarCliente",
            entityName: "Cliente",
            entityId: customer.Id.ToString(),
            userName: userName,
            description: $"Cliente '{customer.LegalName}' cadastrado"
        );

        return ApiResponse<CustomerResponseDto>.Ok(ToDto(customer));
    }

    public async Task<ApiResponse<CustomerResponseDto>> UpdateAsync(
        Guid id, UpdateCustomerDto dto, Guid companyId, Guid userId, string userName)
    {
        var customer = await _repository.GetByIdAsync(id, companyId);
        if (customer == null)
            return ApiResponse<CustomerResponseDto>.Fail("Cliente não encontrado.");

        var existing = await _repository.GetByTaxIdAsync(dto.TaxId, companyId);
        if (existing != null && existing.Id != id)
            return ApiResponse<CustomerResponseDto>.Fail("Já existe outro cliente com este CPF/CNPJ.");

        customer.LegalName = dto.LegalName;
        customer.TaxId = dto.TaxId;
        customer.Address = dto.Address;
        customer.Phone = dto.Phone;
        customer.Notes = dto.Notes;
        customer.IsActive = dto.IsActive;

        await _repository.UpdateAsync(customer);
        await _repository.SaveChangesAsync();

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "EditarCliente",
            entityName: "Cliente",
            entityId: customer.Id.ToString(),
            userName: userName,
            description: $"Cliente '{customer.LegalName}' editado"
        );

        return ApiResponse<CustomerResponseDto>.Ok(ToDto(customer));
    }

    public async Task<ApiResponse<CustomerResponseDto>> GetByIdAsync(Guid id, Guid companyId)
    {
        var customer = await _repository.GetByIdAsync(id, companyId);
        if (customer == null)
            return ApiResponse<CustomerResponseDto>.Fail("Cliente não encontrado.");

        return ApiResponse<CustomerResponseDto>.Ok(ToDto(customer));
    }

    public async Task<ApiResponse<IEnumerable<CustomerResponseDto>>> SearchAsync(string? term, Guid companyId)
    {
        IEnumerable<Customer> customers = string.IsNullOrWhiteSpace(term)
            ? await _repository.GetAllActiveAsync(companyId)
            : await _repository.SearchAsync(term, companyId);

        return ApiResponse<IEnumerable<CustomerResponseDto>>.Ok(customers.Select(ToDto));
    }

    public async Task<ApiResponse> DeactivateAsync(Guid id, Guid companyId, Guid userId, string userName)
    {
        var customer = await _repository.GetByIdAsync(id, companyId);
        if (customer == null)
            return ApiResponse.Fail("Cliente não encontrado.");

        customer.IsActive = false;
        await _repository.UpdateAsync(customer);
        await _repository.SaveChangesAsync();

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "DesativarCliente",
            entityName: "Cliente",
            entityId: customer.Id.ToString(),
            userName: userName,
            description: $"Cliente '{customer.LegalName}' desativado"
        );

        return ApiResponse.Ok();
    }

    private static CustomerResponseDto ToDto(Customer c) => new(
        c.Id, c.CompanyId, c.LegalName, c.TaxId,
        c.Address, c.Phone, c.Notes, c.IsActive, c.CreatedAt
    );
}
