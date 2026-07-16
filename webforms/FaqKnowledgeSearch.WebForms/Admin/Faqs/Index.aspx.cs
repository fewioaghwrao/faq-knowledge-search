using System;
using System.Web.UI.WebControls;
using FaqKnowledgeSearch.WebForms.Services;

namespace FaqKnowledgeSearch.WebForms.Admin.Faqs
{
    public partial class Index : AdminPageBase
    {
        private const int PageSize = 5;

        private readonly IFaqService _faqService;

        public Index()
        {
            _faqService = new FaqService();
        }

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }

            RestoreSearchConditions();

            FaqGridView.PageSize = PageSize;
            FaqGridView.PageIndex = 0;

            BindFaqs();
            ShowQueryMessage();
        }

        protected void SearchButton_Click(
            object sender,
            EventArgs e)
        {
            FaqGridView.PageIndex = 0;

            BindFaqs();
        }

        protected void ClearButton_Click(
            object sender,
            EventArgs e)
        {
            KeywordTextBox.Text = string.Empty;

            PublishStatusDropDownList.SelectedIndex = 0;

            FaqGridView.PageIndex = 0;

            BindFaqs();
        }

        protected void PreviousPageButton_Click(
            object sender,
            EventArgs e)
        {
            if (FaqGridView.PageIndex > 0)
            {
                FaqGridView.PageIndex--;
            }

            BindFaqs();
        }

        protected void NextPageButton_Click(
            object sender,
            EventArgs e)
        {
            if (FaqGridView.PageIndex <
                FaqGridView.PageCount - 1)
            {
                FaqGridView.PageIndex++;
            }

            BindFaqs();
        }

        /// <summary>
        /// 公開状態の表示文字列を返します。
        /// </summary>
        protected string GetPublishStatusText(
            bool isPublished)
        {
            return isPublished
                ? "公開"
                : "非公開";
        }

        /// <summary>
        /// 公開状態に応じたBootstrapクラスを返します。
        /// </summary>
        protected string GetPublishStatusCssClass(
            bool isPublished)
        {
            const string baseClass =
                "badge rounded-pill border px-3 py-2 fw-semibold";

            if (isPublished)
            {
                return baseClass +
                    " text-success" +
                    " border-success" +
                    " bg-success bg-opacity-10";
            }

            return baseClass +
                " text-secondary" +
                " border-secondary" +
                " bg-secondary bg-opacity-10";
        }

        /// <summary>
        /// カテゴリ名に応じたBootstrapクラスを返します。
        /// </summary>
        protected string GetCategoryCssClass(
            string categoryName)
        {
            const string baseClass =
                "badge rounded-pill border px-3 py-2 fw-semibold";

            if (string.IsNullOrWhiteSpace(categoryName))
            {
                return baseClass +
                    " text-secondary" +
                    " border-secondary" +
                    " bg-secondary bg-opacity-10";
            }

            // ログイン、認証、アカウント関連
            if (ContainsAny(
                categoryName,
                "ログイン",
                "認証",
                "アカウント",
                "パスワード"))
            {
                return baseClass +
                    " text-primary" +
                    " border-primary" +
                    " bg-primary bg-opacity-10";
            }

            // システム設定、操作関連
            if (ContainsAny(
                categoryName,
                "システム",
                "設定",
                "操作",
                "管理"))
            {
                return baseClass +
                    " text-info" +
                    " border-info" +
                    " bg-info bg-opacity-10";
            }

            // メール、通知関連
            if (ContainsAny(
                categoryName,
                "メール",
                "通知",
                "連絡"))
            {
                return baseClass +
                    " text-warning" +
                    " border-warning" +
                    " bg-warning bg-opacity-10";
            }

            // 請求、入金、帳票関連
            if (ContainsAny(
                categoryName,
                "請求",
                "入金",
                "支払",
                "PDF",
                "CSV",
                "帳票"))
            {
                return baseClass +
                    " text-success" +
                    " border-success" +
                    " bg-success bg-opacity-10";
            }

            // 障害、エラー関連
            if (ContainsAny(
                categoryName,
                "障害",
                "エラー",
                "不具合",
                "トラブル"))
            {
                return baseClass +
                    " text-danger" +
                    " border-danger" +
                    " bg-danger bg-opacity-10";
            }

            // その他
            return baseClass +
                " text-light" +
                " border-secondary" +
                " bg-secondary bg-opacity-10";
        }

        /// <summary>
        /// FAQ本文を一覧表示用に短縮します。
        /// </summary>
        protected string GetAnswerPreview(
            object answerValue)
        {
            var answer =
                Convert.ToString(answerValue);

            if (string.IsNullOrWhiteSpace(answer))
            {
                return "本文は登録されていません。";
            }

            answer = answer
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Trim();

            const int maximumLength = 105;

            if (answer.Length <= maximumLength)
            {
                return answer;
            }

            return answer.Substring(
                0,
                maximumLength) + "...";
        }

        protected void FaqGridView_RowCommand(
            object sender,
            GridViewCommandEventArgs e)
        {
            if (e.CommandName != "DeleteFaq")
            {
                return;
            }

            long faqId;

            if (!long.TryParse(
                Convert.ToString(e.CommandArgument),
                out faqId))
            {
                ShowErrorMessage(
                    "削除対象のFAQ IDが正しくありません。");

                return;
            }

            try
            {
                var deleted =
                    _faqService.DeleteFaq(faqId);

                if (!deleted)
                {
                    ShowErrorMessage(
                        "削除対象のFAQが見つかりません。");

                    BindFaqs();

                    return;
                }

                Response.Redirect(
                    "~/Admin/Faqs/Index.aspx?message=deleted",
                    false);

                Context.ApplicationInstance
                    .CompleteRequest();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError(
                    ex.ToString());

                ShowErrorMessage(
                    "FAQの削除中にエラーが発生しました。");
            }
        }

        private void BindFaqs()
        {
            var keyword =
                KeywordTextBox.Text.Trim();

            var isPublished =
                GetSelectedPublishStatus();

            var faqs =
                _faqService.SearchFaqsForAdmin(
                    keyword,
                    isPublished);

            CorrectPageIndex(faqs.Count);

            FaqGridView.DataSource = faqs;
            FaqGridView.DataBind();

            UpdateResultCount(faqs.Count);
            UpdatePager(faqs.Count);
        }

        private void CorrectPageIndex(
            int totalCount)
        {
            if (totalCount <= 0)
            {
                FaqGridView.PageIndex = 0;

                return;
            }

            var pageCount =
                (int)Math.Ceiling(
                    totalCount /
                    (double)FaqGridView.PageSize);

            if (FaqGridView.PageIndex >= pageCount)
            {
                FaqGridView.PageIndex =
                    pageCount - 1;
            }

            if (FaqGridView.PageIndex < 0)
            {
                FaqGridView.PageIndex = 0;
            }
        }

        private void UpdateResultCount(
            int totalCount)
        {
            ResultCountLabel.Text =
                string.Format(
                    "登録件数 {0}件",
                    totalCount);
        }

        private void UpdatePager(
            int totalCount)
        {
            if (totalCount <= 0)
            {
                PageStatusLabel.Text = "0 / 0";

                PreviousPageButton.Enabled = false;
                NextPageButton.Enabled = false;

                return;
            }

            var pageCount =
                (int)Math.Ceiling(
                    totalCount /
                    (double)FaqGridView.PageSize);

            PageStatusLabel.Text =
                string.Format(
                    "{0} / {1}",
                    FaqGridView.PageIndex + 1,
                    pageCount);

            PreviousPageButton.Enabled =
                FaqGridView.PageIndex > 0;

            NextPageButton.Enabled =
                FaqGridView.PageIndex <
                pageCount - 1;
        }

        private bool? GetSelectedPublishStatus()
        {
            switch (
                PublishStatusDropDownList.SelectedValue)
            {
                case "published":
                    return true;

                case "unpublished":
                    return false;

                default:
                    return null;
            }
        }

        private void RestoreSearchConditions()
        {
            KeywordTextBox.Text =
                Request.QueryString["keyword"]
                ?? string.Empty;

            var status =
                Request.QueryString["status"];

            if (status == "published" ||
                status == "unpublished")
            {
                PublishStatusDropDownList
                    .SelectedValue = status;
            }
        }

        private void ShowQueryMessage()
        {
            var message =
                Request.QueryString["message"];

            switch (message)
            {
                case "created":
                    ShowMessage(
                        "FAQを登録しました。");
                    break;

                case "updated":
                    ShowMessage(
                        "FAQを更新しました。");
                    break;

                case "deleted":
                    ShowMessage(
                        "FAQを削除しました。");
                    break;
            }
        }

        private void ShowMessage(
            string message)
        {
            MessageLiteral.Text =
                Server.HtmlEncode(message);

            MessagePanel.CssClass =
                "admin-message admin-message--success";

            MessagePanel.Visible = true;
        }

        private void ShowErrorMessage(
            string message)
        {
            MessageLiteral.Text =
                Server.HtmlEncode(message);

            MessagePanel.CssClass =
                "admin-message admin-message--error";

            MessagePanel.Visible = true;
        }

        /// <summary>
        /// 対象文字列に指定したキーワードが含まれるか判定します。
        /// </summary>
        private static bool ContainsAny(
            string source,
            params string[] keywords)
        {
            if (string.IsNullOrWhiteSpace(source) ||
                keywords == null)
            {
                return false;
            }

            foreach (var keyword in keywords)
            {
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    continue;
                }

                if (source.IndexOf(
                    keyword,
                    StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}