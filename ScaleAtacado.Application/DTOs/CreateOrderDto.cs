namespace ScaleAtacado.Application.DTOs;

public record CreateOrderDto(
    Guid CustomerId,
    Guid PaymentMethodId,
    List<CreateOrderItemDto> Items,
    decimal DiscountAmount = 0m
);
