using FaqApp.Api.Data;
using FaqApp.Api.Dtos.Faqs;
using FaqApp.Api.Entities;
using FaqApp.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace FaqApp.Api.Services;

public class FaqService : IFaqService
{
    private readonly AppDbContext _dbContext;

    public FaqService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<FaqListItemDto>> SearchAsync(FaqSearchQuery query)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);
        var keywords = SplitKeywords(query.Keyword);

        var faqs = _dbContext.Faqs
            .Include(x => x.Category)
            .Include(x => x.Tags)
            .Where(x => x.DeletedAt == null)
            .AsQueryable();

        if (!query.IncludeUnpublished)
        {
            faqs = faqs.Where(x => x.IsPublished);
        }

        if (keywords.Length > 0)
        {
            foreach (var keyword in keywords)
            {
                var keywordLocal = keyword;

                faqs = faqs.Where(x =>
                    x.Title.Contains(keywordLocal) ||
                    x.Body.Contains(keywordLocal) ||
                    x.Category.Name.Contains(keywordLocal) ||
                    x.Tags.Any(t => t.Name.Contains(keywordLocal)));
            }
        }

        if (query.CategoryId.HasValue)
        {
            faqs = faqs.Where(x => x.CategoryId == query.CategoryId.Value);
        }

        if (query.TagId.HasValue)
        {
            faqs = faqs.Where(x => x.Tags.Any(t => t.Id == query.TagId.Value));
        }

        var faqList = await faqs.ToListAsync();

        var items = faqList
            .Select(x => new FaqListItemDto
            {
                Id = x.Id,
                Title = x.Title,
                Body = x.Body,
                TitleHighlighted = query.Highlight
                    ? ApplyHighlight(x.Title, keywords)
                    : null,
                BodyExcerpt = CreateBodyExcerpt(x.Body, keywords),
                Score = CalculateScore(x, keywords),
                CategoryName = x.Category.Name,
                Tags = x.Tags.OrderBy(t => t.DisplayOrder).Select(t => t.Name).ToList(),
                IsPublished = x.IsPublished,
                ViewCount = x.ViewCount,
                UpdatedAt = x.UpdatedAt
            })
            .ToList();

        items = query.Sort == "score" && keywords.Length > 0
            ? items
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.UpdatedAt)
                .ToList()
            : items
                .OrderByDescending(x => x.UpdatedAt)
                .ToList();

        return items
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public async Task<FaqListItemDto?> GetByIdAsync(int id)
    {
        var faq = await _dbContext.Faqs
            .Include(x => x.Category)
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

        if (faq is null)
        {
            return null;
        }

        faq.IncrementViewCount();
        await _dbContext.SaveChangesAsync();

        return new FaqListItemDto
        {
            Id = faq.Id,
            Title = faq.Title,
            Body = faq.Body,
            CategoryName = faq.Category.Name,
            Tags = faq.Tags.OrderBy(t => t.DisplayOrder).Select(t => t.Name).ToList(),
            IsPublished = faq.IsPublished,
            ViewCount = faq.ViewCount,
            UpdatedAt = faq.UpdatedAt
        };
    }

    public async Task<FaqListItemDto> CreateAsync(FaqCreateRequest request)
    {
        var categoryExists = await _dbContext.Categories
            .AnyAsync(x => x.Id == request.CategoryId);

        if (!categoryExists)
        {
            throw new InvalidOperationException("指定されたカテゴリが存在しません。");
        }

        var tags = await _dbContext.Tags
            .Where(x => request.TagIds.Contains(x.Id))
            .ToListAsync();

        var faq = new Faq(
            request.Title,
            request.Body,
            request.CategoryId,
            request.IsPublished);

        foreach (var tag in tags)
        {
            faq.Tags.Add(tag);
        }

        _dbContext.Faqs.Add(faq);
        await _dbContext.SaveChangesAsync();

        var created = await GetByIdAsync(faq.Id);

        return created!;
    }

    public async Task<FaqListItemDto?> UpdateAsync(int id, FaqUpdateRequest request)
    {
        var faq = await _dbContext.Faqs
            .Include(x => x.Tags)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

        if (faq is null)
        {
            return null;
        }

        var categoryExists = await _dbContext.Categories
            .AnyAsync(x => x.Id == request.CategoryId);

        if (!categoryExists)
        {
            throw new InvalidOperationException("指定されたカテゴリが存在しません。");
        }

        var tags = await _dbContext.Tags
            .Where(x => request.TagIds.Contains(x.Id))
            .ToListAsync();

        faq.Update(
            request.Title,
            request.Body,
            request.CategoryId,
            request.IsPublished);

        faq.Tags.Clear();

        foreach (var tag in tags)
        {
            faq.Tags.Add(tag);
        }

        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(faq.Id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var faq = await _dbContext.Faqs
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);

        if (faq is null)
        {
            return false;
        }

        faq.Delete();

        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static string[] SplitKeywords(string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return [];
        }

        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "場合",
        "どう",
        "すれば",
        "いい",
        "ですか",
        "ください",
        "について",
        "方法",
        "教えて",
        "とは"
    };

        return keyword
            .Split(
                [' ', '　', '、', '。', ',', '.', '？', '?', '！', '!', '・', '/', '\\', '(', ')', '（', '）'],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(x => x.Length >= 2)
            .Where(x => !stopWords.Contains(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static double CalculateScore(Faq faq, string[] keywords)
    {
        if (keywords.Length == 0)
        {
            return 0;
        }

        double score = 0;

        foreach (var keyword in keywords)
        {
            if (faq.Title.Equals(keyword, StringComparison.OrdinalIgnoreCase))
            {
                score += 10;
            }
            else if (faq.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                score += 7;
            }

            if (faq.Tags.Any(t => t.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                score += 5;
            }

            if (faq.Category.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                score += 4;
            }

            if (faq.Body.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                score += 3;
            }
        }

        return score;
    }

    private static string ApplyHighlight(string text, string[] keywords)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var encoded = WebUtility.HtmlEncode(text);

        if (keywords.Length == 0)
        {
            return encoded;
        }

        foreach (var keyword in keywords)
        {
            var encodedKeyword = WebUtility.HtmlEncode(keyword);

            encoded = encoded.Replace(
                encodedKeyword,
                $"<mark>{encodedKeyword}</mark>",
                StringComparison.OrdinalIgnoreCase);
        }

        return encoded;
    }

    private static string CreateBodyExcerpt(string body, string[] keywords)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        if (keywords.Length == 0)
        {
            return body.Length <= 120
                ? body
                : body[..120] + "...";
        }

        var firstHitIndex = keywords
            .Select(keyword => body.IndexOf(keyword, StringComparison.OrdinalIgnoreCase))
            .Where(index => index >= 0)
            .DefaultIfEmpty(0)
            .Min();

        var start = Math.Max(0, firstHitIndex - 40);
        var length = Math.Min(120, body.Length - start);

        var excerpt = body.Substring(start, length);

        if (start > 0)
        {
            excerpt = "..." + excerpt;
        }

        if (start + length < body.Length)
        {
            excerpt += "...";
        }

        return excerpt;
    }
}