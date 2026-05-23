using FaqApp.Api.Data;
using FaqApp.Api.Dtos.Faqs;
using FaqApp.Api.Entities;
using FaqApp.Api.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FaqApp.Api.Tests.Services;

public class FaqServiceTests
{
    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static async Task<SeedData> SeedMasterDataAsync(AppDbContext dbContext)
    {
        var loginCategory = new Category("ログイン障害", 1);
        var csvCategory = new Category("CSV取込", 2);

        var loginTag = new Tag("ログイン", 1);
        var csvTag = new Tag("CSV", 2);

        dbContext.Categories.AddRange(loginCategory, csvCategory);
        dbContext.Tags.AddRange(loginTag, csvTag);

        await dbContext.SaveChangesAsync();

        return new SeedData(
            loginCategory,
            csvCategory,
            loginTag,
            csvTag);
    }

    [Fact]
    public async Task SearchAsync_ReturnsPublishedFaqsOnly()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var seed = await SeedMasterDataAsync(dbContext);

        dbContext.Faqs.AddRange(
            new Faq(
                "ログインできない場合",
                "パスワード再設定を確認してください。",
                seed.LoginCategory.Id,
                isPublished: true),
            new Faq(
                "管理者向け非公開FAQ",
                "非公開の手順です。",
                seed.LoginCategory.Id,
                isPublished: false));

        await dbContext.SaveChangesAsync();

        var service = new FaqService(dbContext);

        var query = new FaqSearchQuery
        {
            Page = 1,
            PageSize = 20,
            IncludeUnpublished = false
        };

        // Act
        var result = await service.SearchAsync(query);

        // Assert
        Assert.Single(result);
        Assert.Equal("ログインできない場合", result[0].Title);
        Assert.True(result[0].IsPublished);
    }

    [Fact]
    public async Task SearchAsync_IncludeUnpublished_ReturnsPublishedAndUnpublishedFaqs()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var seed = await SeedMasterDataAsync(dbContext);

        dbContext.Faqs.AddRange(
            new Faq(
                "公開FAQ",
                "公開本文",
                seed.LoginCategory.Id,
                isPublished: true),
            new Faq(
                "非公開FAQ",
                "非公開本文",
                seed.LoginCategory.Id,
                isPublished: false));

        await dbContext.SaveChangesAsync();

        var service = new FaqService(dbContext);

        var query = new FaqSearchQuery
        {
            Page = 1,
            PageSize = 20,
            IncludeUnpublished = true
        };

        // Act
        var result = await service.SearchAsync(query);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, x => x.Title == "公開FAQ");
        Assert.Contains(result, x => x.Title == "非公開FAQ");
    }

    [Fact]
    public async Task SearchAsync_WithKeyword_ReturnsMatchedFaq()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var seed = await SeedMasterDataAsync(dbContext);

        dbContext.Faqs.AddRange(
            new Faq(
                "ログインできない場合",
                "パスワード再設定を確認してください。",
                seed.LoginCategory.Id,
                isPublished: true),
            new Faq(
                "CSV取込エラー",
                "文字コードを確認してください。",
                seed.CsvCategory.Id,
                isPublished: true));

        await dbContext.SaveChangesAsync();

        var service = new FaqService(dbContext);

        var query = new FaqSearchQuery
        {
            Keyword = "CSV",
            Page = 1,
            PageSize = 20,
            IncludeUnpublished = false
        };

        // Act
        var result = await service.SearchAsync(query);

        // Assert
        Assert.Single(result);
        Assert.Equal("CSV取込エラー", result[0].Title);
        Assert.Contains("文字コード", result[0].BodyExcerpt);
    }

    [Fact]
    public async Task SearchAsync_WithCategoryId_ReturnsMatchedFaqs()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var seed = await SeedMasterDataAsync(dbContext);

        dbContext.Faqs.AddRange(
            new Faq(
                "ログインできない場合",
                "パスワード再設定を確認してください。",
                seed.LoginCategory.Id,
                isPublished: true),
            new Faq(
                "CSV取込エラー",
                "文字コードを確認してください。",
                seed.CsvCategory.Id,
                isPublished: true));

        await dbContext.SaveChangesAsync();

        var service = new FaqService(dbContext);

        var query = new FaqSearchQuery
        {
            CategoryId = seed.CsvCategory.Id,
            Page = 1,
            PageSize = 20,
            IncludeUnpublished = false
        };

        // Act
        var result = await service.SearchAsync(query);

        // Assert
        Assert.Single(result);
        Assert.Equal("CSV取込エラー", result[0].Title);
        Assert.Equal("CSV取込", result[0].CategoryName);
    }

    [Fact]
    public async Task SearchAsync_WithTagId_ReturnsMatchedFaqs()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var seed = await SeedMasterDataAsync(dbContext);

        var loginFaq = new Faq(
            "ログインできない場合",
            "パスワード再設定を確認してください。",
            seed.LoginCategory.Id,
            isPublished: true);

        loginFaq.Tags.Add(seed.LoginTag);

        var csvFaq = new Faq(
            "CSV取込エラー",
            "文字コードを確認してください。",
            seed.CsvCategory.Id,
            isPublished: true);

        csvFaq.Tags.Add(seed.CsvTag);

        dbContext.Faqs.AddRange(loginFaq, csvFaq);

        await dbContext.SaveChangesAsync();

        var service = new FaqService(dbContext);

        var query = new FaqSearchQuery
        {
            TagId = seed.CsvTag.Id,
            Page = 1,
            PageSize = 20,
            IncludeUnpublished = false
        };

        // Act
        var result = await service.SearchAsync(query);

        // Assert
        Assert.Single(result);
        Assert.Equal("CSV取込エラー", result[0].Title);
        Assert.Contains("CSV", result[0].Tags);
    }

    [Fact]
    public async Task SearchAsync_WithHighlight_ReturnsHighlightedTitle()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var seed = await SeedMasterDataAsync(dbContext);

        dbContext.Faqs.Add(new Faq(
            "ログインできない場合",
            "パスワード再設定を確認してください。",
            seed.LoginCategory.Id,
            isPublished: true));

        await dbContext.SaveChangesAsync();

        var service = new FaqService(dbContext);

        var query = new FaqSearchQuery
        {
            Keyword = "ログイン",
            Highlight = true,
            Page = 1,
            PageSize = 20,
            IncludeUnpublished = false
        };

        // Act
        var result = await service.SearchAsync(query);

        // Assert
        Assert.Single(result);
        Assert.Equal("<mark>ログイン</mark>できない場合", result[0].TitleHighlighted);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingFaq_ReturnsFaqAndIncrementsViewCount()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var seed = await SeedMasterDataAsync(dbContext);

        var faq = new Faq(
            "ログインできない場合",
            "パスワード再設定を確認してください。",
            seed.LoginCategory.Id,
            isPublished: true);

        faq.Tags.Add(seed.LoginTag);

        dbContext.Faqs.Add(faq);
        await dbContext.SaveChangesAsync();

        var service = new FaqService(dbContext);

        // Act
        var result = await service.GetByIdAsync(faq.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(faq.Id, result!.Id);
        Assert.Equal("ログインできない場合", result.Title);
        Assert.Equal("ログイン障害", result.CategoryName);
        Assert.Contains("ログイン", result.Tags);
        Assert.Equal(1, result.ViewCount);

        var savedFaq = await dbContext.Faqs.FirstAsync(x => x.Id == faq.Id);
        Assert.Equal(1, savedFaq.ViewCount);
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        await SeedMasterDataAsync(dbContext);

        var service = new FaqService(dbContext);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_CreatesFaq()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var seed = await SeedMasterDataAsync(dbContext);

        var service = new FaqService(dbContext);

        var request = new FaqCreateRequest
        {
            Title = "PDF出力できない場合",
            Body = "対象データと権限を確認してください。",
            CategoryId = seed.LoginCategory.Id,
            TagIds = new List<int> { seed.LoginTag.Id },
            IsPublished = true
        };

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        Assert.True(result.Id > 0);
        Assert.Equal("PDF出力できない場合", result.Title);
        Assert.Equal("ログイン障害", result.CategoryName);
        Assert.Contains("ログイン", result.Tags);

        var savedFaq = await dbContext.Faqs
            .Include(x => x.Tags)
            .FirstAsync(x => x.Id == result.Id);

        Assert.Equal(request.Title, savedFaq.Title);
        Assert.Equal(request.Body, savedFaq.Body);
        Assert.Equal(seed.LoginCategory.Id, savedFaq.CategoryId);
        Assert.True(savedFaq.IsPublished);
        Assert.Single(savedFaq.Tags);
    }

    [Fact]
    public async Task CreateAsync_CategoryNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var seed = await SeedMasterDataAsync(dbContext);

        var service = new FaqService(dbContext);

        var request = new FaqCreateRequest
        {
            Title = "カテゴリ不正FAQ",
            Body = "本文",
            CategoryId = 999,
            TagIds = new List<int> { seed.LoginTag.Id },
            IsPublished = true
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(request));

        Assert.Equal("指定されたカテゴリが存在しません。", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_ExistingFaq_UpdatesFaq()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var seed = await SeedMasterDataAsync(dbContext);

        var faq = new Faq(
            "ログインできない場合",
            "パスワード再設定を確認してください。",
            seed.LoginCategory.Id,
            isPublished: true);

        faq.Tags.Add(seed.LoginTag);

        dbContext.Faqs.Add(faq);
        await dbContext.SaveChangesAsync();

        var service = new FaqService(dbContext);

        var request = new FaqUpdateRequest
        {
            Title = "CSV取込エラー",
            Body = "文字コードを確認してください。",
            CategoryId = seed.CsvCategory.Id,
            TagIds = new List<int> { seed.CsvTag.Id },
            IsPublished = false
        };

        // Act
        var result = await service.UpdateAsync(faq.Id, request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(faq.Id, result!.Id);
        Assert.Equal("CSV取込エラー", result.Title);
        Assert.Equal("文字コードを確認してください。", result.Body);
        Assert.Equal("CSV取込", result.CategoryName);
        Assert.Contains("CSV", result.Tags);
        Assert.False(result.IsPublished);

        var savedFaq = await dbContext.Faqs
            .Include(x => x.Tags)
            .FirstAsync(x => x.Id == faq.Id);

        Assert.Equal(request.Title, savedFaq.Title);
        Assert.Equal(request.Body, savedFaq.Body);
        Assert.Equal(seed.CsvCategory.Id, savedFaq.CategoryId);
        Assert.False(savedFaq.IsPublished);
        Assert.Single(savedFaq.Tags);
        Assert.Equal(seed.CsvTag.Id, savedFaq.Tags.First().Id);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ReturnsNull()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var seed = await SeedMasterDataAsync(dbContext);

        var service = new FaqService(dbContext);

        var request = new FaqUpdateRequest
        {
            Title = "存在しないFAQ",
            Body = "本文",
            CategoryId = seed.LoginCategory.Id,
            TagIds = new List<int> { seed.LoginTag.Id },
            IsPublished = true
        };

        // Act
        var result = await service.UpdateAsync(999, request);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_CategoryNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var seed = await SeedMasterDataAsync(dbContext);

        var faq = new Faq(
            "ログインできない場合",
            "パスワード再設定を確認してください。",
            seed.LoginCategory.Id,
            isPublished: true);

        dbContext.Faqs.Add(faq);
        await dbContext.SaveChangesAsync();

        var service = new FaqService(dbContext);

        var request = new FaqUpdateRequest
        {
            Title = "カテゴリ不正FAQ",
            Body = "本文",
            CategoryId = 999,
            TagIds = new List<int> { seed.LoginTag.Id },
            IsPublished = true
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateAsync(faq.Id, request));

        Assert.Equal("指定されたカテゴリが存在しません。", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_ExistingFaq_ReturnsTrueAndSoftDeletes()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var seed = await SeedMasterDataAsync(dbContext);

        var faq = new Faq(
            "削除対象FAQ",
            "削除対象本文",
            seed.LoginCategory.Id,
            isPublished: true);

        dbContext.Faqs.Add(faq);
        await dbContext.SaveChangesAsync();

        var service = new FaqService(dbContext);

        // Act
        var result = await service.DeleteAsync(faq.Id);

        // Assert
        Assert.True(result);

        var deletedFaq = await dbContext.Faqs.FirstAsync(x => x.Id == faq.Id);
        Assert.NotNull(deletedFaq.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ReturnsFalse()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        await SeedMasterDataAsync(dbContext);

        var service = new FaqService(dbContext);

        // Act
        var result = await service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    private sealed record SeedData(
        Category LoginCategory,
        Category CsvCategory,
        Tag LoginTag,
        Tag CsvTag);
}