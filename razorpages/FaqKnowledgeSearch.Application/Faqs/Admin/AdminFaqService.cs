using FaqKnowledgeSearch.Domain.Faqs;

namespace FaqKnowledgeSearch.Application.Faqs.Admin;

public sealed class AdminFaqService(
    IAdminFaqRepository repository)
    : IAdminFaqService
{
    public async Task<AdminFaqFormOptions> GetFormOptionsAsync(
        CancellationToken cancellationToken = default)
    {
        var categories = await repository.GetCategoriesAsync(
            cancellationToken);

        var tags = await repository.GetTagsAsync(
            cancellationToken);

        return new AdminFaqFormOptions(
            categories
                .Select(category => new AdminFaqOption(
                    category.Id,
                    category.Name))
                .ToList(),
            tags
                .Select(tag => new AdminFaqOption(
                    tag.Id,
                    tag.Name))
                .ToList());
    }

    public async Task<AdminFaqEditData?> GetEditDataAsync(
        int faqId,
        CancellationToken cancellationToken = default)
    {
        var faq = await repository.GetByIdForUpdateAsync(
            faqId,
            cancellationToken);

        if (faq is null)
        {
            return null;
        }

        return new AdminFaqEditData(
            faq.Id,
            faq.Title,
            faq.Body,
            faq.CategoryId,
            faq.Tags
                .OrderBy(tag => tag.DisplayOrder)
                .ThenBy(tag => tag.Id)
                .Select(tag => tag.Id)
                .ToList(),
            faq.IsPublished);
    }

    public async Task<int> CreateAsync(
        AdminFaqCommand command,
        CancellationToken cancellationToken = default)
    {
        var tags = await ValidateAndLoadTagsAsync(
            command.CategoryId,
            command.TagIds,
            cancellationToken);

        var faq = new Faq(
            command.Title,
            command.Body,
            command.CategoryId,
            command.IsPublished);

        faq.ReplaceTags(tags);

        await repository.AddAsync(
            faq,
            cancellationToken);

        await repository.SaveChangesAsync(
            cancellationToken);

        return faq.Id;
    }

    public async Task<bool> UpdateAsync(
        int faqId,
        AdminFaqCommand command,
        CancellationToken cancellationToken = default)
    {
        var faq = await repository.GetByIdForUpdateAsync(
            faqId,
            cancellationToken);

        if (faq is null)
        {
            return false;
        }

        var tags = await ValidateAndLoadTagsAsync(
            command.CategoryId,
            command.TagIds,
            cancellationToken);

        faq.UpdateContent(
            command.Title,
            command.Body,
            command.CategoryId);

        faq.ReplaceTags(tags);

        if (command.IsPublished)
        {
            faq.Publish();
        }
        else
        {
            faq.Unpublish();
        }

        await repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private async Task<IReadOnlyList<Tag>> ValidateAndLoadTagsAsync(
        int categoryId,
        IReadOnlyCollection<int> tagIds,
        CancellationToken cancellationToken)
    {
        var categoryExists =
            await repository.CategoryExistsAsync(
                categoryId,
                cancellationToken);

        if (!categoryExists)
        {
            throw new ArgumentException(
                "選択されたカテゴリは存在しません。");
        }

        var normalizedTagIds = tagIds
            .Where(tagId => tagId > 0)
            .Distinct()
            .ToArray();

        if (normalizedTagIds.Length == 0)
        {
            return [];
        }

        var tags = await repository.GetTagsByIdsAsync(
            normalizedTagIds,
            cancellationToken);

        if (tags.Count != normalizedTagIds.Length)
        {
            var foundIds = tags
                .Select(tag => tag.Id)
                .ToHashSet();

            var missingIds = normalizedTagIds
                .Where(tagId => !foundIds.Contains(tagId));

            throw new ArgumentException(
                $"存在しないタグIDが指定されています: " +
                $"{string.Join(", ", missingIds)}");
        }

        return tags;
    }
    public async Task<bool> DeleteAsync(
    int faqId,
    CancellationToken cancellationToken = default)
    {
        var faq = await repository.GetByIdForUpdateAsync(
            faqId,
            cancellationToken);

        if (faq is null)
        {
            return false;
        }

        faq.Delete();

        await repository.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}