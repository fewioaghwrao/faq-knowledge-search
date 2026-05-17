using FaqApp.Api.Entities;
using Microsoft.AspNetCore.Identity;

namespace FaqApp.Api.Data.Seed;

public static class AdminUserSeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration)
    {
        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("AdminSeed:Email が設定されていません。");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException("AdminSeed:Password が設定されていません。");
        }

        var existingUser = await userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            if (!await userManager.IsInRoleAsync(existingUser, "Admin"))
            {
                await userManager.AddToRoleAsync(existingUser, "Admin");
            }

            return;
        }

        var adminUser = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = "システム管理者",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(x => x.Description));
            throw new InvalidOperationException($"初期管理者ユーザーの作成に失敗しました: {errors}");
        }

        await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}
