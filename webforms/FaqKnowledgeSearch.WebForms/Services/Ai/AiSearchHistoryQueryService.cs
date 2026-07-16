using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using FaqKnowledgeSearch.WebForms.Data;
using FaqKnowledgeSearch.WebForms.Dtos.Ai;

namespace FaqKnowledgeSearch.WebForms.Services.Ai
{
    public sealed class AiSearchHistoryQueryService
        : IAiSearchHistoryQueryService
    {
        private const int AnswerPreviewMaximumLength = 120;

        public IReadOnlyList<AiSearchHistoryListItemDto>
            SearchHistories(
                string keyword,
                bool? isSuccess)
        {
            using (var db = new FaqKnowledgeDbContext())
            {
                var query = db.AiSearchHistories
                    .AsNoTracking()
                    .Include(x => x.Sources)
                    .AsQueryable();

                var normalizedKeyword =
                    (keyword ?? string.Empty).Trim();

                if (!string.IsNullOrWhiteSpace(
                        normalizedKeyword))
                {
                    query = query.Where(x =>
                        x.Question.Contains(
                            normalizedKeyword) ||
                        x.SearchKeywords.Contains(
                            normalizedKeyword) ||
                        x.AiAnswer.Contains(
                            normalizedKeyword) ||
                        x.ErrorMessage.Contains(
                            normalizedKeyword));
                }

                if (isSuccess.HasValue)
                {
                    query = query.Where(x =>
                        x.IsSuccess ==
                        isSuccess.Value);
                }

                /*
                 * 先にDBから取得し、
                 * 回答プレビューなどの表示用加工は
                 * メモリ上で実施する。
                 */
                var histories = query
                    .OrderByDescending(x =>
                        x.ExecutedAt)
                    .ThenByDescending(x =>
                        x.Id)
                    .ToList();

                var historyIds =
                    histories
                        .Select(x => x.Id)
                        .ToList();

                /*
                 * 履歴ごとのフィードバックを
                 * 1回のクエリでまとめて取得する。
                 */
                var feedbackByHistoryId =
                    historyIds.Count == 0
                        ? new Dictionary<long, bool>()
                        : db.AiSearchFeedbacks
                            .AsNoTracking()
                            .Where(x =>
                                historyIds.Contains(
                                    x.AiSearchHistoryId))
                            .Select(x =>
                                new
                                {
                                    x.AiSearchHistoryId,
                                    x.IsHelpful
                                })
                            .ToDictionary(
                                x => x.AiSearchHistoryId,
                                x => x.IsHelpful);

                return histories
                    .Select(x =>
                        new AiSearchHistoryListItemDto
                        {
                            Id =
                                x.Id,

                            Question =
                                x.Question,

                            AnswerPreview =
                                CreateAnswerPreview(
                                    x.AiAnswer),

                            IsSuccess =
                                x.IsSuccess,

                            ErrorMessage =
                                x.ErrorMessage,

                            SourceCount =
                                x.Sources == null
                                    ? 0
                                    : x.Sources.Count,

                            IsHelpful =
                                feedbackByHistoryId.ContainsKey(
                                    x.Id)
                                    ? (bool?)feedbackByHistoryId[x.Id]
                                    : null,

                            ExecutedAt =
                                x.ExecutedAt
                        })
                    .ToList();
            }
        }

        public AiSearchHistoryDetailDto
            GetHistoryById(
                long id)
        {
            if (id <= 0)
            {
                return null;
            }

            using (var db = new FaqKnowledgeDbContext())
            {
                var history = db.AiSearchHistories
                    .AsNoTracking()
                    .Include(x => x.Sources)
                    .SingleOrDefault(x =>
                        x.Id == id);

                if (history == null)
                {
                    return null;
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

                return new AiSearchHistoryDetailDto
                {
                    Id =
                        history.Id,

                    Question =
                        history.Question,

                    SearchKeywords =
                        history.SearchKeywords,

                    AiAnswer =
                        history.AiAnswer,

                    IsSuccess =
                        history.IsSuccess,

                    ErrorMessage =
                        history.ErrorMessage,

                    IsHelpful =
                        isHelpful,

                    ExecutedAt =
                        history.ExecutedAt,

                    Sources = history.Sources == null
                        ? new List<AiSearchHistorySourceDto>()
                        : history.Sources
                            .OrderBy(x =>
                                x.DisplayOrder)
                            .ThenBy(x =>
                                x.Id)
                            .Select(x =>
                                new AiSearchHistorySourceDto
                                {
                                    FaqId =
                                        x.FaqId,

                                    FaqQuestion =
                                        x.FaqQuestion,

                                    FaqAnswer =
                                        x.FaqAnswer,

                                    CategoryName =
                                        x.CategoryName,

                                    DisplayOrder =
                                        x.DisplayOrder,

                                    Url =
                                        "~/Faqs/Detail.aspx?id=" +
                                        x.FaqId
                                })
                            .ToList()
                };
            }
        }

        private static string CreateAnswerPreview(
            string answer)
        {
            if (string.IsNullOrWhiteSpace(answer))
            {
                return null;
            }

            var normalizedAnswer =
                answer
                    .Replace("\r\n", " ")
                    .Replace("\n", " ")
                    .Replace("\r", " ")
                    .Trim();

            if (normalizedAnswer.Length <=
                AnswerPreviewMaximumLength)
            {
                return normalizedAnswer;
            }

            return normalizedAnswer.Substring(
                       0,
                       AnswerPreviewMaximumLength)
                   + "...";
        }
    }
}