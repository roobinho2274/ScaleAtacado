using Microsoft.AspNetCore.Identity;

namespace ScaleAtacado.Infrastructure.Identity;

public class ApplicationUser: IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public Guid CompanyId { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    //public Company Company { get; set; } = null!;
}
