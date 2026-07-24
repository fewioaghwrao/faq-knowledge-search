using FaqKnowledgeSearch.Application.Common.Models;

namespace FaqKnowledgeSearch.Application.Users.Admin;

public interface IAdminUserService
{
    Task<PagedResult<AdminUserListItem>> SearchAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<AdminUserStatusChangeResult> SetActiveAsync(
        string userId,
        bool isActive,
        string currentUserId,
        CancellationToken cancellationToken = default);
}