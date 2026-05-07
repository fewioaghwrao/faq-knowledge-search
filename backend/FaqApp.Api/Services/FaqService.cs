using FaqApp.Api.Data;
using FaqApp.Api.Dtos.Faqs;
using FaqApp.Api.Entities;
using FaqApp.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

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

        var faqs = _dbContext.Faqs
            .Include(x => x.Category)
            .Include(x => x.Tags)
            .Where(x => x.DeletedAt == null)
            .AsQueryable();

        if (!query.IncludeUnpublished)
        {
            faqs = faqs.Where(x => x.IsPublished);
        }

        if (!string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keyword = query.Keyword.Trim();

            faqs = faqs.Where(x =>
                x.Title.Contains(keyword) ||
                x.Body.Contains(keyword) ||
                x.Category.Name.Contains(keyword) ||
                x.Tags.Any(t => t.Name.Contains(keyword)));
        }

        if (query.CategoryId.HasValue)
        {
            faqs = faqs.Where(x => x.CategoryId == query.CategoryId.Value);
        }

        if (query.TagId.HasValue)
        {
            faqs = faqs.Where(x => x.Tags.Any(t => t.Id == query.TagId.Value));
        }

        return await faqs
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new FaqListItemDto
            {
                Id = x.Id,
                Title = x.Title,
                Body = x.Body,
                CategoryName = x.Category.Name,
                Tags = x.Tags.OrderBy(t => t.DisplayOrder).Select(t => t.Name).ToList(),
                IsPublished = x.IsPublished,
                ViewCount = x.ViewCount,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
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
}