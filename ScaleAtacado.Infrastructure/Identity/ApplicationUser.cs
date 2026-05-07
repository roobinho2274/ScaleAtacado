using Microsoft.AspNetCore.Identity;

namespace ScaleAtacado.Domain.Entities;

public class ApplicationUser: IdentityUser<Guid>
{
    public string NomeCompleto { get; set; } = string.Empty;

    public Guid EmpresaId { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navegação
    //public Empresa Empresa { get; set; } = null!;
}
