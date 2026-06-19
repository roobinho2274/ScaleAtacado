using ScaleAtacado.Domain.Enums;

namespace ScaleAtacado.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public int OrderNumber { get; set; }
    public Guid CompanyId { get; set; }
    public DateTime OrderDate { get; set; }
    public Guid UserId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal AmountTotal { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public decimal AmountWithSurchargeTotal { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.AwaitingPicking;
    public FinancialStatus FinancialStatus { get; set; } = FinancialStatus.Open;
    public bool IsOutstanding { get; set; }
    public bool IsLocked { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
