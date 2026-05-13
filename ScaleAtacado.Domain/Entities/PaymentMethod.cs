namespace ScaleAtacado.Domain.Entities;
public class PaymentMethod
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Description { get; set; } = string.Empty;
    public int DeadlineDays { get; set; }
    public decimal InterestRate { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
