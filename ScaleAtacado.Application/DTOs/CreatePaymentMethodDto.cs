namespace ScaleAtacado.Application.DTOs;

public record CreatePaymentMethodDto(
    string Name,
    int DeadlineDays,
    decimal SurchargePercentage
);
