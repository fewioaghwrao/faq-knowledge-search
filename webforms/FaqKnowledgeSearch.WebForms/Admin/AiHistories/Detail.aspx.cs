using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using FaqKnowledgeSearch.WebForms.Data;

namespace FaqKnowledgeSearch.WebForms.Admin.AiHistories
{
    public partial class Detail : AdminPageBase
    {
        protected void Page_Load(
            object sender,
            EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadHistoryDetail();
            }
        }

        private void LoadHistoryDetail()
        {
            long historyId;

            if (!long.TryParse(
                    Request.QueryString["id"],
                    out historyId) ||
                historyId <= 0)
            {
                ShowError(
                    "AI検索履歴IDが正しくありません。");

                return;
            }

            try
            {
                using (var db =
                    new FaqKnowledgeDbContext())
                {
                    var history =
                        db.AiSearchHistories
                            .AsNoTracking()
                            .Include(x => x.Sources)
                            .SingleOrDefault(
                                x => x.Id == historyId);

                    if (history == null)
                    {
                        Response.StatusCode = 404;
                        Response.TrySkipIisCustomErrors = true;

                        ShowError(
                            "指定されたAI検索履歴は存在しません。");

                        return;
                    }

                    var isHelpful =
    db.AiSearchFeedbacks
        .AsNoTracking()
        .Where(x =>
            x.AiSearchHistoryId ==
            history.Id)
        .Select(x =>
            (bool?)x.IsHelpful)
        .FirstOrDefault();

                    var sources =
                        history.Sources == null
                            ? new List<SourceViewModel>()
                            : history.Sources
                                .OrderBy(x =>
                                    x.DisplayOrder)
                                .Select(x =>
                                    new SourceViewModel
                                    {
                                        FaqId =
                                            x.FaqId,

                                        DisplayOrder =
                                            x.DisplayOrder,

                                        FaqQuestion =
                                            string.IsNullOrWhiteSpace(
                                                x.FaqQuestion)
                                                ? "質問未設定"
                                                : x.FaqQuestion.Trim(),

                                        CategoryName =
                                            string.IsNullOrWhiteSpace(
                                                x.CategoryName)
                                                ? "未分類"
                                                : x.CategoryName.Trim(),

                                        AnswerPreview =
                                            CreatePreview(
                                                x.FaqAnswer,
                                                160)
                                    })
                                .ToList();

                    BindHistory(
                        history.Id,
                        history.Question,
                        history.SearchKeywords,
                        history.AiAnswer,
                        history.IsSuccess,
                        history.ErrorMessage,
                        history.ExecutedAt,
                        isHelpful,
                        sources);
                }
            }
            catch (Exception ex)
            {
                Trace.Warn(
                    "AdminAiHistoryDetail",
                    "AI検索履歴詳細の取得中にエラーが発生しました。",
                    ex);

                ShowError(
                    "AI検索履歴詳細を取得できませんでした。");
            }
        }

        private void BindHistory(
            long historyId,
            string question,
            string searchKeywords,
            string aiAnswer,
            bool isSuccess,
            string errorMessage,
            DateTime executedAt,
            bool? isHelpful,
            IList<SourceViewModel> sources)
        {
            HistoryIdLiteral.Text =
                historyId.ToString();

            StatusLabel.Text =
                isSuccess
                    ? "成功"
                    : "失敗";

            StatusLabel.CssClass =
                isSuccess
                    ? "admin-ai-detail-badge " +
                      "admin-ai-detail-badge--success"
                    : "admin-ai-detail-badge " +
                      "admin-ai-detail-badge--failure";

            SourceCountLiteral.Text =
                sources.Count.ToString();

            ExecutedAtLiteral.Text =
                FormatExecutedAt(
                    executedAt);

            QuestionLiteral.Text =
                question ?? string.Empty;

            /*
             * 現在は質問文と検索キーワードが
             * 同じ値なので、完全に同じ場合は
             * 重複表示を避ける。
             */
            var normalizedQuestion =
                (question ?? string.Empty).Trim();

            var normalizedKeywords =
                (searchKeywords ?? string.Empty).Trim();

            SearchKeywordsSection.Visible =
                !string.IsNullOrWhiteSpace(
                    normalizedKeywords) &&
                !string.Equals(
                    normalizedQuestion,
                    normalizedKeywords,
                    StringComparison.Ordinal);

            SearchKeywordsLiteral.Text =
                normalizedKeywords;

            var hasAnswer =
                !string.IsNullOrWhiteSpace(
                    aiAnswer);

            AnswerPanel.Visible =
                hasAnswer;

            AnswerEmptyPanel.Visible =
                !hasAnswer;

            AnswerLiteral.Text =
                hasAnswer
                    ? aiAnswer.Trim()
                    : string.Empty;

            var hasError =
                !string.IsNullOrWhiteSpace(
                    errorMessage);

            ErrorSection.Visible =
                hasError;

            ErrorMessageLiteral.Text =
                hasError
                    ? errorMessage.Trim()
                    : string.Empty;

            FeedbackLiteral.Text =
                GetFeedbackText(
                    isHelpful);

            SourceRepeater.DataSource =
                sources;

            SourceRepeater.DataBind();

            SourcesEmptyPanel.Visible =
                sources.Count == 0;

            DetailContentPanel.Visible =
                true;
        }

        private void ShowError(
            string message)
        {
            DetailContentPanel.Visible =
                false;

            MessageLiteral.Text =
                message ?? string.Empty;

            MessagePanel.Visible =
                true;
        }

        private static string FormatExecutedAt(
            DateTime executedAt)
        {
            try
            {
                /*
                 * MySQLから読み込んだDateTimeは
                 * KindがUnspecifiedになることがあるため、
                 * UTCとして明示してから日本時間へ変換する。
                 */
                var utcDateTime =
                    DateTime.SpecifyKind(
                        executedAt,
                        DateTimeKind.Utc);

                var japanTimeZone =
                    TimeZoneInfo.FindSystemTimeZoneById(
                        "Tokyo Standard Time");

                var japanDateTime =
                    TimeZoneInfo.ConvertTimeFromUtc(
                        utcDateTime,
                        japanTimeZone);

                return japanDateTime.ToString(
                    "yyyy/MM/dd HH:mm:ss");
            }
            catch
            {
                return executedAt.ToString(
                    "yyyy/MM/dd HH:mm:ss");
            }
        }

        private static string CreatePreview(
            string value,
            int maximumLength)
        {
            if (string.IsNullOrWhiteSpace(
                    value))
            {
                return "回答は保存されていません。";
            }

            var normalized =
                value
                    .Replace("\r\n", " ")
                    .Replace("\n", " ")
                    .Trim();

            if (normalized.Length <=
                maximumLength)
            {
                return normalized;
            }

            return normalized.Substring(
                       0,
                       maximumLength) +
                   "...";
        }

        private sealed class SourceViewModel
        {
            public long FaqId { get; set; }

            public int DisplayOrder { get; set; }

            public string FaqQuestion { get; set; }

            public string CategoryName { get; set; }

            public string AnswerPreview { get; set; }
        }

        private static string GetFeedbackText(
    bool? isHelpful)
        {
            if (!isHelpful.HasValue)
            {
                return "未評価";
            }

            return isHelpful.Value
                ? "役に立った 👍"
                : "役に立たなかった 👎";
        }
    }
}