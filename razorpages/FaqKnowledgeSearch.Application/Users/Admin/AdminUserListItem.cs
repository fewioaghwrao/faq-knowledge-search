namespace FaqKnowledgeSearch.Application.Users.Admin;

public sealed record AdminUserListItem(
    string Id,
    string DisplayName,
    string Email,
    string RoleName,
    bool IsAdmin,
    bool IsActive,
    DateTime CreatedAt);