using System;

namespace FaqKnowledgeSearch.WebForms.ErrorPages
{
    public partial class InternalServerError : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.StatusCode = 500;
            Response.TrySkipIisCustomErrors = true;
        }
    }
}