using ScaleAtacado.Domain.Enums;

namespace ScaleAtacado.Application.DTOs;

public record OrderResponseDto(
    Guid Id,
    int OrderNumber,
    Guid CompanyId,
    Guid CustomerId,
    string CustomerName,
    bool IsInstallment,
    decimal SurchargePercentage,
    List<OrderPaymentMethodDto> PaymentMethods,
    Guid UserId,
    DateTime OrderDate,
    decimal AmountTotal,
    decimal DiscountAmount,
    decimal AmountWithSurchargeTotal,
    OrderStatus OrderStatus,
    FinancialStatus FinancialStatus,
    bool IsLocked,
    List<OrderItemResponseDto> Items,
    string? CompanyName        = null,
    string? CompanyCNPJ       = null,
    string? CompanyAddress    = null,
    string? CompanyLogoBase64 = null,
    int Version               = 1,
    int? LastPrintedVersion   = null,
    string? Notes             = null,
    decimal FixedFeeAmount    = 0
);
