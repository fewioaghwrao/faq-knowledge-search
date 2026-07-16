using System;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace FaqKnowledgeSearch.WebForms.Account
{
    public partial class Logout : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack && !Request.IsAuthenticated)
            {
                RedirectToTop();
            }
        }

        protected void LogoutButton_Click(
            object sender,
            EventArgs e)
        {
            var authentication =
                Context.GetOwinContext().Authentication;

            authentication.SignOut(
                DefaultAuthenticationTypes.ApplicationCookie);

            Session.Clear();
            Session.Abandon();

            Response.Cache.SetCacheability(
                HttpCacheability.NoCache);

            Response.Cache.SetNoStore();

            RedirectToTop();
        }

        private void RedirectToTop()
        {
            Response.Redirect(
                "~/Default.aspx",
                false);

            Context.ApplicationInstance.CompleteRequest();
        }
    }
}