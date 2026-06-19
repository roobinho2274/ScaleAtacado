namespace ScaleAtacado.Domain.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string LegalName { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
