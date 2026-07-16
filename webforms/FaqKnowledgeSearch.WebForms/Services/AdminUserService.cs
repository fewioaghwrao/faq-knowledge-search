using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using FaqKnowledgeSearch.WebForms.Dtos;
using FaqKnowledgeSearch.WebForms.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace FaqKnowledgeSearch.WebForms.Services
{
    /// <summary>
    /// 管理画面のユーザー管理処理を提供します。
    /// </summary>
    public class AdminUserService : IAdminUserService
    {
        private const string AdminRoleName = "Admin";

        /// <summary>
        /// ユーザー一覧をページ単位で取得します。
        /// </summary>
        public AdminUserPageDto GetPage(
            int pageNumber,
            int pageSize)
        {
            pageNumber = Math.Max(pageNumber, 1);
            pageSize = Math.Max(pageSize, 1);
            pageSize = Math.Min(pageSize, 100);

            using (var context =
                ApplicationIdentityDbContext.Create())
            {
                var totalCount =
                    context.Users.Count();

                var totalPages =
                    totalCount == 0
                        ? 0
                        : (int)Math.Ceiling(
                            totalCount / (double)pageSize);

                // 存在しないページが指定された場合は
                // 最終ページへ補正する
                if (totalPages > 0 &&
                    pageNumber > totalPages)
                {
                    pageNumber = totalPages;
                }

                var users = context.Users
                    .AsNoTracking()
                    .OrderByDescending(x => x.CreatedAt)
                    .ThenBy(x => x.DisplayName)
                    .ThenBy(x => x.Email)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new
                    {
                        x.Id,
                        x.DisplayName,
                        x.Email,
                        x.UserName,
                        x.IsActive,
                        x.CreatedAt
                    })
                    .ToList();

                var userIds = users
                    .Select(x => x.Id)
                    .ToList();

                var roleMap =
                    GetRoleMap(
                        context,
                        userIds);

                var items = users
                    .Select(x =>
                        new AdminUserListItemDto
                        {
                            Id = x.Id,

                            DisplayName =
                                string.IsNullOrWhiteSpace(
                                    x.DisplayName)
                                    ? "未設定"
                                    : x.DisplayName,

                            Email =
                                string.IsNullOrWhiteSpace(x.Email)
                                    ? x.UserName
                                    : x.Email,

                            RoleName =
                                roleMap.ContainsKey(x.Id)
                                    ? roleMap[x.Id]
                                    : "未割当",

                            IsActive = x.IsActive,
                            CreatedAt = x.CreatedAt
                        })
                    .ToList();

                return new AdminUserPageDto
                {
                    Items = items,
                    TotalCount = totalCount,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
        }

        /// <summary>
        /// 指定ユーザーの有効・無効状態を変更します。
        /// </summary>
        public async Task<IdentityResult> SetActiveAsync(
            string userId,
            bool isActive)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return IdentityResult.Failed(
                    "ユーザーIDが指定されていません。");
            }

            using (var context =
                ApplicationIdentityDbContext.Create())
            using (var userManager =
                CreateUserManager(context))
            {
                var user =
                    await userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return IdentityResult.Failed(
                        "対象のユーザーが見つかりません。");
                }

                var roleNames =
                    await userManager.GetRolesAsync(user.Id);

                var isAdmin =
                    roleNames.Any(
                        roleName =>
                            string.Equals(
                                roleName,
                                AdminRoleName,
                                StringComparison.OrdinalIgnoreCase));

                if (isAdmin)
                {
                    return IdentityResult.Failed(
                        "Adminユーザーの有効・無効は変更できません。");
                }

                if (user.IsActive == isActive)
                {
                    return IdentityResult.Success;
                }

                user.IsActive = isActive;

                if (!isActive)
                {
                    // 無効化したユーザーの既存認証Cookieを
                    // 将来的に無効と判断できるようにする
                    user.SecurityStamp =
                        Guid.NewGuid().ToString();
                }

                return await userManager.UpdateAsync(user);
            }
        }

        /// <summary>
        /// ユーザーIDごとの代表ロールを取得します。
        /// </summary>
        private static IDictionary<string, string> GetRoleMap(
            ApplicationIdentityDbContext context,
            IList<string> userIds)
        {
            if (userIds == null ||
                userIds.Count == 0)
            {
                return new Dictionary<string, string>();
            }

            var roleRows =
                (from userRole in
                     context.Set<IdentityUserRole>()
                         .AsNoTracking()
                 join role in
                     context.Roles.AsNoTracking()
                     on userRole.RoleId equals role.Id
                 where userIds.Contains(
                     userRole.UserId)
                 select new
                 {
                     userRole.UserId,
                     RoleName = role.Name
                 })
                .ToList();

            return roleRows
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    group => group.Key,
                    group => SelectRoleName(
                        group.Select(x => x.RoleName)));
        }

        /// <summary>
        /// 複数ロールがある場合に一覧へ表示する
        /// 代表ロールを決定します。
        /// </summary>
        private static string SelectRoleName(
            IEnumerable<string> roleNames)
        {
            var roles = roleNames
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x))
                .ToList();

            var adminRole =
                roles.FirstOrDefault(
                    x => string.Equals(
                        x,
                        AdminRoleName,
                        StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrWhiteSpace(adminRole))
            {
                return adminRole;
            }

            return roles
                .OrderBy(x => x)
                .FirstOrDefault()
                ?? "未割当";
        }

        /// <summary>
        /// UserManagerを生成します。
        /// </summary>
        private static UserManager<ApplicationUser>
            CreateUserManager(
                ApplicationIdentityDbContext context)
        {
            var userManager =
                new UserManager<ApplicationUser>(
                    new UserStore<ApplicationUser>(
                        context));

            // Seed処理と同じユーザー名・メール設定にする
            userManager.UserValidator =
                new UserValidator<ApplicationUser>(
                    userManager)
                {
                    AllowOnlyAlphanumericUserNames = false,
                    RequireUniqueEmail = true
                };

            return userManager;
        }
    }
}