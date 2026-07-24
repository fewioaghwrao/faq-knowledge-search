using System.Reflection;
using FaqKnowledgeSearch.Application.Faqs.Admin;
using FaqKnowledgeSearch.Domain.Faqs;
using Moq;
using Xunit;

namespace FaqKnowledgeSearch.Application.Tests.Faqs.Admin;

public sealed class AdminFaqServiceTests
{
    private readonly Mock<IAdminFaqRepository> _repositoryMock;
    private readonly AdminFaqService _sut;

    public AdminFaqServiceTests()
    {
        _repositoryMock =
            new Mock<IAdminFaqRepository>(
                MockBehavior.Strict);

        _sut =
            new AdminFaqService(
                _repositoryMock.Object);
    }

    // =========================================================
    // GetFormOptionsAsync
    // =========================================================

    [Fact]
    public async Task GetFormOptionsAsync_CategoriesAndTagsExist_ReturnsMappedOptions()
    {
        // Arrange
        var cancellationToken =
            CancellationToken.None;

        IReadOnlyList<Category> categories =
        [
            CreateCategory(
                id: 1,
                name: "アカウント"),

            CreateCategory(
                id: 2,
                name: "システム")
        ];

        IReadOnlyList<Tag> tags =
        [
            CreateTag(
                id: 10,
                name: "パスワード",
                displayOrder: 1),

            CreateTag(
                id: 20,
                name: "ログイン",
                displayOrder: 2)
        ];

        _repositoryMock
            .Setup(x => x.GetCategoriesAsync(
                cancellationToken))
            .ReturnsAsync(categories);

        _repositoryMock
            .Setup(x => x.GetTagsAsync(
                cancellationToken))
            .ReturnsAsync(tags);

        // Act
        var result =
            await _sut.GetFormOptionsAsync(
                cancellationToken);

        // Assert
        Assert.Collection(
            result.Categories,
            category =>
            {
                Assert.Equal(1, category.Id);
                Assert.Equal(
                    "アカウント",
                    category.Name);
            },
            category =>
            {
                Assert.Equal(2, category.Id);
                Assert.Equal(
                    "システム",
                    category.Name);
            });

        Assert.Collection(
            result.Tags,
            tag =>
            {
                Assert.Equal(10, tag.Id);
                Assert.Equal(
                    "パスワード",
                    tag.Name);
            },
            tag =>
            {
                Assert.Equal(20, tag.Id);
                Assert.Equal(
                    "ログイン",
                    tag.Name);
            });

        _repositoryMock.Verify(
            x => x.GetCategoriesAsync(
                cancellationToken),
            Times.Once);

        _repositoryMock.Verify(
            x => x.GetTagsAsync(
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetFormOptionsAsync_NoCategoriesOrTags_ReturnsEmptyOptions()
    {
        // Arrange
        var cancellationToken =
            CancellationToken.None;

        _repositoryMock
            .Setup(x => x.GetCategoriesAsync(
                cancellationToken))
            .ReturnsAsync(
                Array.Empty<Category>());

        _repositoryMock
            .Setup(x => x.GetTagsAsync(
                cancellationToken))
            .ReturnsAsync(
                Array.Empty<Tag>());

        // Act
        var result =
            await _sut.GetFormOptionsAsync(
                cancellationToken);

        // Assert
        Assert.Empty(result.Categories);
        Assert.Empty(result.Tags);
    }

    // =========================================================
    // GetEditDataAsync
    // =========================================================

    [Fact]
    public async Task GetEditDataAsync_FaqDoesNotExist_ReturnsNull()
    {
        // Arrange
        const int faqId = 100;

        var cancellationToken =
            CancellationToken.None;

        _repositoryMock
            .Setup(x => x.GetByIdForUpdateAsync(
                faqId,
                cancellationToken))
            .ReturnsAsync((Faq?)null);

        // Act
        var result =
            await _sut.GetEditDataAsync(
                faqId,
                cancellationToken);

        // Assert
        Assert.Null(result);

        _repositoryMock.Verify(
            x => x.GetByIdForUpdateAsync(
                faqId,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetEditDataAsync_FaqExists_ReturnsMappedDataWithOrderedTagIds()
    {
        // Arrange
        const int faqId = 100;

        var cancellationToken =
            CancellationToken.None;

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
                id: faqId,
                title: "パスワード変更方法",
                body: "設定画面から変更できます。",
                categoryId: 5,
                isPublished: true,
                tags: tags);

        _repositoryMock
            .Setup(x => x.GetByIdForUpdateAsync(
                faqId,
                cancellationToken))
            .ReturnsAsync(faq);

        // Act
        var result =
            await _sut.GetEditDataAsync(
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
        Assert.True(result.IsPublished);

        Assert.Equal(
            [10, 20, 30],
            result.TagIds);
    }

    // =========================================================
    // CreateAsync
    // =========================================================

    [Fact]
    public async Task CreateAsync_ValidCommand_AddsFaqAndSavesChanges()
    {
        // Arrange
        const int createdFaqId = 123;

        var cancellationToken =
            CancellationToken.None;

        var command =
            CreateCommand(
                title: "FAQタイトル",
                body: "FAQ本文",
                categoryId: 1,
                tagIds: [10, 20],
                isPublished: true);

        IReadOnlyList<Tag> tags =
        [
            CreateTag(
                id: 10,
                name: "ログイン",
                displayOrder: 1),

            CreateTag(
                id: 20,
                name: "パスワード",
                displayOrder: 2)
        ];

        Faq? addedFaq = null;

        _repositoryMock
            .Setup(x => x.CategoryExistsAsync(
                command.CategoryId,
                cancellationToken))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(x => x.GetTagsByIdsAsync(
                It.Is<IReadOnlyCollection<int>>(
                    ids => ids.SequenceEqual(
                        new[] { 10, 20 })),
                cancellationToken))
            .ReturnsAsync(tags);

        _repositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<Faq>(),
                cancellationToken))
            .Callback<Faq, CancellationToken>(
                (faq, _) =>
                {
                    addedFaq = faq;

                    SetProperty(
                        faq,
                        nameof(Faq.Id),
                        createdFaqId);
                })
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync(
                cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _sut.CreateAsync(
                command,
                cancellationToken);

        // Assert
        Assert.Equal(createdFaqId, result);

        Assert.NotNull(addedFaq);
        Assert.Equal(
            command.Title,
            addedFaq.Title);
        Assert.Equal(
            command.Body,
            addedFaq.Body);
        Assert.Equal(
            command.CategoryId,
            addedFaq.CategoryId);
        Assert.True(addedFaq.IsPublished);

        Assert.Equal(
            [10, 20],
            addedFaq.Tags
                .Select(tag => tag.Id)
                .OrderBy(id => id)
                .ToArray());

        _repositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Faq>(),
                cancellationToken),
            Times.Once);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_NoTagIds_CreatesFaqWithoutLoadingTags()
    {
        // Arrange
        var cancellationToken =
            CancellationToken.None;

        var command =
            CreateCommand(
                title: "タグなしFAQ",
                body: "タグなしFAQの本文",
                categoryId: 1,
                tagIds: [],
                isPublished: false);

        Faq? addedFaq = null;

        _repositoryMock
            .Setup(x => x.CategoryExistsAsync(
                command.CategoryId,
                cancellationToken))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<Faq>(),
                cancellationToken))
            .Callback<Faq, CancellationToken>(
                (faq, _) => addedFaq = faq)
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync(
                cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.CreateAsync(
            command,
            cancellationToken);

        // Assert
        Assert.NotNull(addedFaq);
        Assert.Empty(addedFaq.Tags);
        Assert.False(addedFaq.IsPublished);

        _repositoryMock.Verify(
            x => x.GetTagsByIdsAsync(
                It.IsAny<IReadOnlyCollection<int>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_TagIdsContainInvalidValuesAndDuplicates_NormalizesTagIds()
    {
        // Arrange
        var cancellationToken =
            CancellationToken.None;

        var command =
            CreateCommand(
                title: "FAQタイトル",
                body: "FAQ本文",
                categoryId: 1,
                tagIds: [0, -1, 10, 10, 20],
                isPublished: true);

        IReadOnlyList<Tag> tags =
        [
            CreateTag(
                id: 10,
                name: "タグ10",
                displayOrder: 1),

            CreateTag(
                id: 20,
                name: "タグ20",
                displayOrder: 2)
        ];

        _repositoryMock
            .Setup(x => x.CategoryExistsAsync(
                command.CategoryId,
                cancellationToken))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(x => x.GetTagsByIdsAsync(
                It.Is<IReadOnlyCollection<int>>(
                    ids => ids.SequenceEqual(
                        new[] { 10, 20 })),
                cancellationToken))
            .ReturnsAsync(tags);

        _repositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<Faq>(),
                cancellationToken))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync(
                cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        await _sut.CreateAsync(
            command,
            cancellationToken);

        // Assert
        _repositoryMock.Verify(
            x => x.GetTagsByIdsAsync(
                It.Is<IReadOnlyCollection<int>>(
                    ids =>
                        ids.Count == 2 &&
                        ids.Contains(10) &&
                        ids.Contains(20)),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_CategoryDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        var cancellationToken =
            CancellationToken.None;

        var command =
            CreateCommand(
                title: "FAQタイトル",
                body: "FAQ本文",
                categoryId: 999,
                tagIds: [10],
                isPublished: true);

        _repositoryMock
            .Setup(x => x.CategoryExistsAsync(
                command.CategoryId,
                cancellationToken))
            .ReturnsAsync(false);

        // Act
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.CreateAsync(
                    command,
                    cancellationToken));

        // Assert
        Assert.Equal(
            "選択されたカテゴリは存在しません。",
            exception.Message);

        _repositoryMock.Verify(
            x => x.GetTagsByIdsAsync(
                It.IsAny<IReadOnlyCollection<int>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Faq>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_TagDoesNotExist_ThrowsArgumentExceptionContainingMissingId()
    {
        // Arrange
        var cancellationToken =
            CancellationToken.None;

        var command =
            CreateCommand(
                title: "FAQタイトル",
                body: "FAQ本文",
                categoryId: 1,
                tagIds: [10, 999],
                isPublished: true);

        IReadOnlyList<Tag> existingTags =
        [
            CreateTag(
                id: 10,
                name: "存在するタグ",
                displayOrder: 1)
        ];

        _repositoryMock
            .Setup(x => x.CategoryExistsAsync(
                command.CategoryId,
                cancellationToken))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(x => x.GetTagsByIdsAsync(
                It.IsAny<IReadOnlyCollection<int>>(),
                cancellationToken))
            .ReturnsAsync(existingTags);

        // Act
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.CreateAsync(
                    command,
                    cancellationToken));

        // Assert
        Assert.Contains(
            "存在しないタグIDが指定されています",
            exception.Message);

        Assert.Contains(
            "999",
            exception.Message);

        _repositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Faq>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // =========================================================
    // UpdateAsync
    // =========================================================

    [Fact]
    public async Task UpdateAsync_FaqDoesNotExist_ReturnsFalse()
    {
        // Arrange
        const int faqId = 100;

        var cancellationToken =
            CancellationToken.None;

        var command =
            CreateCommand(
                title: "変更後タイトル",
                body: "変更後本文",
                categoryId: 2,
                tagIds: [10],
                isPublished: true);

        _repositoryMock
            .Setup(x => x.GetByIdForUpdateAsync(
                faqId,
                cancellationToken))
            .ReturnsAsync((Faq?)null);

        // Act
        var result =
            await _sut.UpdateAsync(
                faqId,
                command,
                cancellationToken);

        // Assert
        Assert.False(result);

        _repositoryMock.Verify(
            x => x.CategoryExistsAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateAsync_ValidCommand_UpdatesFaqAndPublicationStatus(
        bool isPublished)
    {
        // Arrange
        const int faqId = 100;

        var cancellationToken =
            CancellationToken.None;

        var existingFaq =
            CreateFaq(
                id: faqId,
                title: "変更前タイトル",
                body: "変更前本文",
                categoryId: 1,
                isPublished: !isPublished,
                tags:
                [
                    CreateTag(
                        id: 1,
                        name: "変更前タグ",
                        displayOrder: 1)
                ]);

        var command =
            CreateCommand(
                title: "変更後タイトル",
                body: "変更後本文",
                categoryId: 2,
                tagIds: [10, 20],
                isPublished: isPublished);

        IReadOnlyList<Tag> newTags =
        [
            CreateTag(
                id: 10,
                name: "タグ10",
                displayOrder: 1),

            CreateTag(
                id: 20,
                name: "タグ20",
                displayOrder: 2)
        ];

        _repositoryMock
            .Setup(x => x.GetByIdForUpdateAsync(
                faqId,
                cancellationToken))
            .ReturnsAsync(existingFaq);

        _repositoryMock
            .Setup(x => x.CategoryExistsAsync(
                command.CategoryId,
                cancellationToken))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(x => x.GetTagsByIdsAsync(
                It.Is<IReadOnlyCollection<int>>(
                    ids => ids.SequenceEqual(
                        new[] { 10, 20 })),
                cancellationToken))
            .ReturnsAsync(newTags);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync(
                cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _sut.UpdateAsync(
                faqId,
                command,
                cancellationToken);

        // Assert
        Assert.True(result);

        Assert.Equal(
            "変更後タイトル",
            existingFaq.Title);
        Assert.Equal(
            "変更後本文",
            existingFaq.Body);
        Assert.Equal(
            2,
            existingFaq.CategoryId);
        Assert.Equal(
            isPublished,
            existingFaq.IsPublished);

        Assert.Equal(
            [10, 20],
            existingFaq.Tags
                .Select(tag => tag.Id)
                .OrderBy(id => id)
                .ToArray());

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(
                cancellationToken),
            Times.Once);

        _repositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<Faq>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_CategoryDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        const int faqId = 100;

        var cancellationToken =
            CancellationToken.None;

        var faq =
            CreateFaq(
                id: faqId,
                title: "変更前タイトル",
                body: "変更前本文",
                categoryId: 1,
                isPublished: false);

        var command =
            CreateCommand(
                title: "変更後タイトル",
                body: "変更後本文",
                categoryId: 999,
                tagIds: [10],
                isPublished: true);

        _repositoryMock
            .Setup(x => x.GetByIdForUpdateAsync(
                faqId,
                cancellationToken))
            .ReturnsAsync(faq);

        _repositoryMock
            .Setup(x => x.CategoryExistsAsync(
                command.CategoryId,
                cancellationToken))
            .ReturnsAsync(false);

        // Act
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.UpdateAsync(
                    faqId,
                    command,
                    cancellationToken));

        // Assert
        Assert.Equal(
            "選択されたカテゴリは存在しません。",
            exception.Message);

        Assert.Equal(
            "変更前タイトル",
            faq.Title);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_TagDoesNotExist_ThrowsArgumentException()
    {
        // Arrange
        const int faqId = 100;

        var cancellationToken =
            CancellationToken.None;

        var faq =
            CreateFaq(
                id: faqId,
                title: "変更前タイトル",
                body: "変更前本文",
                categoryId: 1,
                isPublished: false);

        var command =
            CreateCommand(
                title: "変更後タイトル",
                body: "変更後本文",
                categoryId: 2,
                tagIds: [10, 999],
                isPublished: true);

        IReadOnlyList<Tag> existingTags =
        [
            CreateTag(
                id: 10,
                name: "存在するタグ",
                displayOrder: 1)
        ];

        _repositoryMock
            .Setup(x => x.GetByIdForUpdateAsync(
                faqId,
                cancellationToken))
            .ReturnsAsync(faq);

        _repositoryMock
            .Setup(x => x.CategoryExistsAsync(
                command.CategoryId,
                cancellationToken))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(x => x.GetTagsByIdsAsync(
                It.IsAny<IReadOnlyCollection<int>>(),
                cancellationToken))
            .ReturnsAsync(existingTags);

        // Act
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () => _sut.UpdateAsync(
                    faqId,
                    command,
                    cancellationToken));

        // Assert
        Assert.Contains(
            "999",
            exception.Message);

        Assert.Equal(
            "変更前タイトル",
            faq.Title);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // =========================================================
    // DeleteAsync
    // =========================================================

    [Fact]
    public async Task DeleteAsync_FaqDoesNotExist_ReturnsFalseWithoutSaving()
    {
        // Arrange
        const int faqId = 100;

        var cancellationToken =
            CancellationToken.None;

        _repositoryMock
            .Setup(x => x.GetByIdForUpdateAsync(
                faqId,
                cancellationToken))
            .ReturnsAsync((Faq?)null);

        // Act
        var result =
            await _sut.DeleteAsync(
                faqId,
                cancellationToken);

        // Assert
        Assert.False(result);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_FaqExists_DeletesFaqAndSavesChanges()
    {
        // Arrange
        const int faqId = 100;

        var cancellationToken =
            CancellationToken.None;

        var faq =
            CreateFaq(
                id: faqId,
                title: "削除対象FAQ",
                body: "削除対象FAQの本文",
                categoryId: 1,
                isPublished: true);

        _repositoryMock
            .Setup(x => x.GetByIdForUpdateAsync(
                faqId,
                cancellationToken))
            .ReturnsAsync(faq);

        _repositoryMock
            .Setup(x => x.SaveChangesAsync(
                cancellationToken))
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _sut.DeleteAsync(
                faqId,
                cancellationToken);

        // Assert
        Assert.True(result);
        Assert.True(faq.IsDeleted);
        Assert.NotNull(faq.DeletedAt);

        _repositoryMock.Verify(
            x => x.SaveChangesAsync(
                cancellationToken),
            Times.Once);
    }

    // =========================================================
    // Test helpers
    // =========================================================

    private static AdminFaqCommand CreateCommand(
        string title,
        string body,
        int categoryId,
        IReadOnlyCollection<int> tagIds,
        bool isPublished)
    {
        return new AdminFaqCommand(
            Title: title,
            Body: body,
            CategoryId: categoryId,
            TagIds: tagIds,
            IsPublished: isPublished);
    }

    private static Faq CreateFaq(
        int id,
        string title,
        string body,
        int categoryId,
        bool isPublished,
        IReadOnlyCollection<Tag>? tags = null)
    {
        var faq =
            new Faq(
                title,
                body,
                categoryId,
                isPublished);

        SetProperty(
            faq,
            nameof(Faq.Id),
            id);

        if (tags is not null)
        {
            faq.ReplaceTags(tags);
        }

        return faq;
    }

    private static Category CreateCategory(
        int id,
        string name)
    {
        var category =
            CreateEntity<Category>();

        SetProperty(
            category,
            nameof(Category.Id),
            id);

        SetProperty(
            category,
            nameof(Category.Name),
            name);

        return category;
    }

    private static Tag CreateTag(
        int id,
        string name,
        int displayOrder)
    {
        var tag =
            CreateEntity<Tag>();

        SetProperty(
            tag,
            nameof(Tag.Id),
            id);

        SetProperty(
            tag,
            nameof(Tag.Name),
            name);

        SetProperty(
            tag,
            nameof(Tag.DisplayOrder),
            displayOrder);

        return tag;
    }

    private static T CreateEntity<T>()
        where T : class
    {
        var instance =
            Activator.CreateInstance(
                typeof(T),
                nonPublic: true)
            as T;

        Assert.NotNull(instance);

        return instance;
    }

    private static void SetProperty<TValue>(
        object target,
        string propertyName,
        TValue value)
    {
        var property =
            target.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);

        Assert.NotNull(property);

        property.SetValue(
            target,
            value);
    }
}