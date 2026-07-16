using System;
using System.Web.UI;

namespace FaqKnowledgeSearch.WebForms.Admin
{
    public partial class AdminHeader : UserControl
    {
        public string ActiveItem { get; set; }



        protected override void OnPreRender(
            EventArgs e)
        {
            base.OnPreRender(e);

            FaqListLink.CssClass =
                BuildButtonClass("faq");

            NewFaqLink.CssClass =
                BuildButtonClass("new");

            UserListLink.CssClass =
                BuildButtonClass("users");
        }

        private string BuildButtonClass(
            string itemName)
        {
            const string baseClass =
                "admin-menu-button";

            if (string.Equals(
                ActiveItem,
                itemName,
                StringComparison.OrdinalIgnoreCase))
            {
                return baseClass +
                    " admin-menu-button--active";
            }

            return baseClass;
        }
    }
}