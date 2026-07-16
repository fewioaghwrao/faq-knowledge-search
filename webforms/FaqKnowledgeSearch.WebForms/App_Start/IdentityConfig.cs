using System;
using FaqKnowledgeSearch.WebForms.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;

namespace FaqKnowledgeSearch.WebForms.App_Start
{
    /// <summary>
    /// ASP.NET Identityのユーザー管理処理を提供します。
    /// </summary>
    public class ApplicationUserManager : UserManager<ApplicationUser>
    {
        public ApplicationUserManager(
            IUserStore<ApplicationUser> store)
            : base(store)
        {
        }

        public static ApplicationUserManager Create(
            IdentityFactoryOptions<ApplicationUserManager> options,
            IOwinContext context)
        {
            var identityDbContext =
                context.Get<ApplicationIdentityDbContext>();

            var userStore =
                new UserStore<ApplicationUser>(identityDbContext);

            var manager =
                new ApplicationUserManager(userStore);

            manager.UserValidator =
                new UserValidator<ApplicationUser>(manager)
                {
                    AllowOnlyAlphanumericUserNames = false,
                    RequireUniqueEmail = true
                };

            manager.PasswordValidator =
                new PasswordValidator
                {
                    RequiredLength = 8,
                    RequireNonLetterOrDigit = true,
                    RequireDigit = true,
                    RequireLowercase = true,
                    RequireUppercase = true
                };

            manager.UserLockoutEnabledByDefault = true;
            manager.DefaultAccountLockoutTimeSpan =
                TimeSpan.FromMinutes(10);
            manager.MaxFailedAccessAttemptsBeforeLockout = 5;

            return manager;
        }
    }
}