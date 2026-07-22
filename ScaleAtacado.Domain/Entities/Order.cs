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
    public bool IsInstallment { get; set; }            // false = À Vista | true = A Prazo
    public decimal SurchargePercentage { get; set; }   // snapshot no momento da criação
    public decimal AmountTotal { get; set; } = 0;
    public decimal DiscountAmount { get; set; } = 0;
    public decimal AmountWithSurchargeTotal { get; set; }
    public DeliveryStatus DeliveryStatus { get; set; } = DeliveryStatus.AwaitingPicking;
    public FinancialStatus FinancialStatus { get; set; } = FinancialStatus.Open;
    public bool IsOutstanding { get; set; }
    public bool IsLocked { get; set; }
    public decimal FixedFeeAmount { get; set; } = 0;
    public int Version { get; set; } = 1;
    public int? LastPrintedVersion { get; set; }
    public string? Notes { get; set; }

    public virtual Customer Customer { get; set; } = null!;
    public virtual ICollection<OrderPaymentMethod> PaymentMethods { get; set; } = new List<OrderPaymentMethod>();
    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
