using System;
using FaqKnowledgeSearch.WebForms.App_Start;
using FaqKnowledgeSearch.WebForms.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Owin;

[assembly: OwinStartup(
    typeof(FaqKnowledgeSearch.WebForms.Startup))]

namespace FaqKnowledgeSearch.WebForms
{
    /// <summary>
    /// OWIN認証ミドルウェアを構成します。
    /// </summary>
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.CreatePerOwinContext(
                ApplicationIdentityDbContext.Create);

            app.CreatePerOwinContext<ApplicationUserManager>(
                ApplicationUserManager.Create);

            app.UseCookieAuthentication(
                new CookieAuthenticationOptions
                {
                    AuthenticationType =
                        DefaultAuthenticationTypes.ApplicationCookie,

                    CookieName = ".FaqKnowledgeSearch.Auth",

                    LoginPath =
                        new PathString("/Account/Login.aspx"),

                    ExpireTimeSpan =
                        TimeSpan.FromMinutes(30),

                    SlidingExpiration = true,
                    CookieHttpOnly = true,

                    CookieSecure =
                        CookieSecureOption.SameAsRequest,

                    Provider =
                        new CookieAuthenticationProvider
                        {
                            OnValidateIdentity =
                                SecurityStampValidator
                                    .OnValidateIdentity<
                                        ApplicationUserManager,
                                        ApplicationUser>(
                                        validateInterval:
                                            TimeSpan.FromMinutes(30),

                                        regenerateIdentity:
                                            (manager, user) =>
                                                user.GenerateUserIdentityAsync(
                                                    manager))
                        }
                });
        }
    }
}