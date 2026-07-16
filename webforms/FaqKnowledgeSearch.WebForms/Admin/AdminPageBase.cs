using System;
using System.Web;
using System.Web.UI;

namespace FaqKnowledgeSearch.WebForms.Admin
{
    /// <summary>
    /// 管理者向けページの共通基底クラスです。
    /// </summary>
    public abstract class AdminPageBase : Page
    {
        protected override void OnPreInit(EventArgs e)
        {
            if (Context?.User?.Identity?.IsAuthenticated != true)
            {
                RedirectToLogin();
                return;
            }

            if (!Context.User.IsInRole("Admin"))
            {
                throw new HttpException(
                    403,
                    "このページを表示するには管理者権限が必要です。");
            }

            base.OnPreInit(e);
        }

        private void RedirectToLogin()
        {
            var returnUrl =
                HttpUtility.UrlEncode(Request.RawUrl);

            var loginUrl =
                ResolveUrl("~/Account/Login.aspx")
                + "?ReturnUrl="
                + returnUrl;

            Response.Redirect(loginUrl, true);
        }
    }
}