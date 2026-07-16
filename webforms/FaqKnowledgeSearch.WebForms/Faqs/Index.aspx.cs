using System;
using System.Collections.Generic;
using System.Linq;
using FaqKnowledgeSearch.WebForms.Services;

namespace FaqKnowledgeSearch.WebForms.Faqs
{
    public partial class Index : System.Web.UI.Page
    {
        private const int PageSize = 5;

        private readonly IFaqService _faqService =
            new FaqService();

        /// <summary>
        /// 現在表示しているページ番号。
        /// 内部では0始まりで管理する。
        /// </summary>
        private int CurrentPageIndex
        {
            get
            {
                if (ViewState["CurrentPageIndex"] == null)
                {
                    return 0;
                }

                return (int)ViewState["CurrentPageIndex"];
            }
            set
            {
                ViewState["CurrentPageIndex"] = value;
            }
        }

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            if (!IsPostBack)
            {
                CurrentPageIndex = 0;
                BindFaqs();
            }
        }

        protected void SearchButton_Click(
            object sender,
            EventArgs e)
        {
            // 検索条件が変わるため1ページ目へ戻す
            CurrentPageIndex = 0;

            BindFaqs();
        }

        protected void ClearButton_Click(
            object sender,
            EventArgs e)
        {
            KeywordTextBox.Text = string.Empty;
            CurrentPageIndex = 0;

            BindFaqs();
        }

        protected void PreviousButton_Click(
            object sender,
            EventArgs e)
        {
            if (CurrentPageIndex > 0)
            {
                CurrentPageIndex--;
            }

            BindFaqs();
        }

        protected void NextButton_Click(
            object sender,
            EventArgs e)
        {
            CurrentPageIndex++;

            BindFaqs();
        }

        private void BindFaqs()
        {
            try
            {
                string keyword =
                    KeywordTextBox.Text.Trim();

                var items =
                    _faqService.SearchPublishedFaqs(keyword);

                int totalCount = items.Count;

                int totalPages = (int)Math.Ceiling(
                    totalCount / (double)PageSize);

                // 削除などによって現在ページが範囲外になった場合の補正
                if (totalPages == 0)
                {
                    CurrentPageIndex = 0;
                }
                else if (CurrentPageIndex >= totalPages)
                {
                    CurrentPageIndex = totalPages - 1;
                }

                var pageItems = items
                    .Skip(CurrentPageIndex * PageSize)
                    .Take(PageSize)
                    .ToList();

                FaqRepeater.DataSource = pageItems;
                FaqRepeater.DataBind();

                ResultCountLabel.Text =
                    string.Format(
                        "{0}件",
                        totalCount);

                EmptyPanel.Visible =
                    totalCount == 0;

                PagerPanel.Visible =
                    totalPages > 1;

                if (totalPages > 0)
                {
                    PageStatusLabel.Text =
                        string.Format(
                            "{0} / {1}",
                            CurrentPageIndex + 1,
                            totalPages);
                }
                else
                {
                    PageStatusLabel.Text =
                        string.Empty;
                }

                PreviousButton.Enabled =
                    CurrentPageIndex > 0;

                NextButton.Enabled =
                    totalPages > 0 &&
                    CurrentPageIndex < totalPages - 1;

                // 正常時は画像に合わせて成功メッセージを表示しない
                MessageLabel.Text =
                    string.Empty;

                MessageLabel.CssClass =
                    "faq-message";
            }
            catch (Exception ex)
            {
                FaqRepeater.DataSource = null;
                FaqRepeater.DataBind();

                ResultCountLabel.Text = "0件";
                EmptyPanel.Visible = false;
                PagerPanel.Visible = false;

                MessageLabel.CssClass =
                    "faq-message faq-message--error";

                MessageLabel.Text =
                    "FAQの取得に失敗しました。詳細: " +
                    Server.HtmlEncode(
                        ex.GetBaseException().Message);

                System.Diagnostics.Debug.WriteLine(
                    ex.ToString());
            }
        }

        /// <summary>
        /// FAQ回答を一覧表示用の短い文章に変換する。
        /// </summary>
        protected string GetSummary(object value)
        {
            string answer =
                Convert.ToString(value);

            if (string.IsNullOrWhiteSpace(answer))
            {
                return "回答内容は詳細画面で確認できます。";
            }

            answer = answer
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Trim();

            const int maxLength = 140;

            if (answer.Length <= maxLength)
            {
                return answer;
            }

            return answer.Substring(
                0,
                maxLength) + "...";
        }

        /// <summary>
        /// カンマ区切りのタグ名を一覧へ変換する。
        /// 一覧画面では最大5件まで表示する。
        /// </summary>
        protected IEnumerable<string> SplitTags(
            object value)
        {
            string tagNames =
                Convert.ToString(value);

            if (string.IsNullOrWhiteSpace(tagNames))
            {
                return Enumerable.Empty<string>();
            }

            return tagNames
                .Split(
                    new[]
                    {
                ',',
                '、',
                '\r',
                '\n'
                    },
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(
                    tagName =>
                        tagName
                            .Trim()
                            .TrimStart('#'))
                .Where(
                    tagName =>
                        !string.IsNullOrWhiteSpace(tagName))
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .Take(5)
                .ToList();
        }
    }
}