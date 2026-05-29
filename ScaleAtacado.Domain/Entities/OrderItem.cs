namespace ScaleAtacado.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    // Relations with other entities
    public virtual Order Order { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
