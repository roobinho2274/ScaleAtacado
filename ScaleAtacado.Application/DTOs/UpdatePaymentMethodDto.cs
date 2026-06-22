namespace ScaleAtacado.Application.DTOs;

public record UpdatePaymentMethodDto(
    string Name,
    bool IsInstallment,
    decimal SurchargePercentage,
    bool IsActive
);
