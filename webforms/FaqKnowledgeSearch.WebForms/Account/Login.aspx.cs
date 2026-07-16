using System;
using System.Threading.Tasks;
using System.Web;
using FaqKnowledgeSearch.WebForms.App_Start;
using FaqKnowledgeSearch.WebForms.Identity;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;

namespace FaqKnowledgeSearch.WebForms.Account
{
    public partial class Login : System.Web.UI.Page
    {
        private const string AdminRoleName = "Admin";

        protected void Page_Load(object sender, EventArgs e)
        {
            LoginErrorPanel.Visible = false;

            if (!IsPostBack && Request.IsAuthenticated)
            {
                RedirectAfterLogin();
            }
        }

        protected async void LoginButton_Click(
            object sender,
            EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            LoginErrorPanel.Visible = false;

            var email = EmailTextBox.Text.Trim();
            var password = PasswordTextBox.Text;

            var owinContext = Context.GetOwinContext();

            var userManager =
                owinContext.GetUserManager<ApplicationUserManager>();

            var authentication =
                owinContext.Authentication;

            var user =
                await userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ShowLoginError();
                return;
            }

            if (await userManager.IsLockedOutAsync(user.Id))
            {
                ShowLoginError(
                    "ログイン試行回数の上限に達しました。しばらくしてから再度お試しください。");

                return;
            }

            var passwordIsValid =
                await userManager.CheckPasswordAsync(
                    user,
                    password);

            if (!passwordIsValid)
            {
                await userManager.AccessFailedAsync(user.Id);

                if (await userManager.IsLockedOutAsync(user.Id))
                {
                    ShowLoginError(
                        "ログイン試行回数の上限に達しました。しばらくしてから再度お試しください。");
                }
                else
                {
                    ShowLoginError();
                }

                return;
            }

            var isAdmin =
                await userManager.IsInRoleAsync(
                    user.Id,
                    AdminRoleName);

            if (!isAdmin)
            {
                ShowLoginError();
                return;
            }

            await userManager.ResetAccessFailedCountAsync(user.Id);

            var identity =
                await user.GenerateUserIdentityAsync(
                    userManager);

            authentication.SignOut(
                DefaultAuthenticationTypes.ApplicationCookie);

            authentication.SignIn(
                new AuthenticationProperties
                {
                    IsPersistent =
                        RememberMeCheckBox.Checked,

                    AllowRefresh = true
                },
                identity);

            RedirectAfterLogin();
        }

        private void ShowLoginError(
            string message =
                "メールアドレスまたはパスワードが正しくありません。")
        {
            LoginErrorMessage.Text =
                HttpUtility.HtmlEncode(message);

            LoginErrorPanel.Visible = true;
        }

        private void RedirectAfterLogin()
        {
            var returnUrl =
                Request.QueryString["ReturnUrl"];

            if (IsLocalUrl(returnUrl))
            {
                Response.Redirect(returnUrl, false);
            }
            else
            {
                Response.Redirect(
                    "~/Admin/Default.aspx",
                    false);
            }

            Context.ApplicationInstance.CompleteRequest();
        }

        private static bool IsLocalUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            return
                url[0] == '/' &&
                (url.Length == 1 ||
                 url[1] != '/' &&
                 url[1] != '\\') ||

                url.Length > 1 &&
                url[0] == '~' &&
                url[1] == '/';
        }
    }
}