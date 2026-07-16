using System;
using System.Data.Entity;
using System.Threading.Tasks;
using FaqKnowledgeSearch.WebForms.Data;
using FaqKnowledgeSearch.WebForms.Models;

namespace FaqKnowledgeSearch.WebForms.Services.Ai
{
    public sealed class AiSearchFeedbackService
        : IAiSearchFeedbackService
    {
        public async Task SaveAsync(
            long aiSearchHistoryId,
            bool isHelpful,
            string comment)
        {
            if (aiSearchHistoryId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    "aiSearchHistoryId",
                    "AI検索履歴IDが正しくありません。");
            }

            var normalizedComment =
                NormalizeComment(comment);

            using (var db =
                new FaqKnowledgeDbContext())
            {
                var historyExists =
                    await db.AiSearchHistories
                        .AsNoTracking()
                        .AnyAsync(x =>
                            x.Id == aiSearchHistoryId);

                if (!historyExists)
                {
                    throw new InvalidOperationException(
                        "評価対象のAI検索履歴が存在しません。");
                }

                var feedback =
                    await db.AiSearchFeedbacks
                        .SingleOrDefaultAsync(x =>
                            x.AiSearchHistoryId ==
                            aiSearchHistoryId);

                var now =
                    DateTime.UtcNow;

                if (feedback == null)
                {
                    feedback =
                        new AiSearchFeedback
                        {
                            AiSearchHistoryId =
                                aiSearchHistoryId,

                            IsHelpful =
                                isHelpful,

                            Comment =
                                normalizedComment,

                            CreatedAt =
                                now,

                            UpdatedAt =
                                now
                        };

                    db.AiSearchFeedbacks.Add(
                        feedback);
                }
                else
                {
                    /*
                     * すでに評価されている場合は、
                     * 新規追加せず既存評価を更新する。
                     */
                    feedback.IsHelpful =
                        isHelpful;

                    feedback.Comment =
                        normalizedComment;

                    feedback.UpdatedAt =
                        now;
                }

                await db.SaveChangesAsync();
            }
        }

        private static string NormalizeComment(
            string comment)
        {
            if (string.IsNullOrWhiteSpace(
                    comment))
            {
                return null;
            }

            var normalized =
                comment.Trim();

            return normalized.Length <= 1000
                ? normalized
                : normalized.Substring(
                    0,
                    1000);
        }
    }
}