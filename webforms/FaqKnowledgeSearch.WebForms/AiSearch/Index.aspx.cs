using FaqKnowledgeSearch.WebForms.Services;
using FaqKnowledgeSearch.WebForms.Services.Ai;
using FaqKnowledgeSearch.WebForms.Settings;
using System;
using System.Threading.Tasks;
using System.Web;

namespace FaqKnowledgeSearch.WebForms.AiSearch
{
    public partial class Index : System.Web.UI.Page
    {
        private const int RateLimitCount = 5;

        private static readonly TimeSpan RateLimitWindow =
            TimeSpan.FromMinutes(1);

        /*
         * キャッシュ内のカウンターを更新するときの競合を防止する。
         * AI検索開始時だけの短いロックなので、
         * ポートフォリオ規模では問題にならない。
         */
        private static readonly object RateLimitSync =
            new object();

        private IAiService _aiService;

        private IAiSearchFeedbackService
    _aiSearchFeedbackService;
        protected void Page_Init(
            object sender,
            EventArgs e)
        {
            var settings =
                AiSettings.Load();

            var faqService =
                new FaqService();

            var aiApiClient =
                new AiApiClient(settings);

            var aiSearchHistoryService =
                new AiSearchHistoryService();

            _aiSearchFeedbackService =
                new AiSearchFeedbackService();

            _aiService =
                new AiService(
                    faqService,
                    aiApiClient,
                    aiSearchHistoryService,
                    settings);
        }

        protected async void SearchButton_Click(
            object sender,
            EventArgs e)
        {
            MessagePanel.Visible =
                false;

            ResultPanel.Visible =
                false;

            HistoryIdHiddenField.Value =
                string.Empty;

            FeedbackPanel.Visible =
                false;

            FeedbackCompletePanel.Visible =
                false;

            FeedbackCompleteLiteral.Text =
                string.Empty;

            if (!Page.IsValid)
            {
                return;
            }

            var question =
                (QuestionTextBox.Text ??
                 string.Empty).Trim();

            int retryAfterSeconds;

            if (!TryConsumeAiSearchRequest(
                    out retryAfterSeconds))
            {
                ShowMessage(
                    "AI検索は1分間に5回まで利用できます。" +
                    retryAfterSeconds +
                    "秒ほど待ってから再度お試しください。");

                return;
            }

            SearchButton.Enabled =
                false;

            try
            {
                var result =
                    await _aiService.SearchAsync(
                        question);

                if (result.AiHistoryId > 0 &&
                    !string.IsNullOrWhiteSpace(
                        result.Answer))
                {
                    HistoryIdHiddenField.Value =
                        result.AiHistoryId.ToString();

                    FeedbackPanel.Visible =
                        true;

                    FeedbackCompletePanel.Visible =
                        false;
                }
                else
                {
                    HistoryIdHiddenField.Value =
                        string.Empty;

                    FeedbackPanel.Visible =
                        false;
                }

                if (!string.IsNullOrWhiteSpace(
                        result.Message))
                {
                    ShowMessage(
                        result.Message);
                }

                if (!string.IsNullOrWhiteSpace(
                        result.Answer))
                {
                    AnswerLiteral.Text =
                        FormatPlainText(
                            result.Answer);

                    DisclaimerLiteral.Text =
                        HttpUtility.HtmlEncode(
                            result.Disclaimer ??
                            string.Empty);

                    SourceRepeater.DataSource =
                        result.Sources;

                    SourceRepeater.DataBind();

                    ResultPanel.Visible =
                        true;
                }
                else if (result.Sources != null &&
                         result.Sources.Count > 0)
                {
                    /*
                     * AI APIが失敗した場合でも、
                     * 参照元FAQは表示する。
                     */
                    AnswerLiteral.Text =
                        string.Empty;

                    DisclaimerLiteral.Text =
                        string.Empty;

                    SourceRepeater.DataSource =
                        result.Sources;

                    SourceRepeater.DataBind();

                    ResultPanel.Visible =
                        true;

                    /*
                     * AI回答がないため、
                     * フィードバック対象にはしない。
                     */
                    FeedbackPanel.Visible =
                        false;
                }
            }
            catch (Exception ex)
            {
                Trace.Warn(
                    "AiSearch",
                    "AI FAQ検索中にエラーが発生しました。",
                    ex);

                ShowMessage(
                    "AI検索を実行できませんでした。" +
                    "時間をおいて再度お試しください。");
            }
            finally
            {
                SearchButton.Enabled =
                    true;
            }
        }

        /// <summary>
        /// クライアントIP単位で、
        /// 1分間に5回までAI検索を許可します。
        /// </summary>
        private bool TryConsumeAiSearchRequest(
            out int retryAfterSeconds)
        {
            retryAfterSeconds = 0;

            var cacheKey =
                GetRateLimitCacheKey();

            var now =
                DateTime.UtcNow;

            lock (RateLimitSync)
            {
                var entry =
                    HttpRuntime.Cache[cacheKey]
                    as AiSearchRateLimitEntry;

                /*
                 * 初回アクセス、または期限切れの場合は
                 * 新しい1分間の計測を開始する。
                 */
                if (entry == null ||
                    entry.ExpiresAtUtc <= now)
                {
                    var expiresAtUtc =
                        now.Add(RateLimitWindow);

                    entry =
                        new AiSearchRateLimitEntry
                        {
                            Count = 1,
                            ExpiresAtUtc =
                                expiresAtUtc
                        };

                    HttpRuntime.Cache.Insert(
                        cacheKey,
                        entry,
                        null,
                        expiresAtUtc,
                        System.Web.Caching.Cache
                            .NoSlidingExpiration);

                    return true;
                }

                if (entry.Count >= RateLimitCount)
                {
                    retryAfterSeconds =
                        Math.Max(
                            1,
                            (int)Math.Ceiling(
                                (entry.ExpiresAtUtc - now)
                                .TotalSeconds));

                    return false;
                }

                entry.Count += 1;

                return true;
            }
        }

        private string GetRateLimitCacheKey()
        {
            /*
             * リバースプロキシを信用する設定がない状態で
             * X-Forwarded-Forを直接使用すると偽装される可能性があるため、
             * まずはASP.NETが認識した接続元IPを使用する。
             */
            var ipAddress =
                Request.UserHostAddress;

            if (string.IsNullOrWhiteSpace(
                    ipAddress))
            {
                ipAddress = "unknown";
            }

            return "ai-search-rate-limit:"
                   + ipAddress;
        }

        private void ShowMessage(
            string message)
        {
            MessageLiteral.Text =
                HttpUtility.HtmlEncode(
                    message ??
                    string.Empty);

            MessagePanel.Visible = true;
        }

        private static string FormatPlainText(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return HttpUtility
                .HtmlEncode(value)
                .Replace(
                    "\r\n",
                    "<br />")
                .Replace(
                    "\n",
                    "<br />");
        }

        private sealed class AiSearchRateLimitEntry
        {
            public int Count { get; set; }

            public DateTime ExpiresAtUtc { get; set; }
        }

        private async Task SaveFeedbackAsync(
    bool isHelpful)
        {
            long historyId;

            if (!long.TryParse(
                    HistoryIdHiddenField.Value,
                    out historyId) ||
                historyId <= 0)
            {
                ShowMessage(
                    "評価対象のAI検索履歴を確認できませんでした。");

                return;
            }

            HelpfulButton.Enabled =
                false;

            NotHelpfulButton.Enabled =
                false;

            try
            {
                await _aiSearchFeedbackService
                    .SaveAsync(
                        historyId,
                        isHelpful,
                        null);

                FeedbackCompleteLiteral.Text =
                    isHelpful
                        ? "「役に立った」として送信しました。"
                        : "「役に立たなかった」として送信しました。";

                FeedbackCompletePanel.Visible =
                    true;
            }
            catch (Exception ex)
            {
                Trace.Warn(
                    "AiSearchFeedback",
                    "AI検索フィードバックの保存に失敗しました。",
                    ex);

                ShowMessage(
                    "フィードバックを保存できませんでした。");
            }
            finally
            {
                HelpfulButton.Enabled =
                    true;

                NotHelpfulButton.Enabled =
                    true;
            }
        }

        protected async void HelpfulButton_Click(
    object sender,
    EventArgs e)
        {
            await SaveFeedbackAsync(
                true);
        }

        protected async void NotHelpfulButton_Click(
            object sender,
            EventArgs e)
        {
            await SaveFeedbackAsync(
                false);
        }
    }
}