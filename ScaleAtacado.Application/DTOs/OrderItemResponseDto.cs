namespace ScaleAtacado.Application.DTOs;

public record OrderItemResponseDto(
    Guid Id,
    Guid? ProductId,
    string ProductName,
    string? ProductCode,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);
