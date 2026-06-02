namespace ScaleAtacado.Application.DTOs;

public record CreateOrderDto(
    Guid ClienteId,
    Guid PaymentMethodId,
    List<CreateOrderItemDto> Items
);
