namespace ScaleAtacado.Domain.Entities;

public class Cliente
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string NomeRazaoSocial { get; set; } = string.Empty;
    public string Documento { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string? Observacoes { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
