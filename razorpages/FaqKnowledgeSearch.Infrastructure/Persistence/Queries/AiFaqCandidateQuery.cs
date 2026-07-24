using System.Text;
using System.Text.RegularExpressions;
using FaqKnowledgeSearch.Application.AiSearch;
using Microsoft.EntityFrameworkCore;

namespace FaqKnowledgeSearch.Infrastructure.Persistence.Queries;

public sealed class AiFaqCandidateQuery(
    AppDbContext dbContext)
    : IAiFaqCandidateQuery
{
    public async Task<IReadOnlyList<AiFaqReference>> SearchAsync(
        string question,
        int maxResults,
        CancellationToken cancellationToken = default)
    {
        question = question.Trim();
        maxResults = Math.Clamp(maxResults, 1, 10);

        var faqs = await dbContext.Faqs
            .AsNoTracking()
            .Include(faq => faq.Category)
            .Include(faq => faq.Tags)
            .Where(faq => faq.IsPublished)
            .ToListAsync(cancellationToken);

        var normalizedQuestion = Normalize(question);
        var terms = ExtractTerms(normalizedQuestion);

        return faqs
            .Select(faq =>
            {
                var score = CalculateScore(
                    normalizedQuestion,
                    terms,
                    faq.Title,
                    faq.Body,
                    faq.Category.Name,
                    faq.Tags.Select(tag => tag.Name));

                return new
                {
                    Faq = faq,
                    Score = score
                };
            })
            .Where(item => item.Score > 0)
            .OrderByDescending(item => item.Score)
            .ThenByDescending(item => item.Faq.ViewCount)
            .ThenByDescending(item => item.Faq.UpdatedAt)
            .Take(maxResults)
            .Select(item =>
                new AiFaqReference(
                    item.Faq.Id,
                    item.Faq.Title,
                    item.Faq.Body,
                    item.Faq.Category.Name,
                    item.Faq.Tags
                        .OrderBy(tag => tag.DisplayOrder)
                        .ThenBy(tag => tag.Id)
                        .Select(tag => tag.Name)
                        .ToList(),
                    item.Score))
            .ToList();
    }

    private static int CalculateScore(
        string normalizedQuestion,
        IReadOnlyCollection<string> terms,
        string title,
        string body,
        string categoryName,
        IEnumerable<string> tagNames)
    {
        var normalizedTitle = Normalize(title);
        var normalizedBody = Normalize(body);
        var normalizedCategory = Normalize(categoryName);

        var normalizedTags = tagNames
            .Select(Normalize)
            .ToArray();

        var score = 0;

        if (normalizedQuestion.Length >= 2)
        {
            if (normalizedTitle.Contains(
                    normalizedQuestion,
                    StringComparison.Ordinal))
            {
                score += 50;
            }

            if (normalizedBody.Contains(
                    normalizedQuestion,
                    StringComparison.Ordinal))
            {
                score += 20;
            }
        }

        foreach (var term in terms)
        {
            if (normalizedTitle.Contains(
                    term,
                    StringComparison.Ordinal))
            {
                score += 12;
            }

            if (normalizedBody.Contains(
                    term,
                    StringComparison.Ordinal))
            {
                score += 4;
            }

            if (normalizedCategory.Contains(
                    term,
                    StringComparison.Ordinal))
            {
                score += 6;
            }

            if (normalizedTags.Any(
                    tag => tag.Contains(
                        term,
                        StringComparison.Ordinal)))
            {
                score += 8;
            }
        }

        return score;
    }

    private static IReadOnlyCollection<string> ExtractTerms(
        string normalizedQuestion)
    {
        return Regex.Split(
                normalizedQuestion,
                @"[\s　,，、。．・/\\:：;；\-_]+")
            .Where(term => !string.IsNullOrWhiteSpace(term))
            .Where(term => term.Length >= 2)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    private static string Normalize(string value)
    {
        return value
            .Normalize(NormalizationForm.FormKC)
            .Trim()
            .ToUpperInvariant();
    }
}