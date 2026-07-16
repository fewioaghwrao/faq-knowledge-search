using System;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;

namespace FaqKnowledgeSearch.WebForms
{
    public partial class SiteMaster : System.Web.UI.MasterPage
    {
        protected void Page_Load(
            object sender,
            EventArgs e)
        {
        }

        protected void ConfirmLogoutButton_Click(
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

            Response.Redirect(
                "~/Default.aspx",
                false);

            Context.ApplicationInstance
                .CompleteRequest();
        }
    }
}