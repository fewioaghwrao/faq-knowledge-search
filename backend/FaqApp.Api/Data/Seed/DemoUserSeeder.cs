using FaqApp.Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace FaqApp.Api.Data.Seed;

public static class DemoUserSeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager)
    {
        const string defaultPassword = "Password123!";

        var demoUsers = new[]
        {
            new DemoUserSeed("editor01@faq-app.local", "ナレッジ管理者 01", "Editor"),
            new DemoUserSeed("editor02@faq-app.local", "ナレッジ管理者 02", "Editor"),

            new DemoUserSeed("user01@faq-app.local", "経理担当 01", "User"),
            new DemoUserSeed("user02@faq-app.local", "経理担当 02", "User"),
            new DemoUserSeed("user03@faq-app.local", "カスタマーサポート 01", "User"),
            new DemoUserSeed("user04@faq-app.local", "カスタマーサポート 02", "User"),
            new DemoUserSeed("user05@faq-app.local", "ヘルプデスク 01", "User"),
            new DemoUserSeed("user06@faq-app.local", "ヘルプデスク 02", "User"),
            new DemoUserSeed("user07@faq-app.local", "業務システム運用 01", "User"),
            new DemoUserSeed("user08@faq-app.local", "業務システム運用 02", "User"),
            new DemoUserSeed("user09@faq-app.local", "新人担当 01", "User"),
            new DemoUserSeed("user10@faq-app.local", "引き継ぎ担当 01", "User"),
        };

        foreach (var demoUser in demoUsers)
        {
            var existingUser = await userManager.FindByEmailAsync(demoUser.Email);

            if (existingUser is not null)
            {
                if (!await userManager.IsInRoleAsync(existingUser, demoUser.Role))
                {
                    await userManager.AddToRoleAsync(existingUser, demoUser.Role);
                }

                continue;
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

            var result = await userManager.CreateAsync(user, defaultPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));
                throw new InvalidOperationException(
                    $"デモユーザーの作成に失敗しました: {demoUser.Email} / {errors}");
            }

            await userManager.AddToRoleAsync(user, demoUser.Role);
        }
    }

    private sealed record DemoUserSeed(
        string Email,
        string DisplayName,
        string Role);
}