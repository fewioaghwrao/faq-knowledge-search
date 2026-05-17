namespace FaqApp.Api.Entities;

public class ApplicationUser : Microsoft.AspNetCore.Identity.IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
