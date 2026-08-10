namespace ScaleAtacado.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? ProductId { get; set; }                    // null = produto avulso
    public string ProductName { get; set; } = string.Empty; // gravado no ato do pedido
    public string? ProductCode { get; set; }                // gravado no ato do pedido
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public virtual Order Order { get; set; } = null!;
    public virtual Product? Product { get; set; }           // null para avulsos
}
