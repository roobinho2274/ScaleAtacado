namespace ScaleAtacado.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public decimal CostPrice { get; set; }
    public decimal ProfitMargin { get; set; }
    public decimal BaseSalePrice { get; set; }

    // Relationship
    public Guid CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
}
