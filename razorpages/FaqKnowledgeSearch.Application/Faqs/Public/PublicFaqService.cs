using FaqKnowledgeSearch.Application.Common.Models;
using FaqKnowledgeSearch.Application.Faqs.Abstractions;
using FaqKnowledgeSearch.Domain.Faqs;

namespace FaqKnowledgeSearch.Application.Faqs.Public;

public sealed class PublicFaqService(
    IFaqRepository faqRepository)
    : IPublicFaqService
{
    private const int MaximumPageSize = 50;
    private const int ExcerptLength = 120;

    public async Task<PagedResult<PublicFaqSearchItem>> SearchAsync(
        PublicFaqSearchCondition condition,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(condition);

        var page = Math.Max(condition.Page, 1);

        var pageSize = Math.Clamp(
            condition.PageSize,
            1,
            MaximumPageSize);

        var keywords = SplitKeywords(condition.Keyword);

        var faqs = await faqRepository.GetPublishedAsync(
            cancellationToken);

        IEnumerable<Faq> filteredFaqs = faqs;

        if (condition.CategoryId.HasValue)
        {
            filteredFaqs = filteredFaqs.Where(faq =>
                faq.CategoryId ==
                condition.CategoryId.Value);
        }

        if (condition.TagId.HasValue)
        {
            filteredFaqs = filteredFaqs.Where(faq =>
                faq.Tags.Any(tag =>
                    tag.Id == condition.TagId.Value));
        }

        if (keywords.Length > 0)
        {
            filteredFaqs = filteredFaqs.Where(faq =>
                keywords.All(keyword =>
                    ContainsKeyword(faq, keyword)));
        }

        var candidates = filteredFaqs
            .Select(faq => new SearchCandidate(
                faq,
                CalculateRelevanceScore(
                    faq,
                    keywords)));

        candidates = SortCandidates(
            candidates,
            condition.SortOrder,
            hasKeyword: keywords.Length > 0);

        var totalCount = candidates.Count();

        var items = candidates
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(candidate =>
                ToSearchItem(
                    candidate.Faq,
                    candidate.RelevanceScore,
                    keywords))
            .ToArray();

        return new PagedResult<PublicFaqSearchItem>(
            Items: items,
            TotalCount: totalCount,
            Page: page,
            PageSize: pageSize);
    }

    public async Task<PublicFaqDetail?> GetDetailAsync(
        int faqId,
        CancellationToken cancellationToken = default)
    {
        if (faqId <= 0)
        {
            return null;
        }

        var faq = await faqRepository
            .GetPublishedByIdAsync(
                faqId,
                cancellationToken);

        if (faq is null)
        {
            return null;
        }

        faq.IncrementViewCount();

        await faqRepository.SaveChangesAsync(
            cancellationToken);

        return new PublicFaqDetail(
            Id: faq.Id,
            Title: faq.Title,
            Body: faq.Body,
            CategoryId: faq.CategoryId,
            CategoryName: faq.Category.Name,
            TagNames: GetTagNames(faq),
            ViewCount: faq.ViewCount,
            CreatedAt: faq.CreatedAt,
            UpdatedAt: faq.UpdatedAt);
    }

    private static string[] SplitKeywords(
        string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return [];
        }

        return keyword
            .Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool ContainsKeyword(
        Faq faq,
        string keyword)
    {
        return Contains(
                   faq.Title,
                   keyword) ||
               Contains(
                   faq.Body,
                   keyword) ||
               Contains(
                   faq.Category.Name,
                   keyword) ||
               faq.Tags.Any(tag =>
                   Contains(tag.Name, keyword));
    }

    private static bool Contains(
        string source,
        string keyword)
    {
        return source.Contains(
            keyword,
            StringComparison.OrdinalIgnoreCase);
    }

    private static int CalculateRelevanceScore(
        Faq faq,
        IReadOnlyCollection<string> keywords)
    {
        if (keywords.Count == 0)
        {
            return 0;
        }

        var score = 0;

        foreach (var keyword in keywords)
        {
            if (string.Equals(
                    faq.Title,
                    keyword,
                    StringComparison.OrdinalIgnoreCase))
            {
                score += 100;
            }
            else if (Contains(faq.Title, keyword))
            {
                score += 40;
            }

            if (faq.Tags.Any(tag =>
                    string.Equals(
                        tag.Name,
                        keyword,
                        StringComparison.OrdinalIgnoreCase)))
            {
                score += 30;
            }
            else if (faq.Tags.Any(tag =>
                         Contains(tag.Name, keyword)))
            {
                score += 20;
            }

            if (Contains(
                    faq.Category.Name,
                    keyword))
            {
                score += 15;
            }

            if (Contains(
                    faq.Body,
                    keyword))
            {
                score += 10;
            }
        }

        return score;
    }

    private static IEnumerable<SearchCandidate>
        SortCandidates(
            IEnumerable<SearchCandidate> candidates,
            FaqSortOrder sortOrder,
            bool hasKeyword)
    {
        return sortOrder switch
        {
            FaqSortOrder.Newest =>
                candidates
                    .OrderByDescending(x =>
                        x.Faq.UpdatedAt)
                    .ThenByDescending(x =>
                        x.Faq.Id),

            FaqSortOrder.MostViewed =>
                candidates
                    .OrderByDescending(x =>
                        x.Faq.ViewCount)
                    .ThenByDescending(x =>
                        x.Faq.UpdatedAt)
                    .ThenByDescending(x =>
                        x.Faq.Id),

            FaqSortOrder.Relevance
                when hasKeyword =>
                candidates
                    .OrderByDescending(x =>
                        x.RelevanceScore)
                    .ThenByDescending(x =>
                        x.Faq.ViewCount)
                    .ThenByDescending(x =>
                        x.Faq.UpdatedAt)
                    .ThenByDescending(x =>
                        x.Faq.Id),

            _ =>
                candidates
                    .OrderByDescending(x =>
                        x.Faq.UpdatedAt)
                    .ThenByDescending(x =>
                        x.Faq.Id)
        };
    }

    private static PublicFaqSearchItem ToSearchItem(
        Faq faq,
        int relevanceScore,
        IReadOnlyList<string> keywords)
    {
        return new PublicFaqSearchItem(
            Id: faq.Id,
            Title: faq.Title,
            BodyExcerpt: CreateExcerpt(
                faq.Body,
                keywords),
            CategoryId: faq.CategoryId,
            CategoryName: faq.Category.Name,
            TagNames: GetTagNames(faq),
            ViewCount: faq.ViewCount,
            UpdatedAt: faq.UpdatedAt,
            RelevanceScore: relevanceScore);
    }

    private static string CreateExcerpt(
        string body,
        IReadOnlyList<string> keywords)
    {
        if (body.Length <= ExcerptLength)
        {
            return body;
        }

        var firstMatchIndex = keywords
            .Select(keyword =>
                body.IndexOf(
                    keyword,
                    StringComparison.OrdinalIgnoreCase))
            .Where(index => index >= 0)
            .DefaultIfEmpty(0)
            .Min();

        var startIndex = Math.Max(
            firstMatchIndex - 30,
            0);

        if (startIndex + ExcerptLength >
            body.Length)
        {
            startIndex = Math.Max(
                body.Length - ExcerptLength,
                0);
        }

        var excerpt = body.Substring(
            startIndex,
            Math.Min(
                ExcerptLength,
                body.Length - startIndex));

        if (startIndex > 0)
        {
            excerpt = $"…{excerpt}";
        }

        if (startIndex + ExcerptLength <
            body.Length)
        {
            excerpt = $"{excerpt}…";
        }

        return excerpt;
    }

    private static IReadOnlyList<string> GetTagNames(
        Faq faq)
    {
        return faq.Tags
            .OrderBy(tag => tag.DisplayOrder)
            .ThenBy(tag => tag.Id)
            .Select(tag => tag.Name)
            .ToArray();
    }

    private sealed record SearchCandidate(
        Faq Faq,
        int RelevanceScore);
}