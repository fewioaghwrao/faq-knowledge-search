namespace FaqKnowledgeSearch.WebForms.Migrations.Identity
{
    using System;
    using System.Configuration;
    using System.Data.Entity.Migrations;
    using FaqKnowledgeSearch.WebForms.Identity;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using MySql.Data.EntityFramework;

    internal sealed class Configuration
        : DbMigrationsConfiguration<ApplicationIdentityDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations\Identity";

            CodeGenerator = new MySqlMigrationCodeGenerator();

            SetSqlGenerator(
                "MySql.Data.MySqlClient",
                new MySqlMigrationSqlGenerator());
        }

        protected override void Seed(
            ApplicationIdentityDbContext context)
        {
            const string adminRoleName = "Admin";
            const string userRoleName = "User";

            var adminEmail =
                ConfigurationManager.AppSettings["SeedAdminEmail"];

            var adminPassword =
                ConfigurationManager.AppSettings["SeedAdminPassword"];

            if (string.IsNullOrWhiteSpace(adminEmail) ||
                string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "初期管理者のメールアドレスまたはパスワードが設定されていません。");
            }

            using (var roleManager =
                new RoleManager<IdentityRole>(
                    new RoleStore<IdentityRole>(context)))
            using (var userManager =
                new UserManager<ApplicationUser>(
                    new UserStore<ApplicationUser>(context)))
            {
                userManager.UserValidator =
                    new UserValidator<ApplicationUser>(userManager)
                    {
                        AllowOnlyAlphanumericUserNames = false,
                        RequireUniqueEmail = true
                    };

                userManager.PasswordValidator =
                    new PasswordValidator
                    {
                        RequiredLength = 8,
                        RequireNonLetterOrDigit = true,
                        RequireDigit = true,
                        RequireLowercase = true,
                        RequireUppercase = true
                    };

                var roleNames = new[]
                {
    adminRoleName,
    userRoleName
};

                foreach (var roleName in roleNames)
                {
                    if (roleManager.RoleExists(roleName))
                    {
                        continue;
                    }

                    var roleResult =
                        roleManager.Create(
                            new IdentityRole(roleName));

                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            string.Join(
                                Environment.NewLine,
                                roleResult.Errors));
                    }
                }

                var adminUser =
                    userManager.FindByName(adminEmail);

                if (adminUser == null)
                {
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        LockoutEnabled = true,
                        DisplayName = "システム管理者",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var userResult =
                        userManager.Create(
                            adminUser,
                            adminPassword);

                    if (!userResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            string.Join(
                                Environment.NewLine,
                                userResult.Errors));
                    }
                }
                else
                {
                    var requiresUpdate = false;

                    if (!string.Equals(
                        adminUser.DisplayName,
                        "システム管理者",
                        StringComparison.Ordinal))
                    {
                        adminUser.DisplayName =
                            "システム管理者";

                        requiresUpdate = true;
                    }

                    if (!adminUser.IsActive)
                    {
                        adminUser.IsActive = true;
                        requiresUpdate = true;
                    }

                    if (adminUser.CreatedAt == default(DateTime))
                    {
                        adminUser.CreatedAt =
                            DateTime.UtcNow;

                        requiresUpdate = true;
                    }

                    if (requiresUpdate)
                    {
                        var updateResult =
                            userManager.Update(adminUser);

                        if (!updateResult.Succeeded)
                        {
                            throw new InvalidOperationException(
                                string.Join(
                                    Environment.NewLine,
                                    updateResult.Errors));
                        }
                    }
                }

                if (!userManager.IsInRole(
                    adminUser.Id,
                    adminRoleName))
                {
                    var roleResult =
                        userManager.AddToRole(
                            adminUser.Id,
                            adminRoleName);

                    if (!roleResult.Succeeded)
                    {
                        throw new InvalidOperationException(
                            string.Join(
                                Environment.NewLine,
                                roleResult.Errors));
                    }
                }
            }
        }
    }
}