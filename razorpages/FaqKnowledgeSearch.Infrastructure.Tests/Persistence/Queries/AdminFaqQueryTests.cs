using System.Reflection;
using FaqKnowledgeSearch.Domain.Faqs;
using FaqKnowledgeSearch.Infrastructure.Persistence;
using FaqKnowledgeSearch.Infrastructure.Persistence.Queries;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FaqKnowledgeSearch.Infrastructure.Tests.Persistence.Queries;

public sealed class AdminFaqQueryTests
{
    // =========================================================
    // ページ番号・ページサイズ
    // =========================================================

    [Theory]
    [InlineData(0, 0, 1, 1)]
    [InlineData(-10, -10, 1, 1)]
    [InlineData(1, 50, 1, 50)]
    [InlineData(2, 51, 2, 50)]
    [InlineData(3, 100, 3, 50)]
    public async Task SearchAsync_PageValuesAreOutOfRange_NormalizesValues(
        int requestedPage,
        int requestedPageSize,
        int expectedPage,
        int expectedPageSize)
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var sut =
            new AdminFaqQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                requestedPage,
                requestedPageSize);

        // Assert
        Assert.Equal(
            expectedPage,
            result.Page);

        Assert.Equal(
            expectedPageSize,
            result.PageSize);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    // =========================================================
    // 論理削除
    // =========================================================

    [Fact]
    public async Task SearchAsync_DeletedFaqExists_ExcludesDeletedFaq()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var activeFaq =
            CreateFaq(
                id: 1,
                title: "有効なFAQ",
                updatedAt:
                    new DateTime(
                        2026,
                        7,
                        20,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc));

        var deletedFaq =
            CreateFaq(
                id: 2,
                title: "削除済みFAQ",
                deleted: true,
                updatedAt:
                    new DateTime(
                        2026,
                        7,
                        21,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc));

        await SeedAsync(
            database.Context,
            activeFaq,
            deletedFaq);

        var sut =
            new AdminFaqQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                pageNumber: 1,
                pageSize: 10);

        // Assert
        Assert.Equal(1, result.TotalCount);

        var item =
            Assert.Single(result.Items);

        var (
            id,
            title,
            _,
            _,
            _,
            _,
            _) = item;

        Assert.Equal(1, id);
        Assert.Equal("有効なFAQ", title);
    }

    // =========================================================
    // 並び順
    // =========================================================

    [Fact]
    public async Task SearchAsync_MultipleFaqs_OrdersByUpdatedAtDescendingThenIdDescending()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var older =
            new DateTime(
                2026,
                7,
                20,
                0,
                0,
                0,
                DateTimeKind.Utc);

        var newer =
            new DateTime(
                2026,
                7,
                21,
                0,
                0,
                0,
                DateTimeKind.Utc);

        var faqs = new[]
        {
            CreateFaq(
                id: 1,
                title: "古いFAQ",
                updatedAt: older),

            CreateFaq(
                id: 2,
                title: "新しいFAQ ID2",
                updatedAt: newer),

            CreateFaq(
                id: 3,
                title: "新しいFAQ ID3",
                updatedAt: newer)
        };

        await SeedAsync(
            database.Context,
            faqs);

        var sut =
            new AdminFaqQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                pageNumber: 1,
                pageSize: 10);

        // Assert
        Assert.Equal(
            new[] { 3, 2, 1 },
            result.Items
                .Select(item =>
                {
                    var (
                        id,
                        _,
                        _,
                        _,
                        _,
                        _,
                        _) = item;

                    return id;
                })
                .ToArray());
    }

    // =========================================================
    // ページング
    // =========================================================

    [Fact]
    public async Task SearchAsync_SecondPage_ReturnsCorrectItemsAndTotalCount()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var baseDate =
            new DateTime(
                2026,
                7,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

        var faqs =
            Enumerable.Range(1, 5)
                .Select(id =>
                    CreateFaq(
                        id: id,
                        title: $"FAQ{id}",
                        updatedAt:
                            baseDate.AddDays(id)))
                .ToArray();

        await SeedAsync(
            database.Context,
            faqs);

        var sut =
            new AdminFaqQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                pageNumber: 2,
                pageSize: 2);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);

        // 更新日時降順：
        // FAQ5、FAQ4、FAQ3、FAQ2、FAQ1
        // 2ページ目はFAQ3、FAQ2
        Assert.Equal(
            new[] { 3, 2 },
            result.Items
                .Select(item =>
                {
                    var (
                        id,
                        _,
                        _,
                        _,
                        _,
                        _,
                        _) = item;

                    return id;
                })
                .ToArray());
    }

    [Fact]
    public async Task SearchAsync_PageExceedsLastPage_ReturnsEmptyItems()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        await SeedAsync(
            database.Context,
            CreateFaq(
                id: 1,
                title: "FAQ1"),
            CreateFaq(
                id: 2,
                title: "FAQ2"));

        var sut =
            new AdminFaqQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                pageNumber: 10,
                pageSize: 2);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Empty(result.Items);
    }

    // =========================================================
    // 本文プレビュー
    // =========================================================

    [Fact]
    public async Task SearchAsync_BodyIsExactly100Characters_ReturnsOriginalBody()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var body =
            new string('あ', 100);

        await SeedAsync(
            database.Context,
            CreateFaq(
                id: 1,
                title: "100文字FAQ",
                body: body));

        var sut =
            new AdminFaqQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                pageNumber: 1,
                pageSize: 10);

        // Assert
        var item =
            Assert.Single(result.Items);

        var (
            _,
            _,
            bodyPreview,
            _,
            _,
            _,
            _) = item;

        Assert.Equal(body, bodyPreview);
        Assert.Equal(100, bodyPreview.Length);
    }

    [Fact]
    public async Task SearchAsync_BodyExceeds100Characters_ReturnsFirst100CharactersWithEllipsis()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var first100Characters =
            new string('あ', 100);

        var body =
            first100Characters
            + "この部分は表示されない";

        await SeedAsync(
            database.Context,
            CreateFaq(
                id: 1,
                title: "長文FAQ",
                body: body));

        var sut =
            new AdminFaqQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                pageNumber: 1,
                pageSize: 10);

        // Assert
        var item =
            Assert.Single(result.Items);

        var (
            _,
            _,
            bodyPreview,
            _,
            _,
            _,
            _) = item;

        Assert.Equal(
            first100Characters + "…",
            bodyPreview);

        Assert.DoesNotContain(
            "この部分は表示されない",
            bodyPreview);
    }

    // =========================================================
    // DTO変換
    // =========================================================

    [Fact]
    public async Task SearchAsync_FaqExists_ReturnsMappedListItem()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var updatedAt =
            new DateTime(
                2026,
                7,
                20,
                1,
                2,
                3,
                DateTimeKind.Utc);

        var faq =
            CreateFaq(
                id: 100,
                title: "パスワード変更方法",
                body: "設定画面から変更できます。",
                categoryName: "アカウント",
                isPublished: true,
                viewCount: 25,
                updatedAt: updatedAt);

        await SeedAsync(
            database.Context,
            faq);

        var sut =
            new AdminFaqQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                pageNumber: 1,
                pageSize: 10);

        // Assert
        Assert.Equal(1, result.TotalCount);

        var item =
            Assert.Single(result.Items);

        var (
            id,
            title,
            bodyPreview,
            categoryName,
            isPublished,
            viewCount,
            actualUpdatedAt) = item;

        Assert.Equal(100, id);

        Assert.Equal(
            "パスワード変更方法",
            title);

        Assert.Equal(
            "設定画面から変更できます。",
            bodyPreview);

        Assert.Equal(
            "アカウント",
            categoryName);

        Assert.True(isPublished);
        Assert.Equal(25, viewCount);
        Assert.Equal(updatedAt, actualUpdatedAt);
    }

    [Fact]
    public async Task SearchAsync_UnpublishedFaqExists_IncludesUnpublishedFaq()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        await SeedAsync(
            database.Context,
            CreateFaq(
                id: 1,
                title: "非公開FAQ",
                isPublished: false));

        var sut =
            new AdminFaqQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                pageNumber: 1,
                pageSize: 10);

        // Assert
        var item =
            Assert.Single(result.Items);

        var (
            _,
            _,
            _,
            _,
            isPublished,
            _,
            _) = item;

        Assert.False(isPublished);
    }

    // =========================================================
    // CancellationToken
    // =========================================================

    [Fact]
    public async Task SearchAsync_CancellationIsRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var sut =
            new AdminFaqQuery(
                database.Context);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        // Act・Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => sut.SearchAsync(
                pageNumber: 1,
                pageSize: 10,
                cancellationTokenSource.Token));
    }

    // =========================================================
    // Test helpers
    // =========================================================

    private static async Task<TestDatabase>
        CreateDatabaseAsync()
    {
        var connection =
            new SqliteConnection(
                "Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .EnableSensitiveDataLogging()
                .Options;

        var context =
            new AppDbContext(options);

        await context.Database.EnsureCreatedAsync();

        return new TestDatabase(
            connection,
            context);
    }

    private static async Task SeedAsync(
        AppDbContext context,
        params Faq[] faqs)
    {
        context.Faqs.AddRange(faqs);

        await context.SaveChangesAsync();

        // 検索時は追跡済みエンティティではなく、
        // DBから改めて取得させる。
        context.ChangeTracker.Clear();
    }

    private static Faq CreateFaq(
        int id,
        string title,
        string body = "テスト用FAQ本文です。",
        string? categoryName = null,
        bool isPublished = true,
        int viewCount = 0,
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        bool deleted = false)
    {
        var categoryId =
            1_000 + id;

        var category =
            CreateCategory(
                id: categoryId,
                name:
                    categoryName
                    ?? $"テストカテゴリ{id}");

        var faq =
            new Faq(
                title,
                body,
                categoryId,
                isPublished);

        SetMember(
            faq,
            nameof(Faq.Id),
            id);

        SetMember(
            faq,
            nameof(Faq.Category),
            category);

        SetMember(
            faq,
            nameof(Faq.ViewCount),
            viewCount);

        var fixedCreatedAt =
            createdAt
            ?? new DateTime(
                2026,
                7,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

        var fixedUpdatedAt =
            updatedAt
            ?? new DateTime(
                2026,
                7,
                20,
                0,
                0,
                0,
                DateTimeKind.Utc);

        SetMember(
            faq,
            nameof(Faq.CreatedAt),
            fixedCreatedAt);

        SetMember(
            faq,
            nameof(Faq.UpdatedAt),
            fixedUpdatedAt);

        if (deleted)
        {
            faq.Delete();

            SetMember(
                faq,
                nameof(Faq.DeletedAt),
                fixedUpdatedAt);

            SetMember(
                faq,
                nameof(Faq.UpdatedAt),
                fixedUpdatedAt);
        }

        return faq;
    }

    private static Category CreateCategory(
        int id,
        string name)
    {
        var category =
            new Category(
                name,
                displayOrder: id);

        SetMember(
            category,
            nameof(Category.Id),
            id);

        return category;
    }

    private static void SetMember<TValue>(
        object target,
        string propertyName,
        TValue value)
    {
        const BindingFlags flags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        var type =
            target.GetType();

        var property =
            type.GetProperty(
                propertyName,
                flags);

        if (property?.SetMethod is not null)
        {
            property.SetValue(
                target,
                value);

            return;
        }

        var backingField =
            type.GetField(
                $"<{propertyName}>k__BackingField",
                flags);

        if (backingField is not null)
        {
            backingField.SetValue(
                target,
                value);

            return;
        }

        throw new InvalidOperationException(
            $"{type.Name}.{propertyName}を設定できません。");
    }

    private sealed class TestDatabase(
        SqliteConnection connection,
        AppDbContext context)
        : IAsyncDisposable
    {
        private readonly SqliteConnection _connection =
            connection;

        public AppDbContext Context { get; } =
            context;

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}