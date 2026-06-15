using ScaleAtacado.Domain.Enums;

namespace ScaleAtacado.Application.DTOs;

public record OrderResponseDto(
    Guid Id,
    int OrderNumber,
    Guid CompanyId,
    Guid ClienteId,
    string ClienteNome,
    Guid PaymentMethodId,
    string PaymentMethodName,
    decimal SurchargePercentage,
    Guid UserId,
    DateTime OrderDate,
    decimal AmountTotal,
    decimal DiscountAmount,
    decimal AmountWithSurchargeTotal,
    DeliveryStatus DeliveryStatus,
    FinancialStatus FinancialStatus,
    bool IsLocked,
    List<OrderItemResponseDto> Items
);
