namespace ScaleAtacado.Domain.Entities;

public class PaymentMethod
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DeadlineDays { get; set; }
    public decimal SurchargePercentage { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
