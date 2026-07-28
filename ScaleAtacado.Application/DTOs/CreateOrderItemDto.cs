namespace ScaleAtacado.Application.DTOs;

public record CreateOrderItemDto(Guid ProductId, decimal Quantity);
