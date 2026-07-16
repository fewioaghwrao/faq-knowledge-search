using FaqKnowledgeSearch.WebForms.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace FaqKnowledgeSearch.WebForms.Data.Seed
{
    /// <summary>
    /// ポートフォリオ・動作確認用のデモユーザーを登録します。
    /// </summary>
    public static class DemoUserSeeder
    {
        private const string UserRole = "User";
        private const string EditorRole = "Editor";
        private const string AdminRole = "Admin";

        /// <summary>
        /// デモユーザーと必要なロールを登録します。
        /// </summary>
        public static async Task SeedAsync()
        {
            if (!IsSeedEnabled())
            {
                return;
            }

            var defaultPassword = GetDefaultPassword();

            using (var db = ApplicationIdentityDbContext.Create())
            {
                var userStore = new UserStore<ApplicationUser>(db)
                {
                    DisposeContext = false
                };

                var roleStore = new RoleStore<IdentityRole>(db)
                {
                    DisposeContext = false
                };

                using (var userManager =
                       new UserManager<ApplicationUser>(userStore))
                using (var roleManager =
                       new RoleManager<IdentityRole>(roleStore))
                {
                    ConfigureUserManager(userManager);

                    await EnsureRoleAsync(roleManager, UserRole);
                    await EnsureRoleAsync(roleManager, EditorRole);
                    await EnsureRoleAsync(roleManager, AdminRole);

                    foreach (var demoUser in CreateDemoUsers())
                    {
                        await EnsureUserAsync(
                            userManager,
                            demoUser,
                            defaultPassword);
                    }
                }
            }
        }

        /// <summary>
        /// デモユーザーSeedが有効か確認します。
        /// </summary>
        private static bool IsSeedEnabled()
        {
            var value =
                ConfigurationManager.AppSettings["SeedDemoUsers"];

            bool enabled;

            return bool.TryParse(value, out enabled) && enabled;
        }

        /// <summary>
        /// デモユーザー用の初期パスワードを取得します。
        /// </summary>
        private static string GetDefaultPassword()
        {
            var password =
                ConfigurationManager.AppSettings[
                    "DemoUserSeedPassword"];

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "DemoUserSeedPassword が設定されていません。");
            }

            return password;
        }

        /// <summary>
        /// Seedで使用するUserManagerを設定します。
        /// </summary>
        private static void ConfigureUserManager(
            UserManager<ApplicationUser> userManager)
        {
            // メールアドレスをUserNameとして使用できるようにします。
            userManager.UserValidator =
                new UserValidator<ApplicationUser>(userManager)
                {
                    AllowOnlyAlphanumericUserNames = false,
                    RequireUniqueEmail = true
                };

            // Password123! を満たすパスワードポリシーです。
            userManager.PasswordValidator =
                new PasswordValidator
                {
                    RequiredLength = 8,
                    RequireNonLetterOrDigit = true,
                    RequireDigit = true,
                    RequireLowercase = true,
                    RequireUppercase = true
                };
        }

        /// <summary>
        /// ロールが存在しない場合に作成します。
        /// </summary>
        private static async Task EnsureRoleAsync(
            RoleManager<IdentityRole> roleManager,
            string roleName)
        {
            if (await roleManager.RoleExistsAsync(roleName))
            {
                return;
            }

            var result =
                await roleManager.CreateAsync(
                    new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "ロールの作成に失敗しました: "
                    + roleName
                    + " / "
                    + string.Join(", ", result.Errors));
            }
        }

        /// <summary>
        /// デモユーザーを作成し、指定されたロールを付与します。
        /// 既存ユーザーの場合は、表示名などを同期します。
        /// </summary>
        private static async Task EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            DemoUserSeed demoUser,
            string defaultPassword)
        {
            var existingUser =
                await userManager.FindByEmailAsync(
                    demoUser.Email);

            if (existingUser != null)
            {
                await UpdateExistingUserAsync(
                    userManager,
                    existingUser,
                    demoUser);

                await EnsureUserRoleAsync(
                    userManager,
                    existingUser,
                    demoUser.Role);

                return;
            }

            var user = new ApplicationUser
            {
                UserName = demoUser.Email,
                Email = demoUser.Email,
                EmailConfirmed = true,
                DisplayName = demoUser.DisplayName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult =
                await userManager.CreateAsync(
                    user,
                    defaultPassword);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "デモユーザーの作成に失敗しました: "
                    + demoUser.Email
                    + " / "
                    + string.Join(", ", createResult.Errors));
            }

            await AddUserToRoleAsync(
                userManager,
                user,
                demoUser.Role);
        }

        /// <summary>
        /// 既存ユーザーのSeed対象項目を同期します。
        /// </summary>
        private static async Task UpdateExistingUserAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationUser existingUser,
            DemoUserSeed demoUser)
        {
            var requiresUpdate = false;

            if (existingUser.UserName != demoUser.Email)
            {
                existingUser.UserName = demoUser.Email;
                requiresUpdate = true;
            }

            if (existingUser.Email != demoUser.Email)
            {
                existingUser.Email = demoUser.Email;
                requiresUpdate = true;
            }

            if (existingUser.DisplayName != demoUser.DisplayName)
            {
                existingUser.DisplayName = demoUser.DisplayName;
                requiresUpdate = true;
            }

            if (!existingUser.EmailConfirmed)
            {
                existingUser.EmailConfirmed = true;
                requiresUpdate = true;
            }

            if (!existingUser.IsActive)
            {
                existingUser.IsActive = true;
                requiresUpdate = true;
            }

            if (!requiresUpdate)
            {
                return;
            }

            var updateResult =
                await userManager.UpdateAsync(existingUser);

            if (!updateResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "既存デモユーザーの更新に失敗しました: "
                    + demoUser.Email
                    + " / "
                    + string.Join(", ", updateResult.Errors));
            }
        }

        /// <summary>
        /// ユーザーに必要なロールが付与されているか確認します。
        /// </summary>
        private static async Task EnsureUserRoleAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationUser user,
            string roleName)
        {
            var alreadyInRole =
                await userManager.IsInRoleAsync(
                    user.Id,
                    roleName);

            if (alreadyInRole)
            {
                return;
            }

            await AddUserToRoleAsync(
                userManager,
                user,
                roleName);
        }

        /// <summary>
        /// ユーザーへロールを付与します。
        /// </summary>
        private static async Task AddUserToRoleAsync(
            UserManager<ApplicationUser> userManager,
            ApplicationUser user,
            string roleName)
        {
            var result =
                await userManager.AddToRoleAsync(
                    user.Id,
                    roleName);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "ユーザーへのロール付与に失敗しました: "
                    + user.Email
                    + " / "
                    + roleName
                    + " / "
                    + string.Join(", ", result.Errors));
            }
        }

        /// <summary>
        /// 登録対象のデモユーザーを生成します。
        /// </summary>
        private static IList<DemoUserSeed> CreateDemoUsers()
        {
            return new List<DemoUserSeed>
            {
                new DemoUserSeed(
                    "editor01@faq-app.local",
                    "ナレッジ管理者 01",
                    EditorRole),

                new DemoUserSeed(
                    "editor02@faq-app.local",
                    "ナレッジ管理者 02",
                    EditorRole),

                new DemoUserSeed(
                    "user01@faq-app.local",
                    "経理担当 01",
                    UserRole),

                new DemoUserSeed(
                    "user02@faq-app.local",
                    "経理担当 02",
                    UserRole),

                new DemoUserSeed(
                    "user03@faq-app.local",
                    "カスタマーサポート 01",
                    UserRole),

                new DemoUserSeed(
                    "user04@faq-app.local",
                    "カスタマーサポート 02",
                    UserRole),

                new DemoUserSeed(
                    "user05@faq-app.local",
                    "ヘルプデスク 01",
                    UserRole),

                new DemoUserSeed(
                    "user06@faq-app.local",
                    "ヘルプデスク 02",
                    UserRole),

                new DemoUserSeed(
                    "user07@faq-app.local",
                    "業務システム運用 01",
                    UserRole),

                new DemoUserSeed(
                    "user08@faq-app.local",
                    "業務システム運用 02",
                    UserRole),

                new DemoUserSeed(
                    "user09@faq-app.local",
                    "新人担当 01",
                    UserRole),

                new DemoUserSeed(
                    "user10@faq-app.local",
                    "引き継ぎ担当 01",
                    UserRole)
            };
        }

        /// <summary>
        /// デモユーザーの登録情報です。
        /// </summary>
        private sealed class DemoUserSeed
        {
            public DemoUserSeed(
                string email,
                string displayName,
                string role)
            {
                Email = email;
                DisplayName = displayName;
                Role = role;
            }

            public string Email { get; private set; }

            public string DisplayName { get; private set; }

            public string Role { get; private set; }
        }
    }
}