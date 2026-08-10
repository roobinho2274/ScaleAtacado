namespace ScaleAtacado.Domain.Entities;

public class PaymentMethod
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsInstallment { get; set; }   // false = À Vista | true = A Prazo
    public decimal SurchargePercentage { get; set; }
    public decimal FixedFee { get; set; } = 0;
    public bool IsActive { get; set; } = true;

    public virtual ICollection<OrderPaymentMethod> OrderPaymentMethods { get; set; } = new List<OrderPaymentMethod>();
}
