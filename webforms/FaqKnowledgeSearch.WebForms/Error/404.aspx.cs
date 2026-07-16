using System;

namespace FaqKnowledgeSearch.WebForms.ErrorPages
{
    public partial class NotFound : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
        }
    }
}