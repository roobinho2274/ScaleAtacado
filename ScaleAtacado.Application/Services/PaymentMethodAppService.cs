using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Application.Services;

public class PaymentMethodAppService
{
    private readonly IPaymentMethodRepository _repository;
    private readonly AuditLogAppService _auditLog;

    public PaymentMethodAppService(IPaymentMethodRepository repository, AuditLogAppService auditLog)
    {
        _repository = repository;
        _auditLog = auditLog;
    }

    public async Task<ApiResponse<PaymentMethodResponseDto>> CreateAsync(
        CreatePaymentMethodDto dto, Guid companyId, Guid userId, string userName)
    {
        var existing = await _repository.GetByNameAsync(dto.Name, companyId);
        if (existing != null)
            return ApiResponse<PaymentMethodResponseDto>.Fail("Já existe uma forma de pagamento com este nome.");

        var paymentMethod = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = dto.Name,
            IsInstallment = dto.IsInstallment,
            SurchargePercentage = dto.SurchargePercentage,
            IsActive = true
        };

        await _repository.AddAsync(paymentMethod);
        await _repository.SaveChangesAsync();

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "CriarFormaPagamento",
            entityName: "FormaPagamento",
            entityId: paymentMethod.Id.ToString(),
            userName: userName,
            description: $"Forma de pagamento '{paymentMethod.Name}' criada ({(paymentMethod.IsInstallment ? "A Prazo" : "À Vista")})"
        );

        return ApiResponse<PaymentMethodResponseDto>.Ok(ToDto(paymentMethod));
    }

    public async Task<ApiResponse<PaymentMethodResponseDto>> UpdateAsync(
        Guid id, UpdatePaymentMethodDto dto, Guid companyId, Guid userId, string userName)
    {
        var paymentMethod = await _repository.GetByIdAsync(id, companyId);
        if (paymentMethod == null)
            return ApiResponse<PaymentMethodResponseDto>.Fail("Forma de pagamento não encontrada.");

        var existing = await _repository.GetByNameAsync(dto.Name, companyId);
        if (existing != null && existing.Id != id)
            return ApiResponse<PaymentMethodResponseDto>.Fail("Já existe outra forma de pagamento com este nome.");

        paymentMethod.Name = dto.Name;
        paymentMethod.IsInstallment = dto.IsInstallment;
        paymentMethod.SurchargePercentage = dto.SurchargePercentage;
        paymentMethod.IsActive = dto.IsActive;

        await _repository.UpdateAsync(paymentMethod);
        await _repository.SaveChangesAsync();

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "EditarFormaPagamento",
            entityName: "FormaPagamento",
            entityId: paymentMethod.Id.ToString(),
            userName: userName,
            description: $"Forma de pagamento '{paymentMethod.Name}' editada"
        );

        return ApiResponse<PaymentMethodResponseDto>.Ok(ToDto(paymentMethod));
    }

    public async Task<ApiResponse<PaymentMethodResponseDto>> GetByIdAsync(Guid id, Guid companyId)
    {
        var paymentMethod = await _repository.GetByIdAsync(id, companyId);
        if (paymentMethod == null)
            return ApiResponse<PaymentMethodResponseDto>.Fail("Forma de pagamento não encontrada.");

        return ApiResponse<PaymentMethodResponseDto>.Ok(ToDto(paymentMethod));
    }

    public async Task<ApiResponse<IEnumerable<PaymentMethodResponseDto>>> GetAllAsync(Guid companyId, bool onlyActive = false)
    {
        var list = onlyActive
            ? await _repository.GetAllActiveAsync(companyId)
            : await _repository.GetAllAsync(companyId);

        return ApiResponse<IEnumerable<PaymentMethodResponseDto>>.Ok(list.Select(ToDto));
    }

    public async Task<ApiResponse> DeactivateAsync(Guid id, Guid companyId, Guid userId, string userName)
    {
        var paymentMethod = await _repository.GetByIdAsync(id, companyId);
        if (paymentMethod == null)
            return ApiResponse.Fail("Forma de pagamento não encontrada.");

        paymentMethod.IsActive = false;
        await _repository.UpdateAsync(paymentMethod);
        await _repository.SaveChangesAsync();

        await _auditLog.RecordAsync(
            companyId, userId,
            operation: "DesativarFormaPagamento",
            entityName: "FormaPagamento",
            entityId: paymentMethod.Id.ToString(),
            userName: userName,
            description: $"Forma de pagamento '{paymentMethod.Name}' desativada"
        );

        return ApiResponse.Ok();
    }

    private static PaymentMethodResponseDto ToDto(PaymentMethod p) => new(
        p.Id, p.CompanyId, p.Name, p.IsInstallment, p.SurchargePercentage, p.IsActive
    );
}
