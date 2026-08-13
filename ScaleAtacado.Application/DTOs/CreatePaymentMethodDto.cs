namespace ScaleAtacado.Application.DTOs;

public record CreatePaymentMethodDto(
    string Name,
    bool IsInstallment,
    decimal SurchargePercentage,
    decimal FixedFee = 0,
    bool AcceptsChange = false
);
