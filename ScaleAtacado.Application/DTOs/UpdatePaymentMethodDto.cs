namespace ScaleAtacado.Application.DTOs;

public record UpdatePaymentMethodDto(
    string Name,
    int DeadlineDays,
    decimal SurchargePercentage,
    bool IsActive
);
