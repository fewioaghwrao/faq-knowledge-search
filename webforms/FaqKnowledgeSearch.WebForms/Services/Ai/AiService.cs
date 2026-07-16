using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using FaqKnowledgeSearch.WebForms.Dtos;
using FaqKnowledgeSearch.WebForms.Dtos.Ai;
using FaqKnowledgeSearch.WebForms.Settings;

namespace FaqKnowledgeSearch.WebForms.Services.Ai
{
    public sealed class AiService : IAiService
    {
        private readonly IFaqService _faqService;
        private readonly IAiApiClient _aiApiClient;
        private readonly IAiSearchHistoryService
            _historyService;
        private readonly AiSettings _settings;

        public AiService(
            IFaqService faqService,
            IAiApiClient aiApiClient,
            IAiSearchHistoryService historyService,
            AiSettings settings)
        {
            if (faqService == null)
            {
                throw new ArgumentNullException(
                    "faqService");
            }

            if (aiApiClient == null)
            {
                throw new ArgumentNullException(
                    "aiApiClient");
            }

            if (historyService == null)
            {
                throw new ArgumentNullException(
                    "historyService");
            }

            if (settings == null)
            {
                throw new ArgumentNullException(
                    "settings");
            }

            _faqService = faqService;
            _aiApiClient = aiApiClient;
            _historyService = historyService;
            _settings = settings;
        }

        public async Task<AiSearchResponse> SearchAsync(
            string question)
        {
            var normalizedQuestion =
                (question ?? string.Empty).Trim();

            /*
             * 入力値として不正な場合は、
             * AI検索を実行していないため履歴には保存しない。
             */
            if (string.IsNullOrWhiteSpace(
                    normalizedQuestion))
            {
                return CreateMessageResponse(
                    "質問文を入力してください。",
                    0);
            }

            if (normalizedQuestion.Length > 500)
            {
                return CreateMessageResponse(
                    "質問文は500文字以内で入力してください。",
                    0);
            }

            var maximumCount =
                _settings.MaxContextFaqCount <= 0
                    ? 5
                    : _settings.MaxContextFaqCount;

            /*
             * FaqServiceは同期処理なので、
             * ここではawaitを使用しない。
             */
            var faqs = _faqService
                .SearchPublishedFaqs(
                    normalizedQuestion)
                .Take(maximumCount)
                .ToList();

            /*
             * 関連FAQが存在しない場合も、
             * AI検索失敗履歴として保存する。
             */
            if (faqs.Count == 0)
            {
                var historyId =
                    await TrySaveFailureHistoryAsync(
                        normalizedQuestion,
                        "該当するFAQが見つかりませんでした。",
                        faqs);

                return CreateMessageResponse(
                    "該当するFAQが見つかりませんでした。" +
                    "キーワードを変えて検索してください。",
                    historyId);
            }

            var faqContext = string.Join(
                Environment.NewLine +
                Environment.NewLine,
                faqs.Select(
                    (faq, index) =>
                        BuildFaqContext(
                            faq,
                            index + 1)));

            var sources = faqs
                .Select(faq => new AiSourceDto
                {
                    FaqId = faq.Id,
                    Title = faq.Question,
                    Url =
                        "~/Faqs/Detail.aspx?id=" +
                        faq.Id
                })
                .ToList();

            string answer;

            try
            {
                answer =
                    await _aiApiClient
                        .GenerateAnswerAsync(
                            normalizedQuestion,
                            faqContext);
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "AI回答の生成に失敗しました。{0}",
                    ex);

                /*
                 * APIレスポンス全文や認証情報が
                 * DBへ保存されるのを避けるため、
                 * 履歴には固定メッセージを保存する。
                 */
                var historyId =
                    await TrySaveFailureHistoryAsync(
                        normalizedQuestion,
                        "AI回答の生成に失敗しました。",
                        faqs);

                return new AiSearchResponse
                {
                    Answer = null,
                    Disclaimer = null,
                    Sources = sources,
                    Message =
                        "AI回答の生成に失敗しました。" +
                        "参照元FAQをご確認ください。",
                    AiHistoryId = historyId
                };
            }

            /*
             * AI回答が生成できた場合は、
             * 回答と参照元FAQを成功履歴として保存する。
             */
            var successHistoryId =
                await TrySaveSuccessHistoryAsync(
                    normalizedQuestion,
                    answer,
                    faqs);

            return new AiSearchResponse
            {
                Answer = answer,
                Disclaimer =
                    "この回答はFAQをもとに生成されています。" +
                    "必ず参照元を確認してください。",
                Sources = sources,
                Message = null,
                AiHistoryId = successHistoryId
            };
        }

        /// <summary>
        /// AI検索成功履歴を保存します。
        /// 履歴保存に失敗してもAI回答自体は返します。
        /// </summary>
        private async Task<long>
            TrySaveSuccessHistoryAsync(
                string question,
                string answer,
                IReadOnlyList<FaqListItemDto> sources)
        {
            try
            {
                return await _historyService
                    .SaveSuccessAsync(
                        question,
                        answer,
                        sources);
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "AI検索成功履歴の保存に失敗しました。{0}",
                    ex);

                return 0;
            }
        }

        /// <summary>
        /// AI検索失敗履歴を保存します。
        /// 履歴保存に失敗しても検索結果の返却を継続します。
        /// </summary>
        private async Task<long>
            TrySaveFailureHistoryAsync(
                string question,
                string errorMessage,
                IReadOnlyList<FaqListItemDto> sources)
        {
            try
            {
                return await _historyService
                    .SaveFailureAsync(
                        question,
                        errorMessage,
                        sources);
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "AI検索失敗履歴の保存に失敗しました。{0}",
                    ex);

                return 0;
            }
        }

        private static AiSearchResponse
            CreateMessageResponse(
                string message,
                long historyId)
        {
            return new AiSearchResponse
            {
                Answer = null,
                Disclaimer = null,
                Sources =
                    new List<AiSourceDto>(),
                Message = message,
                AiHistoryId = historyId
            };
        }

        private static string BuildFaqContext(
            FaqListItemDto faq,
            int index)
        {
            var categoryName =
                string.IsNullOrWhiteSpace(
                    faq.CategoryName)
                    ? "未分類"
                    : faq.CategoryName.Trim();

            var faqQuestion =
                string.IsNullOrWhiteSpace(
                    faq.Question)
                    ? "質問が登録されていません。"
                    : faq.Question.Trim();

            var faqAnswer =
                string.IsNullOrWhiteSpace(
                    faq.Answer)
                    ? "回答が登録されていません。"
                    : faq.Answer.Trim();

            var tagNames =
                string.IsNullOrWhiteSpace(
                    faq.TagNames)
                    ? "なし"
                    : faq.TagNames.Trim();

            return string.Join(
                Environment.NewLine,
                new[]
                {
                    "[FAQ" + index + "]",
                    "ID: " + faq.Id,
                    "カテゴリ: " +
                        categoryName,
                    "タグ: " +
                        tagNames,
                    "質問:",
                    faqQuestion,
                    "回答:",
                    faqAnswer
                });
        }
    }
}