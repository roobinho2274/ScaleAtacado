namespace ScaleAtacado.Application.DTOs;

public record PaymentMethodResponseDto(
    Guid Id,
    Guid CompanyId,
    string Name,
    bool IsInstallment,
    decimal SurchargePercentage,
    bool IsActive,
    decimal FixedFee = 0,
    bool AcceptsChange = false
);
