namespace ScaleAtacado.Application.DTOs;

public record ProductResponseDto(
    Guid Id,
    Guid CompanyId,
    string Name,
    string? Code,
    decimal CostPrice,
    decimal ProfitMargin,
    decimal BaseSalePrice,
    bool IsActive,
    Guid CategoryId,
    string CategoryName
);
