using System;
using System.Collections.Generic;
using System.Linq;
using FaqKnowledgeSearch.WebForms.Services;

namespace FaqKnowledgeSearch.WebForms.Faqs
{
    public partial class Detail : System.Web.UI.Page
    {
        private readonly IFaqService _faqService =
            new FaqService();

        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadFaq();
            }
        }

        /// <summary>
        /// クエリ文字列で指定されたFAQを取得し、
        /// 詳細画面へ表示します。
        /// </summary>
        private void LoadFaq()
        {
            long faqId;

            if (!long.TryParse(
                    Request.QueryString["id"],
                    out faqId) ||
                faqId <= 0)
            {
                ShowError(
                    "FAQ IDが正しくありません。",
                    400);

                return;
            }

            try
            {
                var faq = _faqService
                    .GetPublishedFaqById(faqId);

                if (faq == null)
                {
                    ShowError(
                        "指定されたFAQは見つかりませんでした。",
                        404);

                    return;
                }

                Page.Title = faq.Question;

                CategoryLiteral.Text =
                    faq.CategoryName;

                QuestionLiteral.Text =
                    faq.Question;

                AnswerLiteral.Text =
                    faq.Answer;

                /*
                 * カンマ区切りで取得したタグ名を分割し、
                 * 詳細画面のタグRepeaterへ設定する。
                 */
                TagRepeater.DataSource =
                    SplitTags(faq.TagNames);

                TagRepeater.DataBind();

                ViewCountLiteral.Text =
                    faq.ViewCount.ToString("N0");

                UpdatedAtLiteral.Text =
                    faq.UpdatedAt.ToString(
                        "yyyy/MM/dd HH:mm");

                FaqPanel.Visible = true;
                ErrorPanel.Visible = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    ex.ToString());

                ShowError(
                    "FAQの取得に失敗しました。",
                    500);
            }
        }

        /// <summary>
        /// カンマ区切りのタグ名を画面表示用の一覧へ変換します。
        /// </summary>
        private static IEnumerable<string> SplitTags(
            string tagNames)
        {
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
                .ToList();
        }

        /// <summary>
        /// エラーメッセージを表示します。
        /// </summary>
        private void ShowError(
            string message,
            int statusCode)
        {
            Response.StatusCode = statusCode;
            Response.TrySkipIisCustomErrors = true;

            ErrorMessageLiteral.Text = message;

            TagRepeater.DataSource = null;
            TagRepeater.DataBind();

            ErrorPanel.Visible = true;
            FaqPanel.Visible = false;
        }
    }
}