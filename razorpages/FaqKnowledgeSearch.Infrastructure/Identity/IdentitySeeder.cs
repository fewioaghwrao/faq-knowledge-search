using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace FaqKnowledgeSearch.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        string adminEmail,
        string adminPassword,
        bool seedDemoUsers = false,
        string? demoUserPassword = null)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager =
            scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

        await EnsureRoleAsync(
            roleManager,
            AppRoles.Admin);

        await EnsureRoleAsync(
            roleManager,
            AppRoles.User);

        await EnsureAdminUserAsync(
            userManager,
            adminEmail,
            adminPassword);

        if (!seedDemoUsers)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(demoUserPassword))
        {
            throw new InvalidOperationException(
                "デモユーザーを作成する場合は、"
                + "デモユーザー用パスワードを設定してください。");
        }

        await SeedDemoUsersAsync(
            userManager,
            demoUserPassword);
    }

    private static async Task EnsureAdminUserAsync(
        UserManager<ApplicationUser> userManager,
        string adminEmail,
        string adminPassword)
    {
        var adminUser =
            await userManager.FindByEmailAsync(adminEmail);

        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                DisplayName = "システム管理者",
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                IsActive = true,
                LockoutEnabled = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult =
                await userManager.CreateAsync(
                    adminUser,
                    adminPassword);

            EnsureSucceeded(
                createResult,
                "管理者ユーザーの作成");
        }
        else if (string.IsNullOrWhiteSpace(
                     adminUser.DisplayName))
        {
            adminUser.DisplayName = "システム管理者";

            var updateResult =
                await userManager.UpdateAsync(adminUser);

            EnsureSucceeded(
                updateResult,
                "管理者ユーザーの更新");
        }

        if (await userManager.IsInRoleAsync(
                adminUser,
                AppRoles.Admin))
        {
            return;
        }

        var roleResult =
            await userManager.AddToRoleAsync(
                adminUser,
                AppRoles.Admin);

        EnsureSucceeded(
            roleResult,
            "管理者ロールの付与");
    }

    private static async Task SeedDemoUsersAsync(
        UserManager<ApplicationUser> userManager,
        string demoUserPassword)
    {
        var demoUsers = new[]
        {
            new DemoUserSeed(
                "引き継ぎ担当 01",
                "user10@faq-app.local",
                true),

            new DemoUserSeed(
                "新人担当 01",
                "user09@faq-app.local",
                true),

            new DemoUserSeed(
                "業務システム運用 02",
                "user08@faq-app.local",
                true),

            new DemoUserSeed(
                "業務システム運用 01",
                "user07@faq-app.local",
                true),

            new DemoUserSeed(
                "ヘルプデスク 02",
                "user06@faq-app.local",
                true),

            new DemoUserSeed(
                "ヘルプデスク 01",
                "user05@faq-app.local",
                true),

            new DemoUserSeed(
                "経理担当 02",
                "user04@faq-app.local",
                true),

            new DemoUserSeed(
                "経理担当 01",
                "user03@faq-app.local",
                true),

            new DemoUserSeed(
                "総務担当 02",
                "user02@faq-app.local",
                false),

            new DemoUserSeed(
                "総務担当 01",
                "user01@faq-app.local",
                true),

            new DemoUserSeed(
                "テストユーザー 02",
                "test02@faq-app.local",
                false),

            new DemoUserSeed(
                "テストユーザー 01",
                "test01@faq-app.local",
                true)
        };

        for (var index = 0;
             index < demoUsers.Length;
             index++)
        {
            var seed = demoUsers[index];

            await EnsureDemoUserAsync(
                userManager,
                seed,
                demoUserPassword,
                index);
        }
    }

    private static async Task EnsureDemoUserAsync(
        UserManager<ApplicationUser> userManager,
        DemoUserSeed seed,
        string password,
        int index)
    {
        var user =
            await userManager.FindByEmailAsync(seed.Email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                DisplayName = seed.DisplayName,
                UserName = seed.Email,
                Email = seed.Email,
                EmailConfirmed = true,
                IsActive = seed.IsActive,
                LockoutEnabled = true,

                // 一覧で作成日時に少し差が出るようにする
                CreatedAt = DateTime.UtcNow
                    .AddMinutes(-index)
            };

            var createResult =
                await userManager.CreateAsync(
                    user,
                    password);

            EnsureSucceeded(
                createResult,
                $"デモユーザー「{seed.Email}」の作成");
        }
        else if (string.IsNullOrWhiteSpace(
                     user.DisplayName))
        {
            // 既存ユーザーの有効・無効状態は上書きしない
            user.DisplayName = seed.DisplayName;

            var updateResult =
                await userManager.UpdateAsync(user);

            EnsureSucceeded(
                updateResult,
                $"デモユーザー「{seed.Email}」の更新");
        }

        if (await userManager.IsInRoleAsync(
                user,
                AppRoles.User))
        {
            return;
        }

        var roleResult =
            await userManager.AddToRoleAsync(
                user,
                AppRoles.User);

        EnsureSucceeded(
            roleResult,
            $"デモユーザー「{seed.Email}」へのロール付与");
    }

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

        EnsureSucceeded(
            result,
            $"ロール「{roleName}」の作成");
    }

    private static void EnsureSucceeded(
        IdentityResult result,
        string operationName)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(
            Environment.NewLine,
            result.Errors.Select(
                error =>
                    $"{error.Code}: {error.Description}"));

        throw new InvalidOperationException(
            $"{operationName}に失敗しました。"
            + $"{Environment.NewLine}{errors}");
    }

    private sealed record DemoUserSeed(
        string DisplayName,
        string Email,
        bool IsActive);
}