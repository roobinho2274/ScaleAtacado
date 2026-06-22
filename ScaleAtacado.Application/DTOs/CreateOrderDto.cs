namespace ScaleAtacado.Application.DTOs;

public record CreateOrderDto(
    Guid CustomerId,
    bool IsInstallment,
    List<OrderPaymentInputDto> Payments,
    List<CreateOrderItemDto> Items,
    decimal DiscountAmount = 0m
);
