using FaqKnowledgeSearch.Domain.Faqs;

namespace FaqKnowledgeSearch.Application.Faqs.Admin;

public interface IAdminFaqRepository
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Tag>> GetTagsAsync(
        CancellationToken cancellationToken = default);

    Task<bool> CategoryExistsAsync(
        int categoryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Tag>> GetTagsByIdsAsync(
        IReadOnlyCollection<int> tagIds,
        CancellationToken cancellationToken = default);

    Task<Faq?> GetByIdForUpdateAsync(
        int faqId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Faq faq,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}