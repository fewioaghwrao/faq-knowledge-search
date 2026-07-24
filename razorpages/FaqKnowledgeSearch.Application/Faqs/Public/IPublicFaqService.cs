using FaqKnowledgeSearch.Application.Common.Models;

namespace FaqKnowledgeSearch.Application.Faqs.Public;

public interface IPublicFaqService
{
    Task<PagedResult<PublicFaqSearchItem>> SearchAsync(
        PublicFaqSearchCondition condition,
        CancellationToken cancellationToken = default);

    Task<PublicFaqDetail?> GetDetailAsync(
        int faqId,
        CancellationToken cancellationToken = default);
}