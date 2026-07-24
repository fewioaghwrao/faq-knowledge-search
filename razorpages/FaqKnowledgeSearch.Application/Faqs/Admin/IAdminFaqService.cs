namespace FaqKnowledgeSearch.Application.Faqs.Admin;

public interface IAdminFaqService
{
    Task<AdminFaqFormOptions> GetFormOptionsAsync(
        CancellationToken cancellationToken = default);

    Task<AdminFaqEditData?> GetEditDataAsync(
        int faqId,
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        AdminFaqCommand command,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(
        int faqId,
        AdminFaqCommand command,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
    int faqId,
    CancellationToken cancellationToken = default);
}
