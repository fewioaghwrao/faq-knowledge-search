using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FaqKnowledgeSearch.WebForms.Data;
using FaqKnowledgeSearch.WebForms.Dtos;
using FaqKnowledgeSearch.WebForms.Models;

namespace FaqKnowledgeSearch.WebForms.Services.Ai
{
    public sealed class AiSearchHistoryService
        : IAiSearchHistoryService
    {
        public Task<long> SaveSuccessAsync(
            string question,
            string answer,
            IReadOnlyList<FaqListItemDto> sources)
        {
            return SaveAsync(
                question,
                answer,
                true,
                null,
                sources);
        }

        public Task<long> SaveFailureAsync(
            string question,
            string errorMessage,
            IReadOnlyList<FaqListItemDto> sources)
        {
            return SaveAsync(
                question,
                null,
                false,
                errorMessage,
                sources);
        }

        private static async Task<long> SaveAsync(
            string question,
            string answer,
            bool isSuccess,
            string errorMessage,
            IReadOnlyList<FaqListItemDto> sources)
        {
            var normalizedQuestion =
                (question ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(
                    normalizedQuestion))
            {
                throw new ArgumentException(
                    "履歴へ保存する質問文がありません。",
                    "question");
            }

            if (normalizedQuestion.Length > 500)
            {
                normalizedQuestion =
                    normalizedQuestion.Substring(
                        0,
                        500);
            }

            var normalizedAnswer =
                string.IsNullOrWhiteSpace(answer)
                    ? null
                    : answer.Trim();

            if (isSuccess &&
                string.IsNullOrWhiteSpace(
                    normalizedAnswer))
            {
                throw new ArgumentException(
                    "成功履歴にはAI回答が必要です。",
                    "answer");
            }

            var normalizedErrorMessage =
                NormalizeErrorMessage(
                    errorMessage,
                    isSuccess);

            var history =
                new AiSearchHistory
                {
                    Question =
                        normalizedQuestion,

                    /*
                     * 現段階では質問文をそのまま保存する。
                     * 将来、検索キーワード抽出を実装したら
                     * 別の値へ変更できる。
                     */
                    SearchKeywords =
                        normalizedQuestion,

                    AiAnswer =
                        isSuccess
                            ? normalizedAnswer
                            : null,

                    IsSuccess =
                        isSuccess,

                    ErrorMessage =
                        isSuccess
                            ? null
                            : normalizedErrorMessage,

                    /*
                     * Core版と同様にUTCで保存する。
                     * 管理画面では必要に応じて日本時間へ変換する。
                     */
                    ExecutedAt =
                        DateTime.UtcNow
                };

            var normalizedSources =
                NormalizeSources(sources);

            for (var index = 0;
                 index < normalizedSources.Count;
                 index++)
            {
                var source =
                    normalizedSources[index];

                history.Sources.Add(
                    new AiSearchHistorySource
                    {
                        FaqId =
                            source.Id,

                        FaqQuestion =
                            Truncate(
                                source.Question,
                                500,
                                "質問未設定"),

                        FaqAnswer =
                            string.IsNullOrWhiteSpace(
                                source.Answer)
                                ? null
                                : source.Answer.Trim(),

                        CategoryName =
                            Truncate(
                                source.CategoryName,
                                100,
                                null),

                        DisplayOrder =
                            index + 1
                    });
            }

            using (var db =
                new FaqKnowledgeDbContext())
            {
                db.AiSearchHistories.Add(
                    history);

                await db.SaveChangesAsync();

                return history.Id;
            }
        }

        /// <summary>
        /// 同じFAQが重複して渡された場合に、
        /// 外部キー・一意制約違反を防止します。
        /// </summary>
        private static IList<FaqListItemDto>
            NormalizeSources(
                IReadOnlyList<FaqListItemDto> sources)
        {
            if (sources == null)
            {
                return new List<FaqListItemDto>();
            }

            return sources
                .Where(x =>
                    x != null &&
                    x.Id > 0)
                .GroupBy(x => x.Id)
                .Select(x => x.First())
                .ToList();
        }

        private static string NormalizeErrorMessage(
            string errorMessage,
            bool isSuccess)
        {
            if (isSuccess)
            {
                return null;
            }

            var value =
                string.IsNullOrWhiteSpace(
                    errorMessage)
                    ? "AI検索に失敗しました。"
                    : errorMessage.Trim();

            return value.Length <= 1000
                ? value
                : value.Substring(0, 1000);
        }

        private static string Truncate(
            string value,
            int maximumLength,
            string defaultValue)
        {
            var normalized =
                string.IsNullOrWhiteSpace(value)
                    ? defaultValue
                    : value.Trim();

            if (string.IsNullOrEmpty(normalized))
            {
                return null;
            }

            return normalized.Length <= maximumLength
                ? normalized
                : normalized.Substring(
                    0,
                    maximumLength);
        }
    }
}