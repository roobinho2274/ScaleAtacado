using ScaleAtacado.Domain.Enums;

namespace ScaleAtacado.Application.DTOs;

public record OrderListItemDto(
    Guid Id,
    int OrderNumber,
    string CustomerName,
    bool IsInstallment,
    string PaymentMethodName,   // nomes unidos por " + " (ex: "Dinheiro + PIX")
    DateTime OrderDate,
    decimal AmountTotal,
    decimal AmountWithSurchargeTotal,
    OrderStatus OrderStatus,
    FinancialStatus FinancialStatus,
    bool IsLocked
);
