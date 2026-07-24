using System.Reflection;
using System.Runtime.CompilerServices;
using FaqKnowledgeSearch.Application.Faqs.Abstractions;
using FaqKnowledgeSearch.Application.Faqs.Public;
using FaqKnowledgeSearch.Domain.Faqs;
using Moq;

namespace FaqKnowledgeSearch.Application.Tests.Faqs.Public;

public sealed class PublicFaqServiceTests
{
    private readonly Mock<IFaqRepository> _faqRepositoryMock;
    private readonly PublicFaqService _sut;

    public PublicFaqServiceTests()
    {
        _faqRepositoryMock =
            new Mock<IFaqRepository>(
                MockBehavior.Strict);

        _sut =
            new PublicFaqService(
                _faqRepositoryMock.Object);
    }

    // =========================================================
    // SearchAsync：入力値
    // =========================================================

    [Fact]
    public async Task SearchAsync_ConditionIsNull_ThrowsArgumentNullException()
    {
        // Act
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sut.SearchAsync(null!));

        // Assert
        _faqRepositoryMock.Verify(
            x => x.GetPublishedAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0, 0, 1, 1)]
    [InlineData(-10, -10, 1, 1)]
    [InlineData(1, 51, 1, 50)]
    [InlineData(2, 100, 2, 50)]
    public async Task SearchAsync_PageAndPageSizeAreOutOfRange_NormalizesValues(
        int requestedPage,
        int requestedPageSize,
        int expectedPage,
        int expectedPageSize)
    {
        // Arrange
        var cancellationToken =
            CancellationToken.None;

        var condition =
            CreateCondition(
                page: requestedPage,
                pageSize: requestedPageSize);

        _faqRepositoryMock
            .Setup(x => x.GetPublishedAsync(
                cancellationToken))
            .ReturnsAsync(
                Array.Empty<Faq>());

        // Act
        var result =
            await _sut.SearchAsync(
                condition,
                cancellationToken);

        // Assert
        Assert.Equal(expectedPage, result.Page);
        Assert.Equal(expectedPageSize, result.PageSize);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    // =========================================================
    // SearchAsync：カテゴリ・タグ絞り込み
    // =========================================================

    [Fact]
    public async Task SearchAsync_CategoryIdIsSpecified_ReturnsMatchingFaqs()
    {
        // Arrange
        var faqs = new[]
        {
            CreateFaq(
                id: 1,
                title: "カテゴリ1 FAQ",
                categoryId: 1,
                categoryName: "カテゴリ1"),

            CreateFaq(
                id: 2,
                title: "カテゴリ2 FAQ",
                categoryId: 2,
                categoryName: "カテゴリ2")
        };

        SetupPublishedFaqs(faqs);

        var condition =
            CreateCondition(
                categoryId: 2);

        // Act
        var result =
            await _sut.SearchAsync(condition);

        // Assert
        var item =
            Assert.Single(result.Items);

        Assert.Equal(2, item.Id);
        Assert.Equal(2, item.CategoryId);
        Assert.Equal("カテゴリ2", item.CategoryName);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task SearchAsync_TagIdIsSpecified_ReturnsFaqContainingTag()
    {
        // Arrange
        var targetTag =
            CreateTag(
                id: 20,
                name: "パスワード",
                displayOrder: 1);

        var faqs = new[]
        {
            CreateFaq(
                id: 1,
                title: "対象FAQ",
                tags: [targetTag]),

            CreateFaq(
                id: 2,
                title: "対象外FAQ",
                tags:
                [
                    CreateTag(
                        id: 30,
                        name: "アカウント",
                        displayOrder: 1)
                ])
        };

        SetupPublishedFaqs(faqs);

        var condition =
            CreateCondition(
                tagId: 20);

        // Act
        var result =
            await _sut.SearchAsync(condition);

        // Assert
        var item =
            Assert.Single(result.Items);

        Assert.Equal(1, item.Id);
        Assert.Equal("対象FAQ", item.Title);
    }

    [Fact]
    public async Task SearchAsync_CategoryAndTagAreSpecified_AppliesBothConditions()
    {
        // Arrange
        var targetTag =
            CreateTag(
                id: 10,
                name: "ログイン",
                displayOrder: 1);

        var faqs = new[]
        {
            CreateFaq(
                id: 1,
                title: "両方一致",
                categoryId: 1,
                tags: [targetTag]),

            CreateFaq(
                id: 2,
                title: "カテゴリのみ一致",
                categoryId: 1),

            CreateFaq(
                id: 3,
                title: "タグのみ一致",
                categoryId: 2,
                tags: [targetTag])
        };

        SetupPublishedFaqs(faqs);

        var condition =
            CreateCondition(
                categoryId: 1,
                tagId: 10);

        // Act
        var result =
            await _sut.SearchAsync(condition);

        // Assert
        var item =
            Assert.Single(result.Items);

        Assert.Equal(1, item.Id);
        Assert.Equal("両方一致", item.Title);
    }

    // =========================================================
    // SearchAsync：キーワード検索
    // =========================================================

    [Fact]
    public async Task SearchAsync_KeywordIsSpecified_SearchesAllSupportedFields()
    {
        // Arrange
        const string keyword = "Needle";

        var faqs = new[]
        {
            CreateFaq(
                id: 1,
                title: "Needle を含むタイトル"),

            CreateFaq(
                id: 2,
                title: "本文一致",
                body: "この本文には needle が含まれています。"),

            CreateFaq(
                id: 3,
                title: "カテゴリ一致",
                categoryName: "Needleカテゴリ"),

            CreateFaq(
                id: 4,
                title: "タグ一致",
                tags:
                [
                    CreateTag(
                        id: 10,
                        name: "needleタグ",
                        displayOrder: 1)
                ]),

            CreateFaq(
                id: 5,
                title: "一致しないFAQ",
                body: "検索対象外の本文です。")
        };

        SetupPublishedFaqs(faqs);

        var condition =
            CreateCondition(
                keyword: keyword);

        // Act
        var result =
            await _sut.SearchAsync(condition);

        // Assert
        Assert.Equal(4, result.TotalCount);

        Assert.Equal(
            new[] { 1, 4, 3, 2 },
            result.Items
                .Select(item => item.Id)
                .ToArray());
    }

    [Fact]
    public async Task SearchAsync_MultipleKeywords_RequiresAllKeywords()
    {
        // Arrange
        var faqs = new[]
        {
            CreateFaq(
                id: 1,
                title: "ログインできない",
                body: "パスワードを再設定してください。"),

            CreateFaq(
                id: 2,
                title: "ログインできない",
                body: "管理者へ連絡してください。"),

            CreateFaq(
                id: 3,
                title: "パスワード変更",
                body: "設定画面で変更できます。")
        };

        SetupPublishedFaqs(faqs);

        var condition =
            CreateCondition(
                keyword: "ログイン パスワード");

        // Act
        var result =
            await _sut.SearchAsync(condition);

        // Assert
        var item =
            Assert.Single(result.Items);

        Assert.Equal(1, item.Id);
    }

    [Fact]
    public async Task SearchAsync_DuplicateKeywords_IgnoresCaseAndDuplicates()
    {
        // Arrange
        var faq =
            CreateFaq(
                id: 1,
                title: "LOGINについて",
                body: "ログイン方法を説明します。");

        SetupPublishedFaqs([faq]);

        var condition =
            CreateCondition(
                keyword: "login LOGIN Login");

        // Act
        var result =
            await _sut.SearchAsync(condition);

        // Assert
        var item =
            Assert.Single(result.Items);

        // 重複除外されるため、タイトル部分一致40点を1回だけ加算
        Assert.Equal(40, item.RelevanceScore);
    }

    // =========================================================
    // SearchAsync：関連度
    // =========================================================

    [Fact]
    public async Task SearchAsync_RelevanceSort_OrdersByCalculatedScore()
    {
        // Arrange
        const string keyword = "パスワード";

        var faqs = new[]
        {
            // タイトル完全一致100 + 本文一致10 = 110
            CreateFaq(
                id: 1,
                title: "パスワード",
                body: "パスワードについて説明します。"),

            // タイトル部分一致40
            CreateFaq(
                id: 2,
                title: "パスワード変更方法"),

            // タグ完全一致30
            CreateFaq(
                id: 3,
                title: "アカウント設定",
                tags:
                [
                    CreateTag(
                        id: 10,
                        name: "パスワード",
                        displayOrder: 1)
                ]),

            // 本文一致10
            CreateFaq(
                id: 4,
                title: "セキュリティ",
                body: "パスワードを定期的に変更してください。")
        };

        SetupPublishedFaqs(faqs);

        var condition =
            CreateCondition(
                keyword: keyword,
                sortOrder: FaqSortOrder.Relevance);

        // Act
        var result =
            await _sut.SearchAsync(condition);

        // Assert
        Assert.Equal(
            new[] { 1, 2, 3, 4 },
            result.Items
                .Select(item => item.Id)
                .ToArray());

        Assert.Equal(
            new[] { 110, 40, 30, 10 },
            result.Items
                .Select(item => item.RelevanceScore)
                .ToArray());
    }

    [Fact]
    public async Task SearchAsync_RelevanceScoresAreEqual_OrdersByViewCount()
    {
        // Arrange
        var updatedAt =
            new DateTime(
                2026,
                7,
                22,
                0,
                0,
                0,
                DateTimeKind.Utc);

        var faqs = new[]
        {
            CreateFaq(
                id: 1,
                title: "ログイン FAQ1",
                viewCount: 10,
                updatedAt: updatedAt),

            CreateFaq(
                id: 2,
                title: "ログイン FAQ2",
                viewCount: 50,
                updatedAt: updatedAt)
        };

        SetupPublishedFaqs(faqs);

        var condition =
            CreateCondition(
                keyword: "ログイン",
                sortOrder: FaqSortOrder.Relevance);

        // Act
        var result =
            await _sut.SearchAsync(condition);

        // Assert
        Assert.Equal(
            new[] { 2, 1 },
            result.Items
                .Select(item => item.Id)
                .ToArray());
    }

    // =========================================================
    // SearchAsync：ソート
    // =========================================================

    [Fact]
    public async Task SearchAsync_NewestSort_OrdersByUpdatedAtThenId()
    {
        // Arrange
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

        SetupPublishedFaqs(faqs);

        var condition =
            CreateCondition(
                sortOrder: FaqSortOrder.Newest);

        // Act
        var result =
            await _sut.SearchAsync(condition);

        // Assert
        Assert.Equal(
            new[] { 3, 2, 1 },
            result.Items
                .Select(item => item.Id)
                .ToArray());
    }

    [Fact]
    public async Task SearchAsync_MostViewedSort_OrdersByViewCountThenUpdatedAtThenId()
    {
        // Arrange
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
                title: "閲覧数10",
                viewCount: 10,
                updatedAt: newer),

            CreateFaq(
                id: 2,
                title: "閲覧数50・古い",
                viewCount: 50,
                updatedAt: older),

            CreateFaq(
                id: 3,
                title: "閲覧数50・新しいID3",
                viewCount: 50,
                updatedAt: newer),

            CreateFaq(
                id: 4,
                title: "閲覧数50・新しいID4",
                viewCount: 50,
                updatedAt: newer)
        };

        SetupPublishedFaqs(faqs);

        var condition =
            CreateCondition(
                sortOrder: FaqSortOrder.MostViewed);

        // Act
        var result =
            await _sut.SearchAsync(condition);

        // Assert
        Assert.Equal(
            new[] { 4, 3, 2, 1 },
            result.Items
                .Select(item => item.Id)
                .ToArray());
    }

    [Fact]
    public async Task SearchAsync_RelevanceSortWithoutKeyword_FallsBackToNewestSort()
    {
        // Arrange
        var faqs = new[]
        {
            CreateFaq(
                id: 1,
                title: "古いFAQ",
                updatedAt:
                    new DateTime(
                        2026,
                        7,
                        20,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc)),

            CreateFaq(
                id: 2,
                title: "新しいFAQ",
                updatedAt:
                    new DateTime(
                        2026,
                        7,
                        21,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc))
        };

        SetupPublishedFaqs(faqs);

        var condition =
            CreateCondition(
                keyword: null,
                sortOrder: FaqSortOrder.Relevance);

        // Act
        var result =
            await _sut.SearchAsync(condition);

        // Assert
        Assert.Equal(
            new[] { 2, 1 },
            result.Items
                .Select(item => item.Id)
                .ToArray());

        Assert.All(
            result.Items,
            item => Assert.Equal(
                0,
                item.RelevanceScore));
    }

    // =========================================================
    // SearchAsync：ページング
    // =========================================================

    [Fact]
    public async Task SearchAsync_SecondPage_ReturnsCorrectItemsAndTotalCount()
    {
        // Arrange
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

        SetupPublishedFaqs(faqs);

        var condition =
            CreateCondition(
                page: 2,
                pageSize: 2,
                sortOrder: FaqSortOrder.Newest);

        // Act
        var result =
            await _sut.SearchAsync(condition);

        // Assert
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);

        // Newest順は 5,4,3,2,1。2ページ目は3,2
        Assert.Equal(
            new[] { 3, 2 },
            result.Items
                .Select(item => item.Id)
                .ToArray());
    }

    [Fact]
    public async Task SearchAsync_PageExceedsLastPage_ReturnsEmptyItems()
    {
        // Arrange
        var faqs = new[]
        {
            CreateFaq(
                id: 1,
                title: "FAQ1"),

            CreateFaq(
                id: 2,
                title: "FAQ2")
        };

        SetupPublishedFaqs(faqs);

        var condition =
            CreateCondition(
                page: 10,
                pageSize: 2);

        // Act
        var result =
            await _sut.SearchAsync(condition);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Empty(result.Items);
    }

    // =========================================================
    // SearchAsync：本文抜粋
    // =========================================================

    [Fact]
    public async Task SearchAsync_BodyIs120CharactersOrLess_ReturnsOriginalBody()
    {
        // Arrange
        var body =
            new string('あ', 120);

        var faq =
            CreateFaq(
                id: 1,
                title: "短い本文",
                body: body);

        SetupPublishedFaqs([faq]);

        // Act
        var result =
            await _sut.SearchAsync(
                CreateCondition());

        // Assert
        var item =
            Assert.Single(result.Items);

        Assert.Equal(body, item.BodyExcerpt);
        Assert.Equal(120, item.BodyExcerpt.Length);
    }

    [Fact]
    public async Task SearchAsync_LongBodyWithoutKeyword_ReturnsBeginningWithTrailingEllipsis()
    {
        // Arrange
        var body =
            new string('あ', 200);

        var faq =
            CreateFaq(
                id: 1,
                title: "長い本文",
                body: body);

        SetupPublishedFaqs([faq]);

        // Act
        var result =
            await _sut.SearchAsync(
                CreateCondition());

        // Assert
        var item =
            Assert.Single(result.Items);

        Assert.Equal(
            new string('あ', 120) + "…",
            item.BodyExcerpt);
    }

    [Fact]
    public async Task SearchAsync_KeywordIsInMiddleOfLongBody_CreatesExcerptAroundKeyword()
    {
        // Arrange
        const string keyword =
            "パスワード";

        var body =
            new string('前', 100) +
            keyword +
            new string('後', 100);

        var faq =
            CreateFaq(
                id: 1,
                title: "本文検索",
                body: body);

        SetupPublishedFaqs([faq]);

        var condition =
            CreateCondition(
                keyword: keyword);

        // Act
        var result =
            await _sut.SearchAsync(condition);

        // Assert
        var item =
            Assert.Single(result.Items);

        Assert.StartsWith("…", item.BodyExcerpt);
        Assert.EndsWith("…", item.BodyExcerpt);
        Assert.Contains(keyword, item.BodyExcerpt);
    }

    // =========================================================
    // SearchAsync：タグ表示順
    // =========================================================

    [Fact]
    public async Task SearchAsync_FaqHasTags_ReturnsTagNamesOrderedByDisplayOrderThenId()
    {
        // Arrange
        var tags = new[]
        {
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
        };

        var faq =
            CreateFaq(
                id: 1,
                title: "タグ付きFAQ",
                tags: tags);

        SetupPublishedFaqs([faq]);

        // Act
        var result =
            await _sut.SearchAsync(
                CreateCondition());

        // Assert
        var item =
            Assert.Single(result.Items);

        Assert.Equal(
            new[]
            {
                "タグ10",
                "タグ20",
                "タグ30"
            },
            item.TagNames);
    }

    // =========================================================
    // GetDetailAsync
    // =========================================================

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetDetailAsync_FaqIdIsZeroOrNegative_ReturnsNullWithoutCallingRepository(
        int faqId)
    {
        // Act
        var result =
            await _sut.GetDetailAsync(faqId);

        // Assert
        Assert.Null(result);

        _faqRepositoryMock.Verify(
            x => x.GetPublishedByIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _faqRepositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetDetailAsync_FaqDoesNotExist_ReturnsNullWithoutSaving()
    {
        // Arrange
        const int faqId = 100;

        var cancellationToken =
            CancellationToken.None;

        _faqRepositoryMock
            .Setup(x => x.GetPublishedByIdAsync(
                faqId,
                cancellationToken))
            .ReturnsAsync((Faq?)null);

        // Act
        var result =
            await _sut.GetDetailAsync(
                faqId,
                cancellationToken);

        // Assert
        Assert.Null(result);

        _faqRepositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetDetailAsync_FaqExists_IncrementsViewCountSavesAndReturnsDetail()
    {
        // Arrange
        const int faqId = 100;

        var cancellationToken =
            CancellationToken.None;

        var createdAt =
            new DateTime(
                2026,
                7,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

        var updatedAt =
            new DateTime(
                2026,
                7,
                20,
                0,
                0,
                0,
                DateTimeKind.Utc);

        var faq =
            CreateFaq(
                id: faqId,
                title: "パスワード変更方法",
                body: "設定画面から変更できます。",
                categoryId: 5,
                categoryName: "アカウント",
                viewCount: 10,
                createdAt: createdAt,
                updatedAt: updatedAt,
                tags:
                [
                    CreateTag(
                        id: 20,
                        name: "ログイン",
                        displayOrder: 2),

                    CreateTag(
                        id: 10,
                        name: "パスワード",
                        displayOrder: 1)
                ]);

        _faqRepositoryMock
            .Setup(x => x.GetPublishedByIdAsync(
                faqId,
                cancellationToken))
            .ReturnsAsync(faq);

        _faqRepositoryMock
            .Setup(x => x.SaveChangesAsync(
                cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _sut.GetDetailAsync(
                faqId,
                cancellationToken);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(faqId, result.Id);
        Assert.Equal(
            "パスワード変更方法",
            result.Title);
        Assert.Equal(
            "設定画面から変更できます。",
            result.Body);
        Assert.Equal(5, result.CategoryId);
        Assert.Equal(
            "アカウント",
            result.CategoryName);
        Assert.Equal(11, result.ViewCount);
        Assert.Equal(createdAt, result.CreatedAt);
        Assert.Equal(updatedAt, result.UpdatedAt);

        Assert.Equal(
            new[]
            {
                "パスワード",
                "ログイン"
            },
            result.TagNames);

        Assert.Equal(11, faq.ViewCount);

        _faqRepositoryMock.Verify(
            x => x.GetPublishedByIdAsync(
                faqId,
                cancellationToken),
            Times.Once);

        _faqRepositoryMock.Verify(
            x => x.SaveChangesAsync(
                cancellationToken),
            Times.Once);
    }

    // =========================================================
    // Test helpers
    // =========================================================

    private void SetupPublishedFaqs(
        IReadOnlyList<Faq> faqs,
        CancellationToken cancellationToken = default)
    {
        _faqRepositoryMock
            .Setup(x => x.GetPublishedAsync(
                cancellationToken))
            .ReturnsAsync(faqs);
    }

    private static PublicFaqSearchCondition CreateCondition(
        string? keyword = null,
        int? categoryId = null,
        int? tagId = null,
        FaqSortOrder sortOrder = FaqSortOrder.Relevance,
        int page = 1,
        int pageSize = 10)
    {
        return new PublicFaqSearchCondition
        {
            Keyword = keyword,
            CategoryId = categoryId,
            TagId = tagId,
            SortOrder = sortOrder,
            Page = page,
            PageSize = pageSize
        };
    }

    private static Faq CreateFaq(
        int id,
        string title,
        string body = "テスト用FAQ本文です。",
        int categoryId = 1,
        string categoryName = "テストカテゴリ",
        int viewCount = 0,
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        IReadOnlyCollection<Tag>? tags = null)
    {
        var faq =
            new Faq(
                title,
                body,
                categoryId,
                isPublished: true);

        var category =
            CreateEntity<Category>();

        SetMember(
            category,
            nameof(Category.Id),
            categoryId);

        SetMember(
            category,
            nameof(Category.Name),
            categoryName);

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
        // 固定日時を設定する前に実行する
        faq.ReplaceTags(
            tags ?? Array.Empty<Tag>());

        SetMember(
            faq,
            nameof(Faq.CreatedAt),
            createdAt ??
            new DateTime(
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
            updatedAt ??
            new DateTime(
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
            CreateEntity<Tag>();

        SetMember(
            tag,
            nameof(Tag.Id),
            id);

        SetMember(
            tag,
            nameof(Tag.Name),
            name);

        SetMember(
            tag,
            nameof(Tag.DisplayOrder),
            displayOrder);

        return tag;
    }

    private static T CreateEntity<T>()
        where T : class
    {
        return (T)RuntimeHelpers.GetUninitializedObject(
            typeof(T));
    }

    private static void SetMember<TValue>(
        object target,
        string propertyName,
        TValue value)
    {
        const BindingFlags bindingFlags =
            BindingFlags.Instance |
            BindingFlags.Public |
            BindingFlags.NonPublic;

        var type =
            target.GetType();

        var property =
            type.GetProperty(
                propertyName,
                bindingFlags);

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
                bindingFlags);

        if (backingField is not null)
        {
            backingField.SetValue(
                target,
                value);

            return;
        }

        throw new InvalidOperationException(
            $"{type.Name}.{propertyName} を設定できません。");
    }
}