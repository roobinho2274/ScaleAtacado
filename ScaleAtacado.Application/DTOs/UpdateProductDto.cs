namespace ScaleAtacado.Application.DTOs;

public record UpdateProductDto(
    string Name,
    string? Code,
    decimal CostPrice,
    decimal ProfitMargin,
    Guid CategoryId,
    bool IsActive,
    int PackageQuantity = 1
);
