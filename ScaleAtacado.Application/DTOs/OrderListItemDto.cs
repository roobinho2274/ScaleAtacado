using ScaleAtacado.Domain.Enums;

namespace ScaleAtacado.Application.DTOs;

public record OrderListItemDto(
    Guid Id,
    int OrderNumber,
    string CustomerName,
    string PaymentMethodName,
    DateTime OrderDate,
    decimal AmountTotal,
    decimal AmountWithSurchargeTotal,
    DeliveryStatus DeliveryStatus,
    FinancialStatus FinancialStatus,
    bool IsLocked
);
