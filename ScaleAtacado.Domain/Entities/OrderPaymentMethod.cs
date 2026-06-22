namespace ScaleAtacado.Domain.Entities;

public class OrderPaymentMethod
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal Amount { get; set; }

    public virtual Order Order { get; set; } = null!;
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}
