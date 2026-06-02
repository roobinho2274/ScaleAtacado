namespace ScaleAtacado.Application.DTOs;

public record OrderItemResponseDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string? ProductCode,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);
