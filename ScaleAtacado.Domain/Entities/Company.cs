namespace ScaleAtacado.Domain.Entities;

public class Company
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CNPJ { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? Address { get; set; }
    public string? Phone { get; set; }
}

