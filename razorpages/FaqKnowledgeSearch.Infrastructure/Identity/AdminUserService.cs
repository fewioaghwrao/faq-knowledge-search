using FaqKnowledgeSearch.Application.Common.Models;
using FaqKnowledgeSearch.Application.Users.Admin;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FaqKnowledgeSearch.Infrastructure.Identity;

public sealed class AdminUserService(
    UserManager<ApplicationUser> userManager)
    : IAdminUserService
{
    public async Task<PagedResult<AdminUserListItem>> SearchAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var query = userManager.Users
            .AsNoTracking();

        var totalCount = await query.CountAsync(
            cancellationToken);

        var users = await query
            .OrderByDescending(user => user.CreatedAt)
            .ThenBy(user => user.Email)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = new List<AdminUserListItem>(
            users.Count);

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);

            var isAdmin = roles.Contains(
                AppRoles.Admin,
                StringComparer.OrdinalIgnoreCase);

            items.Add(
                new AdminUserListItem(
                    user.Id,
                    user.UserName
                        ?? user.Email
                        ?? "名称未設定",
                    user.Email ?? "メール未設定",
                    isAdmin
                        ? AppRoles.Admin
                        : "User",
                    isAdmin,
                    user.IsActive,
                    user.CreatedAt));
        }

        return new PagedResult<AdminUserListItem>(
            items,
            totalCount,
            pageNumber,
            pageSize);
    }

    public async Task<AdminUserStatusChangeResult> SetActiveAsync(
        string userId,
        bool isActive,
        string currentUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return AdminUserStatusChangeResult.NotFound;
        }

        var user = await userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return AdminUserStatusChangeResult.NotFound;
        }

        if (string.Equals(
                user.Id,
                currentUserId,
                StringComparison.Ordinal))
        {
            return AdminUserStatusChangeResult
                .CannotChangeCurrentUser;
        }

        if (await userManager.IsInRoleAsync(
                user,
                AppRoles.Admin))
        {
            return AdminUserStatusChangeResult
                .CannotChangeAdmin;
        }

        if (user.IsActive == isActive)
        {
            return AdminUserStatusChangeResult.Success;
        }

        user.IsActive = isActive;

        var result = await userManager.UpdateAsync(user);

        return result.Succeeded
            ? AdminUserStatusChangeResult.Success
            : AdminUserStatusChangeResult.Failed;
    }
}