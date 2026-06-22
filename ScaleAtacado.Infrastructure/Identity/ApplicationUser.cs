using Microsoft.AspNetCore.Identity;
using ScaleAtacado.Domain.Enums;

namespace ScaleAtacado.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public UserProfile Profile { get; set; } = UserProfile.Attendant;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int UserCode { get; set; }
}
