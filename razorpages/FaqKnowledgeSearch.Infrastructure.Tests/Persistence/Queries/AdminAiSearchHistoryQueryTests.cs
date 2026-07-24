using System.Reflection;
using FaqKnowledgeSearch.Application.AiSearch.History.Admin;
using FaqKnowledgeSearch.Domain.AiSearch;
using FaqKnowledgeSearch.Infrastructure.Persistence;
using FaqKnowledgeSearch.Infrastructure.Persistence.Queries;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FaqKnowledgeSearch.Infrastructure.Tests.Persistence.Queries;

public sealed class AdminAiSearchHistoryQueryTests
{
    // =========================================================
    // SearchAsync：入力値
    // =========================================================

    [Fact]
    public async Task SearchAsync_ConditionIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var sut =
            new AdminAiSearchHistoryQuery(
                database.Context);

        // Act・Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.SearchAsync(null!));
    }

    [Theory]
    [InlineData(0, 0, 1, 1)]
    [InlineData(-10, -10, 1, 1)]
    [InlineData(1, 100, 1, 100)]
    [InlineData(2, 101, 2, 100)]
    [InlineData(3, 500, 3, 100)]
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
            new AdminAiSearchHistoryQuery(
                database.Context);

        var condition =
            new AdminAiSearchHistorySearchCondition
            {
                Page = requestedPage,
                PageSize = requestedPageSize
            };

        // Act
        var result =
            await sut.SearchAsync(condition);

        // Assert
        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedPageSize, result.PageSize);
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    // =========================================================
    // SearchAsync：キーワード検索
    // =========================================================

    [Fact]
    public async Task SearchAsync_KeywordIsSpecified_SearchesQuestionAnswerAndErrorMessage()
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

        var questionMatch =
            CreateSuccessHistory(
                id: 1,
                question: "Needleを含む質問",
                answer: "通常の回答",
                createdAt: baseDate.AddDays(1));

        var answerMatch =
            CreateSuccessHistory(
                id: 2,
                question: "通常の質問",
                answer: "Needleを含む回答",
                createdAt: baseDate.AddDays(2));

        var errorMatch =
            CreateFailureHistory(
                id: 3,
                question: "失敗する質問",
                errorMessage: "Needleを含むエラー",
                createdAt: baseDate.AddDays(3));

        var notMatch =
            CreateSuccessHistory(
                id: 4,
                question: "対象外の質問",
                answer: "対象外の回答",
                createdAt: baseDate.AddDays(4));

        await SeedAsync(
            database.Context,
            questionMatch,
            answerMatch,
            errorMatch,
            notMatch);

        var sut =
            new AdminAiSearchHistoryQuery(
                database.Context);

        var condition =
            new AdminAiSearchHistorySearchCondition
            {
                Keyword = "  Needle  ",
                Page = 1,
                PageSize = 10
            };

        // Act
        var result =
            await sut.SearchAsync(condition);

        // Assert
        Assert.Equal(3, result.TotalCount);

        // CreatedAt降順
        Assert.Equal(
            [3L, 2L, 1L],
            result.Items
                .Select(item =>
                    GetPropertyValue<long>(
                        item,
                        "Id"))
                .ToArray());
    }

    [Fact]
    public async Task SearchAsync_KeywordIsWhiteSpace_DoesNotFilter()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        await SeedAsync(
            database.Context,
            CreateSuccessHistory(
                id: 1,
                question: "質問1",
                answer: "回答1"),
            CreateSuccessHistory(
                id: 2,
                question: "質問2",
                answer: "回答2"));

        var sut =
            new AdminAiSearchHistoryQuery(
                database.Context);

        var condition =
            new AdminAiSearchHistorySearchCondition
            {
                Keyword = "   ",
                Page = 1,
                PageSize = 10
            };

        // Act
        var result =
            await sut.SearchAsync(condition);

        // Assert
        Assert.Equal(2, result.TotalCount);
    }

    // =========================================================
    // SearchAsync：ステータス
    // =========================================================

    [Fact]
    public async Task SearchAsync_StatusIsSuccess_ReturnsOnlySuccessfulHistories()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        await SeedAsync(
            database.Context,
            CreateSuccessHistory(
                id: 1,
                question: "成功1",
                answer: "回答1"),
            CreateFailureHistory(
                id: 2,
                question: "失敗",
                errorMessage: "エラー"),
            CreateSuccessHistory(
                id: 3,
                question: "成功2",
                answer: "回答2"));

        var sut =
            new AdminAiSearchHistoryQuery(
                database.Context);

        var condition =
            new AdminAiSearchHistorySearchCondition
            {
                Status =
                    AdminAiSearchStatusFilter.Success
            };

        // Act
        var result =
            await sut.SearchAsync(condition);

        // Assert
        Assert.Equal(2, result.TotalCount);

        Assert.All(
            result.Items,
            item => Assert.True(
                GetPropertyValue<bool>(
                    item,
                    "IsSuccess")));
    }

    [Fact]
    public async Task SearchAsync_StatusIsFailure_ReturnsOnlyFailedHistories()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        await SeedAsync(
            database.Context,
            CreateSuccessHistory(
                id: 1,
                question: "成功",
                answer: "回答"),
            CreateFailureHistory(
                id: 2,
                question: "失敗1",
                errorMessage: "エラー1"),
            CreateFailureHistory(
                id: 3,
                question: "失敗2",
                errorMessage: "エラー2"));

        var sut =
            new AdminAiSearchHistoryQuery(
                database.Context);

        var condition =
            new AdminAiSearchHistorySearchCondition
            {
                Status =
                    AdminAiSearchStatusFilter.Failure
            };

        // Act
        var result =
            await sut.SearchAsync(condition);

        // Assert
        Assert.Equal(2, result.TotalCount);

        Assert.All(
            result.Items,
            item => Assert.False(
                GetPropertyValue<bool>(
                    item,
                    "IsSuccess")));
    }

    // =========================================================
    // SearchAsync：フィードバック
    // =========================================================

    [Fact]
    public async Task SearchAsync_FeedbackIsHelpful_ReturnsOnlyHelpfulHistories()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        await SeedFeedbackHistoriesAsync(
            database.Context);

        var sut =
            new AdminAiSearchHistoryQuery(
                database.Context);

        var condition =
            new AdminAiSearchHistorySearchCondition
            {
                Feedback =
                    AdminAiSearchFeedbackFilter.Helpful
            };

        // Act
        var result =
            await sut.SearchAsync(condition);

        // Assert
        var item =
            Assert.Single(result.Items);

        Assert.True(
            GetPropertyValue<bool?>(
                item,
                "WasHelpful"));
    }

    [Fact]
    public async Task SearchAsync_FeedbackIsNotHelpful_ReturnsOnlyNotHelpfulHistories()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        await SeedFeedbackHistoriesAsync(
            database.Context);

        var sut =
            new AdminAiSearchHistoryQuery(
                database.Context);

        var condition =
            new AdminAiSearchHistorySearchCondition
            {
                Feedback =
                    AdminAiSearchFeedbackFilter.NotHelpful
            };

        // Act
        var result =
            await sut.SearchAsync(condition);

        // Assert
        var item =
            Assert.Single(result.Items);

        Assert.False(
            GetPropertyValue<bool?>(
                item,
                "WasHelpful"));
    }

    [Fact]
    public async Task SearchAsync_FeedbackIsNone_ReturnsHistoriesWithoutFeedback()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        await SeedFeedbackHistoriesAsync(
            database.Context);

        var sut =
            new AdminAiSearchHistoryQuery(
                database.Context);

        var condition =
            new AdminAiSearchHistorySearchCondition
            {
                Feedback =
                    AdminAiSearchFeedbackFilter.None
            };

        // Act
        var result =
            await sut.SearchAsync(condition);

        // Assert
        Assert.Equal(2, result.TotalCount);

        Assert.All(
            result.Items,
            item => Assert.Null(
                GetPropertyValue<bool?>(
                    item,
                    "WasHelpful")));
    }

    // =========================================================
    // SearchAsync：並び順・ページング
    // =========================================================

    [Fact]
    public async Task SearchAsync_OrdersByCreatedAtDescendingThenIdDescending()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var older =
            new DateTime(
                2026,
                7,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

        var newer =
            new DateTime(
                2026,
                7,
                2,
                0,
                0,
                0,
                DateTimeKind.Utc);

        await SeedAsync(
            database.Context,
            CreateSuccessHistory(
                id: 1,
                question: "古い履歴",
                answer: "回答",
                createdAt: older),
            CreateSuccessHistory(
                id: 2,
                question: "新しい履歴ID2",
                answer: "回答",
                createdAt: newer),
            CreateSuccessHistory(
                id: 3,
                question: "新しい履歴ID3",
                answer: "回答",
                createdAt: newer));

        var sut =
            new AdminAiSearchHistoryQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                new AdminAiSearchHistorySearchCondition());

        // Assert
        Assert.Equal(
            [3L, 2L, 1L],
            result.Items
                .Select(item =>
                    GetPropertyValue<long>(
                        item,
                        "Id"))
                .ToArray());
    }

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

        var histories =
            Enumerable.Range(1, 5)
                .Select(index =>
                    CreateSuccessHistory(
                        id: index,
                        question: $"質問{index}",
                        answer: $"回答{index}",
                        createdAt:
                            baseDate.AddDays(index)))
                .ToArray();

        await SeedAsync(
            database.Context,
            histories);

        var sut =
            new AdminAiSearchHistoryQuery(
                database.Context);

        var condition =
            new AdminAiSearchHistorySearchCondition
            {
                Page = 2,
                PageSize = 2
            };

        // Act
        var result =
            await sut.SearchAsync(condition);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);

        // 降順は5、4、3、2、1。
        // 2ページ目は3、2。
        Assert.Equal(
            [3L, 2L],
            result.Items
                .Select(item =>
                    GetPropertyValue<long>(
                        item,
                        "Id"))
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
            CreateSuccessHistory(
                id: 1,
                question: "質問1",
                answer: "回答1"),
            CreateSuccessHistory(
                id: 2,
                question: "質問2",
                answer: "回答2"));

        var sut =
            new AdminAiSearchHistoryQuery(
                database.Context);

        var condition =
            new AdminAiSearchHistorySearchCondition
            {
                Page = 10,
                PageSize = 2
            };

        // Act
        var result =
            await sut.SearchAsync(condition);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Empty(result.Items);
    }

    // =========================================================
    // SearchAsync：プレビュー・参照件数
    // =========================================================

    [Fact]
    public async Task SearchAsync_ReturnsPreviewFromAnswerOrErrorMessage()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var success =
            CreateSuccessHistory(
                id: 1,
                question: "成功質問",
                answer: "1行目\r\n2行目",
                createdAt:
                    new DateTime(
                        2026,
                        7,
                        2,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc));

        var longError =
            new string('あ', 101);

        var failure =
            CreateFailureHistory(
                id: 2,
                question: "失敗質問",
                errorMessage: longError,
                createdAt:
                    new DateTime(
                        2026,
                        7,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc));

        await SeedAsync(
            database.Context,
            success,
            failure);

        var sut =
            new AdminAiSearchHistoryQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                new AdminAiSearchHistorySearchCondition());

        // Assert
        var successItem =
            result.Items.Single(item =>
                item.Id == 1);

        var failureItem =
            result.Items.Single(item =>
                item.Id == 2);

        Assert.Equal(
            "1行目  2行目",
            successItem.ResponsePreview);

        Assert.Equal(
            new string('あ', 100) + "…",
            failureItem.ResponsePreview);
    }

    [Fact]
    public async Task SearchAsync_HistoryHasReferences_ReturnsReferenceCount()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var history =
            CreateSuccessHistory(
                id: 1,
                question: "参照あり質問",
                answer: "回答");

        AddReference(
            history,
            referenceId: 10,
            faqId: 100,
            title: "FAQ100",
            categoryName: "カテゴリ",
            displayOrder: 1,
            score: 100);

        AddReference(
            history,
            referenceId: 20,
            faqId: 200,
            title: "FAQ200",
            categoryName: "カテゴリ",
            displayOrder: 2,
            score: 80);

        await SeedAsync(
            database.Context,
            history);

        var sut =
            new AdminAiSearchHistoryQuery(
                database.Context);

        // Act
        var result =
            await sut.SearchAsync(
                new AdminAiSearchHistorySearchCondition());

        // Assert
        var item =
            Assert.Single(result.Items);

        Assert.Equal(
            2,
            GetPropertyValue<int>(
                item,
                "ReferenceCount"));
    }

    // =========================================================
    // GetDetailAsync
    // =========================================================

    [Fact]
    public async Task GetDetailAsync_HistoryDoesNotExist_ReturnsNull()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var sut =
            new AdminAiSearchHistoryQuery(
                database.Context);

        // Act
        var result =
            await sut.GetDetailAsync(
                historyId: 999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetDetailAsync_HistoryExists_ReturnsMappedDetail()
    {
        // Arrange
        await using var database =
            await CreateDatabaseAsync();

        var createdAt =
            new DateTime(
                2026,
                7,
                20,
                1,
                2,
                3,
                DateTimeKind.Utc);

        var history =
            CreateSuccessHistory(
                id: 100,
                question: "パスワードの変更方法は？",
                answer: "設定画面から変更できます。",
                modelName: "test-model",
                usedExternalAi: true,
                wasHelpful: true,
                createdAt: createdAt);

        AddReference(
            history,
            referenceId: 30,
            faqId: 300,
            title: "FAQ300",
            categoryName: "カテゴリ3",
            displayOrder: 3,
            score: 70);

        AddReference(
            history,
            referenceId: 20,
            faqId: 200,
            title: "FAQ200",
            categoryName: "カテゴリ2",
            displayOrder: 2,
            score: 90);

        AddReference(
            history,
            referenceId: 10,
            faqId: 100,
            title: "FAQ100",
            categoryName: "カテゴリ1",
            displayOrder: 1,
            score: 100);

        await SeedAsync(
            database.Context,
            history);

        var sut =
            new AdminAiSearchHistoryQuery(
                database.Context);

        // Act
        var result =
            await sut.GetDetailAsync(
                historyId: 100);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            100L,
            GetPropertyValue<long>(
                result,
                "Id"));

        Assert.Equal(
            "パスワードの変更方法は？",
            GetPropertyValue<string>(
                result,
                "Question"));

        Assert.Equal(
            "設定画面から変更できます。",
            GetPropertyValue<string?>(
                result,
                "Answer"));

        Assert.True(
            GetPropertyValue<bool>(
                result,
                "IsSuccess"));

        Assert.Null(
            GetPropertyValue<string?>(
                result,
                "ErrorMessage"));

        Assert.Equal(
            "test-model",
            GetPropertyValue<string>(
                result,
                "ModelName"));

        Assert.True(
            GetPropertyValue<bool>(
                result,
                "UsedExternalAi"));

        Assert.True(
            GetPropertyValue<bool?>(
                result,
                "WasHelpful"));

        Assert.Equal(
            createdAt,
            GetPropertyValue<DateTime>(
                result,
                "CreatedAt"));
    }

[Fact]
public async Task GetDetailAsync_HistoryHasReferences_OrdersByDisplayOrder()
{
    // Arrange
    await using var database =
        await CreateDatabaseAsync();

    var history =
        CreateSuccessHistory(
            id: 100,
            question: "質問",
            answer: "回答");

    AddReference(
        history,
        referenceId: 30,
        faqId: 300,
        title: "表示順3",
        categoryName: "カテゴリ",
        displayOrder: 3,
        score: 70);

    AddReference(
        history,
        referenceId: 20,
        faqId: 200,
        title: "表示順2",
        categoryName: "カテゴリ",
        displayOrder: 2,
        score: 80);

    AddReference(
        history,
        referenceId: 10,
        faqId: 100,
        title: "表示順1",
        categoryName: "カテゴリ",
        displayOrder: 1,
        score: 90);

    await SeedAsync(
        database.Context,
        history);

    var sut =
        new AdminAiSearchHistoryQuery(
            database.Context);

    // Act
    var result =
        await sut.GetDetailAsync(
            historyId: 100);

    // Assert
    Assert.NotNull(result);

    var references =
        GetPropertyValue<
            IReadOnlyList<
                AdminAiSearchHistoryReferenceItem>>(
            result,
            "References");

    Assert.Equal(
        [100, 200, 300],
        references
            .Select(reference =>
                GetPropertyValue<int>(
                    reference,
                    "FaqId"))
            .ToArray());

    Assert.Equal(
        [1, 2, 3],
        references
            .Select(reference =>
                GetPropertyValue<int>(
                    reference,
                    "DisplayOrder"))
            .ToArray());

    Assert.Equal(
        [90, 80, 70],
        references
            .Select(reference =>
                GetPropertyValue<int>(
                    reference,
                    "Score"))
            .ToArray());
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
        params AiSearchHistory[] histories)
    {
        context.AiSearchHistories.AddRange(
            histories);

        await context.SaveChangesAsync();

        // 以後の検索を追跡済みエンティティに依存させず、
        // DBから取得させる。
        context.ChangeTracker.Clear();
    }

    private static async Task SeedFeedbackHistoriesAsync(
        AppDbContext context)
    {
        var helpful =
            CreateSuccessHistory(
                id: 1,
                question: "役に立った履歴",
                answer: "回答",
                wasHelpful: true);

        var notHelpful =
            CreateSuccessHistory(
                id: 2,
                question: "役に立たなかった履歴",
                answer: "回答",
                wasHelpful: false);

        var noFeedback =
            CreateSuccessHistory(
                id: 3,
                question: "未回答履歴",
                answer: "回答");

        var failure =
            CreateFailureHistory(
                id: 4,
                question: "失敗履歴",
                errorMessage: "エラー");

        await SeedAsync(
            context,
            helpful,
            notHelpful,
            noFeedback,
            failure);
    }

    private static AiSearchHistory CreateSuccessHistory(
        long id,
        string question,
        string answer,
        string modelName = "test-model",
        bool usedExternalAi = true,
        bool? wasHelpful = null,
        DateTime? createdAt = null)
    {
        var history =
            AiSearchHistory.CreateSuccess(
                question,
                answer,
                modelName,
                usedExternalAi);

        if (wasHelpful.HasValue)
        {
            history.SetFeedback(
                wasHelpful.Value);
        }

        SetMember(
            history,
            nameof(AiSearchHistory.Id),
            id);

        SetMember(
            history,
            nameof(AiSearchHistory.CreatedAt),
            createdAt ??
            new DateTime(
                2026,
                7,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc));

        return history;
    }

    private static AiSearchHistory CreateFailureHistory(
        long id,
        string question,
        string errorMessage,
        string modelName = "test-model",
        bool usedExternalAi = true,
        DateTime? createdAt = null)
    {
        var history =
            AiSearchHistory.CreateFailure(
                question,
                errorMessage,
                modelName,
                usedExternalAi);

        SetMember(
            history,
            nameof(AiSearchHistory.Id),
            id);

        SetMember(
            history,
            nameof(AiSearchHistory.CreatedAt),
            createdAt ??
            new DateTime(
                2026,
                7,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc));

        return history;
    }

    private static void AddReference(
        AiSearchHistory history,
        long referenceId,
        int faqId,
        string title,
        string categoryName,
        int displayOrder,
        int score)
    {
        history.AddReference(
            faqId,
            title,
            categoryName,
            displayOrder,
            score);

        var reference =
            history.References.Last();

        SetMember(
            reference,
            nameof(AiSearchReference.Id),
            referenceId);
    }

    private static T GetPropertyValue<T>(
        object target,
        params string[] candidatePropertyNames)
    {
        var type =
            target.GetType();

        foreach (var propertyName
                 in candidatePropertyNames)
        {
            var property =
                type.GetProperty(
                    propertyName,
                    BindingFlags.Instance |
                    BindingFlags.Public);

            if (property is null)
            {
                continue;
            }

            var value =
                property.GetValue(target);

            if (value is null)
            {
                return default!;
            }

            if (value is T typedValue)
            {
                return typedValue;
            }
        }

        throw new InvalidOperationException(
            $"{type.Name}に対象プロパティがありません。"
            + $"候補: "
            + $"{string.Join(", ", candidatePropertyNames)}");
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