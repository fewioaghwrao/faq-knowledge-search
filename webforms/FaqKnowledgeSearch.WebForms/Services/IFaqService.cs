using System.Collections.Generic;
using FaqKnowledgeSearch.WebForms.Dtos;

namespace FaqKnowledgeSearch.WebForms.Services
{
    public interface IFaqService
    {
        IReadOnlyList<FaqListItemDto> SearchPublishedFaqs(
            string keyword);

        FaqDetailDto GetPublishedFaqById(long id);

        IReadOnlyList<FaqListItemDto> SearchFaqsForAdmin(
            string keyword,
            bool? isPublished);

        IReadOnlyList<CategoryOptionDto> GetCategoryOptions(
            long? includeCategoryId);

        /// <summary>
        /// FAQ編集画面に表示するタグ一覧を取得します。
        /// </summary>
        IReadOnlyList<TagOptionDto> GetTagOptions();

        FaqEditDto GetFaqForAdmin(long id);

        long CreateFaq(FaqEditDto input);

        bool UpdateFaq(
            long id,
            FaqEditDto input);

        bool DeleteFaq(long id);
    }
}