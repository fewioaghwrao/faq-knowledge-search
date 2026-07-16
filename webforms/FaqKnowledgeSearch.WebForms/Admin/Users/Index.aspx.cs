using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using FaqKnowledgeSearch.WebForms.Services;

namespace FaqKnowledgeSearch.WebForms.Admin.Users
{
    /// <summary>
    /// 管理者向けユーザー一覧画面です。
    /// </summary>
    public partial class Index : AdminPageBase
    {
        private const int PageSize = 5;

        private readonly IAdminUserService _adminUserService =
            new AdminUserService();

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadUsers();
            }
        }

        /// <summary>
        /// ユーザー一覧を読み込みます。
        /// </summary>
        private void LoadUsers()
        {
            var requestedPageNumber =
                GetRequestedPageNumber();

            var result =
                _adminUserService.GetPage(
                    requestedPageNumber,
                    PageSize);

            UsersRepeater.DataSource =
                result.Items;

            UsersRepeater.DataBind();

            TotalCountLabel.Text =
                result.TotalCount.ToString();

            EmptyPanel.Visible =
                result.TotalCount == 0;

            UsersRepeater.Visible =
                result.TotalCount > 0;

            ConfigurePager(result);
        }

        /// <summary>
        /// 有効・無効切り替えボタンの処理です。
        /// </summary>
        protected void ToggleStatusButton_Command(
            object sender,
            CommandEventArgs e)
        {
            var userId =
                Convert.ToString(e.CommandArgument);

            var shouldActivate =
                string.Equals(
                    e.CommandName,
                    "Enable",
                    StringComparison.OrdinalIgnoreCase);

            RegisterAsyncTask(
                new PageAsyncTask(
                    async () =>
                    {
                        HideMessages();

                        var result =
                            await _adminUserService
                                .SetActiveAsync(
                                    userId,
                                    shouldActivate);

                        if (!result.Succeeded)
                        {
                            ShowError(
                                string.Join(
                                    " ",
                                    result.Errors));

                            LoadUsers();
                            return;
                        }

                        ShowSuccess(
                            shouldActivate
                                ? "ユーザーを有効にしました。"
                                : "ユーザーを無効にしました。");

                        LoadUsers();
                    }));
        }

        /// <summary>
        /// クエリ文字列からページ番号を取得します。
        /// </summary>
        private int GetRequestedPageNumber()
        {
            int pageNumber;

            if (!int.TryParse(
                Request.QueryString["page"],
                out pageNumber))
            {
                return 1;
            }

            return Math.Max(pageNumber, 1);
        }

        /// <summary>
        /// ページャーを設定します。
        /// </summary>
        private void ConfigurePager(
            Dtos.AdminUserPageDto result)
        {
            PagerPanel.Visible =
                result.TotalPages > 1;

            if (result.TotalPages <= 1)
            {
                return;
            }

            PageInfoLabel.Text =
                string.Format(
                    "{0} / {1}",
                    result.PageNumber,
                    result.TotalPages);

            PreviousPageLink.Visible =
                result.HasPreviousPage;

            NextPageLink.Visible =
                result.HasNextPage;

            if (result.HasPreviousPage)
            {
                PreviousPageLink.NavigateUrl =
                    ResolveUrl(
                        string.Format(
                            "~/Admin/Users/Index.aspx?page={0}",
                            result.PageNumber - 1));
            }

            if (result.HasNextPage)
            {
                NextPageLink.NavigateUrl =
                    ResolveUrl(
                        string.Format(
                            "~/Admin/Users/Index.aspx?page={0}",
                            result.PageNumber + 1));
            }
        }

        protected string GetRoleCssClass(
            string roleName)
        {
            const string baseClass =
                "admin-role-badge";

            if (string.Equals(
                roleName,
                "Admin",
                StringComparison.OrdinalIgnoreCase))
            {
                return baseClass +
                    " admin-role-badge--admin";
            }

            if (string.Equals(
                roleName,
                "User",
                StringComparison.OrdinalIgnoreCase))
            {
                return baseClass +
                    " admin-role-badge--user";
            }

            return baseClass +
                " admin-role-badge--unassigned";
        }

        protected string GetStatusText(
            bool isActive)
        {
            return isActive
                ? "有効"
                : "無効";
        }

        protected string GetStatusCssClass(
            bool isActive)
        {
            const string baseClass =
                "admin-status-badge";

            return isActive
                ? baseClass +
                    " admin-status-badge--active"
                : baseClass +
                    " admin-status-badge--inactive";
        }

        protected string GetActionText(
            bool isActive)
        {
            return isActive
                ? "無効にする"
                : "有効にする";
        }

        protected string GetCommandName(
            bool isActive)
        {
            return isActive
                ? "Disable"
                : "Enable";
        }

        protected string GetActionCssClass(
            bool isActive)
        {
            const string baseClass =
                "admin-user-action";

            return isActive
                ? baseClass +
                    " admin-user-action--disable"
                : baseClass +
                    " admin-user-action--enable";
        }

        protected string GetConfirmationMessage(
            bool isActive)
        {
            return isActive
                ? "return confirm('このユーザーを無効にしますか？');"
                : "return confirm('このユーザーを有効にしますか？');";
        }

        protected string FormatCreatedAt(
            DateTime createdAt)
        {
            if (createdAt.Kind ==
                DateTimeKind.Unspecified)
            {
                createdAt =
                    DateTime.SpecifyKind(
                        createdAt,
                        DateTimeKind.Utc);
            }

            var japanTimeZone =
                TimeZoneInfo.FindSystemTimeZoneById(
                    "Tokyo Standard Time");

            var japanDateTime =
                TimeZoneInfo.ConvertTimeFromUtc(
                    createdAt.ToUniversalTime(),
                    japanTimeZone);

            return japanDateTime.ToString(
                "yyyy/MM/dd HH:mm");
        }

        private void ShowSuccess(
            string message)
        {
            SuccessMessageLabel.Text =
                Server.HtmlEncode(message);

            SuccessMessagePanel.Visible =
                true;

            ErrorMessagePanel.Visible =
                false;
        }

        private void ShowError(
            string message)
        {
            ErrorMessageLabel.Text =
                Server.HtmlEncode(message);

            ErrorMessagePanel.Visible =
                true;

            SuccessMessagePanel.Visible =
                false;
        }

        private void HideMessages()
        {
            SuccessMessagePanel.Visible =
                false;

            ErrorMessagePanel.Visible =
                false;
        }
    }
}