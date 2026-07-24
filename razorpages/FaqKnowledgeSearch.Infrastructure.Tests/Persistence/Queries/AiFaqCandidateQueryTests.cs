using System.Reflection;
using FaqKnowledgeSearch.Domain.Faqs;
using FaqKnowledgeSearch.Infrastructure.Persistence;
using FaqKnowledgeSearch.Infrastructure.Persistence.Queries;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FaqKnowledgeSearch.Infrastructure.Tests.Persistence.Queries;

public sealed class AiFaqCandidateQueryTests
{
    // =========================================================
    // 公開状態
    // =========================================================

    [Fact]
    public async Task SearchAsync_PublishedAndUnpublishedFaqsExist_ReturnsOnlyPublishedFaq()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var publishedFaq =
            CreateFaq(
                id: 1,
                title: "ログイン方法",
                isPublished: true);

        var unpublishedFaq =
            CreateFaq(
                id: 2,
                title: "ログイントラブル",
                isPublished: false);

        await SeedAsync(
            database.Context,
            publishedFaq,
            unpublishedFaq);

        var sut =
            new AiFaqCandidateQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                question: "ログイン",
                maxResults: 10);

        // Assert
        var reference =
            Assert.Single(result);

        Assert.Equal(1, reference.Id);
        Assert.Equal(
            "ログイン方法",
            reference.Title);
    }

    // =========================================================
    // 質問のTrim・Normalize
    // =========================================================

    [Fact]
    public async Task SearchAsync_QuestionContainsSpacesAndFullWidthCharacters_NormalizesQuestion()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var faq =
            CreateFaq(
                id: 1,
                title: "Password Reset",
                body: "アカウント情報です。");

        await SeedAsync(
            database.Context,
            faq);

        var sut =
            new AiFaqCandidateQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                question: "  ｐＡｓＳｗＯｒＤ  ",
                maxResults: 10);

        // Assert
        var reference =
            Assert.Single(result);

        Assert.Equal(1, reference.Id);

        // タイトルに質問全体を含む：50点
        // タイトルに抽出語を含む：12点
        Assert.Equal(62, reference.Score);
    }

    // =========================================================
    // 検索語の分割・重複除去
    // =========================================================

    [Fact]
    public async Task SearchAsync_QuestionContainsSeparatorsAndDuplicateTerms_NormalizesTerms()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var faq =
            CreateFaq(
                id: 1,
                title: "アカウント操作",
                body: "ログインとパスワードについて説明します。");

        await SeedAsync(
            database.Context,
            faq);

        var sut =
            new AiFaqCandidateQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                question:
                    "ログイン、ログイン A パスワード",
                maxResults: 10);

        // Assert
        var reference =
            Assert.Single(result);

        // ログイン：本文一致4点
        // パスワード：本文一致4点
        //
        // 重複した「ログイン」は1回だけ。
        // 1文字の「A」は検索語から除外される。
        Assert.Equal(8, reference.Score);
    }

    [Fact]
    public async Task SearchAsync_QuestionContainsOneCharacterOnly_ReturnsEmptyResult()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var faq =
            CreateFaq(
                id: 1,
                title: "A",
                body: "A");

        await SeedAsync(
            database.Context,
            faq);

        var sut =
            new AiFaqCandidateQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                question: "A",
                maxResults: 10);

        // Assert
        Assert.Empty(result);
    }

    // =========================================================
    // スコア計算
    // =========================================================

    [Fact]
    public async Task SearchAsync_KeywordMatchesDifferentFields_CalculatesScoresAndOrdersDescending()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        // 質問全体がタイトルに一致：50
        // 抽出語がタイトルに一致：12
        // 合計：62
        var titleMatch =
            CreateFaq(
                id: 1,
                title: "パスワード変更",
                body: "認証情報を変更します。");

        // 質問全体が本文に一致：20
        // 抽出語が本文に一致：4
        // 合計：24
        var bodyMatch =
            CreateFaq(
                id: 2,
                title: "認証情報の変更",
                body: "パスワードを設定画面から変更します。");

        // 抽出語がカテゴリ名に一致：6
        var categoryMatch =
            CreateFaq(
                id: 3,
                title: "セキュリティ情報",
                body: "認証情報を管理します。",
                categoryName:
                    "パスワード関連");

        // 抽出語がタグ名に一致：8
        var tagMatch =
            CreateFaq(
                id: 4,
                title: "アカウント管理",
                body: "認証情報を管理します。",
                tags:
                [
                    CreateTag(
                        id: 40,
                        name: "パスワード管理",
                        displayOrder: 1)
                ]);

        var noMatch =
            CreateFaq(
                id: 5,
                title: "勤務時間",
                body: "勤務時間について説明します。");

        await SeedAsync(
            database.Context,
            titleMatch,
            bodyMatch,
            categoryMatch,
            tagMatch,
            noMatch);

        var sut =
            new AiFaqCandidateQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                question: "パスワード",
                maxResults: 10);

        // Assert
        Assert.Equal(
            [1, 2, 4, 3],
            result
                .Select(reference => reference.Id)
                .ToArray());

        Assert.Equal(
            [62, 24, 8, 6],
            result
                .Select(reference => reference.Score)
                .ToArray());

        Assert.DoesNotContain(
            result,
            reference => reference.Id == 5);
    }

    [Fact]
    public async Task SearchAsync_NoFaqHasPositiveScore_ReturnsEmptyResult()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        await SeedAsync(
            database.Context,
            CreateFaq(
                id: 1,
                title: "勤務時間",
                body: "勤務時間について説明します。"),
            CreateFaq(
                id: 2,
                title: "休暇申請",
                body: "休暇の申請方法を説明します。"));

        var sut =
            new AiFaqCandidateQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                question: "パスワード",
                maxResults: 10);

        // Assert
        Assert.Empty(result);
    }

    // =========================================================
    // 同点時の並び順
    // =========================================================

    [Fact]
    public async Task SearchAsync_ScoresAreEqual_OrdersByViewCountThenUpdatedAt()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var older =
            new DateTime(
                2026,
                7,
                10,
                0,
                0,
                0,
                DateTimeKind.Utc);

        var newer =
            new DateTime(
                2026,
                7,
                20,
                0,
                0,
                0,
                DateTimeKind.Utc);

        var lowViewCount =
            CreateFaq(
                id: 1,
                title: "ログイン FAQ1",
                viewCount: 10,
                updatedAt: newer);

        var highViewCountOlder =
            CreateFaq(
                id: 2,
                title: "ログイン FAQ2",
                viewCount: 50,
                updatedAt: older);

        var highViewCountNewer =
            CreateFaq(
                id: 3,
                title: "ログイン FAQ3",
                viewCount: 50,
                updatedAt: newer);

        await SeedAsync(
            database.Context,
            lowViewCount,
            highViewCountOlder,
            highViewCountNewer);

        var sut =
            new AiFaqCandidateQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                question: "ログイン",
                maxResults: 10);

        // Assert
        Assert.Equal(
            [3, 2, 1],
            result
                .Select(reference => reference.Id)
                .ToArray());
    }

    // =========================================================
    // 最大取得件数
    // =========================================================

    [Fact]
    public async Task SearchAsync_MaxResultsIsZero_ClampsToOne()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var faqs =
            Enumerable.Range(1, 3)
                .Select(id =>
                    CreateFaq(
                        id: id,
                        title: $"検索対象{id}",
                        viewCount: id))
                .ToArray();

        await SeedAsync(
            database.Context,
            faqs);

        var sut =
            new AiFaqCandidateQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                question: "検索対象",
                maxResults: 0);

        // Assert
        var reference =
            Assert.Single(result);

        // スコアが同じなので閲覧数が最大のFAQ
        Assert.Equal(3, reference.Id);
    }

    [Fact]
    public async Task SearchAsync_MaxResultsExceedsTen_ClampsToTen()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var faqs =
            Enumerable.Range(1, 12)
                .Select(id =>
                    CreateFaq(
                        id: id,
                        title: $"検索対象{id}",
                        viewCount: id))
                .ToArray();

        await SeedAsync(
            database.Context,
            faqs);

        var sut =
            new AiFaqCandidateQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                question: "検索対象",
                maxResults: 100);

        // Assert
        Assert.Equal(10, result.Count);

        Assert.Equal(
            [12, 11, 10, 9, 8, 7, 6, 5, 4, 3],
            result
                .Select(reference => reference.Id)
                .ToArray());
    }

    // =========================================================
    // 戻り値のマッピング
    // =========================================================

    [Fact]
    public async Task SearchAsync_MatchingFaqExists_ReturnsMappedReference()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var faq =
            CreateFaq(
                id: 100,
                title: "パスワード変更方法",
                body: "設定画面から変更できます。",
                categoryName: "アカウント",
                tags:
                [
                    CreateTag(
                        id: 10,
                        name: "ログイン",
                        displayOrder: 1),

                    CreateTag(
                        id: 20,
                        name: "パスワード",
                        displayOrder: 2)
                ]);

        await SeedAsync(
            database.Context,
            faq);

        var sut =
            new AiFaqCandidateQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                question: "パスワード",
                maxResults: 10);

        // Assert
        var reference =
            Assert.Single(result);

        Assert.Equal(100, reference.Id);

        Assert.Equal(
            "パスワード変更方法",
            reference.Title);

        Assert.Equal(
            "設定画面から変更できます。",
            reference.Body);

        Assert.Equal(
            "アカウント",
            reference.CategoryName);

        Assert.Equal(
            ["ログイン", "パスワード"],
            reference.Tags);

        // タイトル：
        // 質問全体50 + 抽出語12
        //
        // タグ：
        // 抽出語8
        //
        // 合計70
        Assert.Equal(70, reference.Score);
    }

    [Fact]
    public async Task SearchAsync_FaqHasUnorderedTags_ReturnsTagsOrderedByDisplayOrderThenId()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var faq =
            CreateFaq(
                id: 1,
                title: "タグ順確認",
                tags:
                [
                    CreateTag(
                        id: 30,
                        name: "タグ30",
                        displayOrder: 2),

                    CreateTag(
                        id: 20,
                        name: "タグ20",
                        displayOrder: 1),

                    CreateTag(
                        id: 10,
                        name: "タグ10",
                        displayOrder: 1)
                ]);

        await SeedAsync(
            database.Context,
            faq);

        var sut =
            new AiFaqCandidateQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                question: "タグ順",
                maxResults: 10);

        // Assert
        var reference =
            Assert.Single(result);

        Assert.Equal(
            ["タグ10", "タグ20", "タグ30"],
            reference.Tags);
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
            new AiFaqCandidateQuery(
                database.Context);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        // Act・Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => sut.SearchAsync(
                question: "ログイン",
                maxResults: 10,
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

        // 追跡中のオブジェクトではなく、
        // SQLiteから再取得させる。
        context.ChangeTracker.Clear();
    }

    private static Faq CreateFaq(
        int id,
        string title,
        string body = "関連情報です。",
        string? categoryName = null,
        bool isPublished = true,
        int viewCount = 0,
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        IReadOnlyCollection<Tag>? tags = null)
    {
        var categoryId =
            10_000 + id;

        var category =
            new Category(
                categoryName
                ?? $"テストカテゴリ{id}",
                displayOrder: id);

        SetMember(
            category,
            nameof(Category.Id),
            categoryId);

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

        // ReplaceTagsはUpdatedAtを変更するため、
        // 固定日時の設定前に呼び出す。
        faq.ReplaceTags(
            tags ?? []);

        SetMember(
            faq,
            nameof(Faq.CreatedAt),
            createdAt
            ?? new DateTime(
                2026,
                7,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc));

        SetMember(
            faq,
            nameof(Faq.UpdatedAt),
            updatedAt
            ?? new DateTime(
                2026,
                7,
                20,
                0,
                0,
                0,
                DateTimeKind.Utc));

        return faq;
    }

    private static Tag CreateTag(
        int id,
        string name,
        int displayOrder)
    {
        var tag =
            new Tag(
                name,
                displayOrder);

        SetMember(
            tag,
            nameof(Tag.Id),
            id);

        return tag;
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