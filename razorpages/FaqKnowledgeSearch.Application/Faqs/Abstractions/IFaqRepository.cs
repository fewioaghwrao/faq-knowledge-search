using FaqKnowledgeSearch.Domain.Faqs;

namespace FaqKnowledgeSearch.Application.Faqs.Abstractions;

public interface IFaqRepository
{
    Task<IReadOnlyList<Faq>> GetPublishedAsync(
        CancellationToken cancellationToken = default);

    Task<Faq?> GetPublishedByIdAsync(
        int faqId,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}