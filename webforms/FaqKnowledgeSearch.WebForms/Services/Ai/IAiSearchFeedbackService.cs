using System.Threading.Tasks;

namespace FaqKnowledgeSearch.WebForms.Services.Ai
{
    public interface IAiSearchFeedbackService
    {
        /// <summary>
        /// AI検索履歴に対する評価を保存します。
        /// すでに評価が存在する場合は更新します。
        /// </summary>
        Task SaveAsync(
            long aiSearchHistoryId,
            bool isHelpful,
            string comment);
    }
}