using ScaleAtacado.Application.DTOs;
using ScaleAtacado.Application.Interfaces;
using ScaleAtacado.Domain.Entities;
using ScaleAtacado.Shared.Common;

namespace ScaleAtacado.Application.Services;

public class PaymentMethodAppService
{
    private readonly IPaymentMethodRepository _repository;

    public PaymentMethodAppService(IPaymentMethodRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<PaymentMethodResponseDto>> CreateAsync(CreatePaymentMethodDto dto, Guid companyId)
    {
        var existing = await _repository.GetByNameAsync(dto.Name, companyId);
        if (existing != null)
            return ApiResponse<PaymentMethodResponseDto>.Fail("Já existe uma forma de pagamento com este nome.");

        var paymentMethod = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = dto.Name,
            DeadlineDays = dto.DeadlineDays,
            SurchargePercentage = dto.SurchargePercentage,
            IsActive = true
        };

        await _repository.AddAsync(paymentMethod);
        await _repository.SaveChangesAsync();

        return ApiResponse<PaymentMethodResponseDto>.Ok(ToDto(paymentMethod));
    }

    public async Task<ApiResponse<PaymentMethodResponseDto>> UpdateAsync(Guid id, UpdatePaymentMethodDto dto, Guid companyId)
    {
        var paymentMethod = await _repository.GetByIdAsync(id, companyId);
        if (paymentMethod == null)
            return ApiResponse<PaymentMethodResponseDto>.Fail("Forma de pagamento não encontrada.");

        var existing = await _repository.GetByNameAsync(dto.Name, companyId);
        if (existing != null && existing.Id != id)
            return ApiResponse<PaymentMethodResponseDto>.Fail("Já existe outra forma de pagamento com este nome.");

        paymentMethod.Name = dto.Name;
        paymentMethod.DeadlineDays = dto.DeadlineDays;
        paymentMethod.SurchargePercentage = dto.SurchargePercentage;
        paymentMethod.IsActive = dto.IsActive;

        await _repository.UpdateAsync(paymentMethod);
        await _repository.SaveChangesAsync();

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

    public async Task<ApiResponse> DeactivateAsync(Guid id, Guid companyId)
    {
        var paymentMethod = await _repository.GetByIdAsync(id, companyId);
        if (paymentMethod == null)
            return ApiResponse.Fail("Forma de pagamento não encontrada.");

        paymentMethod.IsActive = false;
        await _repository.UpdateAsync(paymentMethod);
        await _repository.SaveChangesAsync();

        return ApiResponse.Ok();
    }

    private static PaymentMethodResponseDto ToDto(PaymentMethod p) => new(
        p.Id, p.CompanyId, p.Name, p.DeadlineDays, p.SurchargePercentage, p.IsActive
    );
}
