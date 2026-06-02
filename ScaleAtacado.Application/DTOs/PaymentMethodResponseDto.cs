namespace ScaleAtacado.Application.DTOs;

public record PaymentMethodResponseDto(
    Guid Id,
    Guid CompanyId,
    string Name,
    int DeadlineDays,
    decimal SurchargePercentage,
    bool IsActive
);
