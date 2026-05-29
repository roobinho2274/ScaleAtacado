namespace ScaleAtacado.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public DateTime OrderDate { get; set; }
    public Guid UserId { get; set; }
    public decimal AmountTotal { get; set; } = 0;
    public decimal AmountWithInterestTotal { get; set; }
    public int DeliveryStatus { get; set; } 
    public int PaymentStatus { get; set; }
    public bool IsOutstanding { get; set; }
    public bool IsLoked { get; set; }

    // Relations with other entities
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    // Order Items
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
