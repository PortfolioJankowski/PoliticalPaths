using Microsoft.AspNetCore.Identity;

namespace PoliticalPaths.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
}
