using Microsoft.AspNetCore.Identity;

namespace FaqApp.Api.Data.Seed;

public static class RoleSeeder
{
    private static readonly string[] Roles =
    {
        "User",
        "Editor",
        "Admin"
    };

    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in Roles)
        {
            var exists = await roleManager.RoleExistsAsync(role);

            if (!exists)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}
