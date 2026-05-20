namespace ScaleAtacado.Application.DTOs;

public record CreateProductDto
(
    Guid CompanyId,
    string Code,
    string Name,
    decimal CostPrice,
    decimal ProfitMargin,
    Guid CategoryId
);