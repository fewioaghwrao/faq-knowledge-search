using Microsoft.AspNetCore.Identity;

namespace FaqKnowledgeSearch.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}