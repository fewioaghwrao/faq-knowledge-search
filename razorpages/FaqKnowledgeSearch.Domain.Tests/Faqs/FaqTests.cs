using System.Reflection;
using System.Runtime.CompilerServices;
using FaqKnowledgeSearch.Domain.Faqs;

namespace FaqKnowledgeSearch.Domain.Tests.Faqs;

public sealed class FaqTests
{
    // =========================================================
    // Constructor
    // =========================================================

    [Fact]
    public void Constructor_ValidValues_CreatesFaq()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var faq =
            new Faq(
                "  パスワード変更方法  ",
                "  設定画面から変更できます。  ",
                categoryId: 10);

        var after = DateTime.UtcNow;

        // Assert
        Assert.Equal(
            "パスワード変更方法",
            faq.Title);

        Assert.Equal(
            "設定画面から変更できます。",
            faq.Body);

        Assert.Equal(10, faq.CategoryId);
        Assert.False(faq.IsPublished);
        Assert.Equal(0, faq.ViewCount);
        Assert.Empty(faq.Tags);
        Assert.False(faq.IsDeleted);
        Assert.Null(faq.DeletedAt);

        Assert.InRange(
            faq.CreatedAt,
            before,
            after);

        Assert.Equal(
            faq.CreatedAt,
            faq.UpdatedAt);
    }

    [Fact]
    public void Constructor_IsPublishedIsTrue_CreatesPublishedFaq()
    {
        // Act
        var faq =
            new Faq(
                "FAQタイトル",
                "FAQ本文",
                categoryId: 1,
                isPublished: true);

        // Assert
        Assert.True(faq.IsPublished);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public void Constructor_TitleIsWhiteSpace_ThrowsArgumentException(
        string title)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new Faq(
                    title,
                    "FAQ本文",
                    categoryId: 1));

        // Assert
        Assert.Equal(
            "title",
            exception.ParamName);

        Assert.Contains(
            "FAQタイトルは必須です。",
            exception.Message);
    }

    [Fact]
    public void Constructor_TitleIsExactly100Characters_CreatesFaq()
    {
        // Arrange
        var title =
            new string('あ', 100);

        // Act
        var faq =
            new Faq(
                title,
                "FAQ本文",
                categoryId: 1);

        // Assert
        Assert.Equal(100, faq.Title.Length);
    }

    [Fact]
    public void Constructor_TrimmedTitleExceeds100Characters_ThrowsArgumentException()
    {
        // Arrange
        var title =
            $"  {new string('あ', 101)}  ";

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new Faq(
                    title,
                    "FAQ本文",
                    categoryId: 1));

        // Assert
        Assert.Equal(
            "title",
            exception.ParamName);

        Assert.Contains(
            "FAQタイトルは100文字以内で入力してください。",
            exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public void Constructor_BodyIsWhiteSpace_ThrowsArgumentException(
        string body)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new Faq(
                    "FAQタイトル",
                    body,
                    categoryId: 1));

        // Assert
        Assert.Equal(
            "body",
            exception.ParamName);

        Assert.Contains(
            "FAQ本文は必須です。",
            exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_CategoryIdIsNotPositive_ThrowsArgumentOutOfRangeException(
        int categoryId)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Faq(
                    "FAQタイトル",
                    "FAQ本文",
                    categoryId));

        // Assert
        Assert.Equal(
            "categoryId",
            exception.ParamName);

        Assert.Contains(
            "有効なカテゴリを指定してください。",
            exception.Message);
    }

    // =========================================================
    // UpdateContent
    // =========================================================

    [Fact]
    public void UpdateContent_ValidValues_UpdatesAndTrimsContent()
    {
        // Arrange
        var faq =
            CreateFaq();

        var originalCreatedAt =
            faq.CreatedAt;

        var oldUpdatedAt =
            new DateTime(
                2026,
                1,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

        SetMember(
            faq,
            nameof(Faq.UpdatedAt),
            oldUpdatedAt);

        // Act
        faq.UpdateContent(
            "  変更後タイトル  ",
            "  変更後本文  ",
            categoryId: 20);

        // Assert
        Assert.Equal(
            "変更後タイトル",
            faq.Title);

        Assert.Equal(
            "変更後本文",
            faq.Body);

        Assert.Equal(20, faq.CategoryId);

        Assert.Equal(
            originalCreatedAt,
            faq.CreatedAt);

        Assert.True(
            faq.UpdatedAt > oldUpdatedAt);
    }

    [Fact]
    public void UpdateContent_InvalidValues_DoesNotChangeExistingState()
    {
        // Arrange
        var faq =
            CreateFaq(
                title: "変更前タイトル",
                body: "変更前本文",
                categoryId: 1);

        var oldUpdatedAt =
            faq.UpdatedAt;

        // Act
        Assert.Throws<ArgumentOutOfRangeException>(
            () => faq.UpdateContent(
                "変更後タイトル",
                "変更後本文",
                categoryId: 0));

        // Assert
        Assert.Equal(
            "変更前タイトル",
            faq.Title);

        Assert.Equal(
            "変更前本文",
            faq.Body);

        Assert.Equal(1, faq.CategoryId);
        Assert.Equal(oldUpdatedAt, faq.UpdatedAt);
    }

    // =========================================================
    // ReplaceTags
    // =========================================================

    [Fact]
    public void ReplaceTags_ValidTags_ReplacesExistingTags()
    {
        // Arrange
        var faq = CreateFaq();

        faq.ReplaceTags(
        [
            CreateTag(
                id: 1,
                name: "変更前タグ")
        ]);

        var oldUpdatedAt =
            new DateTime(
                2026,
                1,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

        SetMember(
            faq,
            nameof(Faq.UpdatedAt),
            oldUpdatedAt);

        var newTags = new[]
        {
            CreateTag(
                id: 10,
                name: "ログイン"),

            CreateTag(
                id: 20,
                name: "パスワード")
        };

        // Act
        faq.ReplaceTags(newTags);

        // Assert
        Assert.Collection(
            faq.Tags,
            tag =>
            {
                Assert.Equal(10, tag.Id);
                Assert.Equal("ログイン", tag.Name);
            },
            tag =>
            {
                Assert.Equal(20, tag.Id);
                Assert.Equal("パスワード", tag.Name);
            });

        Assert.True(
            faq.UpdatedAt > oldUpdatedAt);
    }

    [Fact]
    public void ReplaceTags_EmptyCollection_RemovesAllTags()
    {
        // Arrange
        var faq = CreateFaq();

        faq.ReplaceTags(
        [
            CreateTag(
                id: 10,
                name: "ログイン")
        ]);

        // Act
        faq.ReplaceTags([]);

        // Assert
        Assert.Empty(faq.Tags);
    }

    [Fact]
    public void ReplaceTags_PersistedTagsHaveSameId_DoesNotAddDuplicate()
    {
        // Arrange
        var faq = CreateFaq();

        var firstTag =
            CreateTag(
                id: 10,
                name: "最初の名前");

        var duplicateTag =
            CreateTag(
                id: 10,
                name: "異なる名前");

        // Act
        faq.ReplaceTags(
        [
            firstTag,
            duplicateTag
        ]);

        // Assert
        var tag =
            Assert.Single(faq.Tags);

        Assert.Same(firstTag, tag);
        Assert.Equal(
            "最初の名前",
            tag.Name);
    }

    [Fact]
    public void ReplaceTags_PersistedTagsHaveDifferentIdsAndSameName_AddsBoth()
    {
        // Arrange
        var faq = CreateFaq();

        // Act
        faq.ReplaceTags(
        [
            CreateTag(
                id: 10,
                name: "ログイン"),

            CreateTag(
                id: 20,
                name: "ログイン")
        ]);

        // Assert
        Assert.Equal(2, faq.Tags.Count);
    }

    [Fact]
    public void ReplaceTags_UnpersistedTagsHaveSameNameIgnoringCase_DoesNotAddDuplicate()
    {
        // Arrange
        var faq = CreateFaq();

        var firstTag =
            CreateTag(
                id: 0,
                name: "Login");

        var duplicateTag =
            CreateTag(
                id: 0,
                name: "login");

        // Act
        faq.ReplaceTags(
        [
            firstTag,
            duplicateTag
        ]);

        // Assert
        var tag =
            Assert.Single(faq.Tags);

        Assert.Same(firstTag, tag);
    }

    [Fact]
    public void ReplaceTags_TagsIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var faq = CreateFaq();

        // Act
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => faq.ReplaceTags(null!));

        // Assert
        Assert.Equal(
            "tags",
            exception.ParamName);
    }

    [Fact]
    public void ReplaceTags_ContainsNullTag_ThrowsArgumentNullException()
    {
        // Arrange
        var faq = CreateFaq();

        Tag[] tags =
        [
            CreateTag(
                id: 10,
                name: "ログイン"),

            null!
        ];

        // Act
        Assert.Throws<ArgumentNullException>(
            () => faq.ReplaceTags(tags));
    }

    [Fact]
    public void Tags_ReturnedCollectionCannotBeModifiedDirectly()
    {
        // Arrange
        var faq = CreateFaq();

        faq.ReplaceTags(
        [
            CreateTag(
                id: 10,
                name: "ログイン")
        ]);

        var collection =
            Assert.IsAssignableFrom<ICollection<Tag>>(
                faq.Tags);

        // Act・Assert
        Assert.Throws<NotSupportedException>(
            () => collection.Add(
                CreateTag(
                    id: 20,
                    name: "パスワード")));
    }

    // =========================================================
    // Publish
    // =========================================================

    [Fact]
    public void Publish_UnpublishedFaq_PublishesAndUpdatesTimestamp()
    {
        // Arrange
        var faq =
            CreateFaq(
                isPublished: false);

        var oldUpdatedAt =
            new DateTime(
                2026,
                1,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

        SetMember(
            faq,
            nameof(Faq.UpdatedAt),
            oldUpdatedAt);

        // Act
        faq.Publish();

        // Assert
        Assert.True(faq.IsPublished);
        Assert.True(
            faq.UpdatedAt > oldUpdatedAt);
    }

    [Fact]
    public void Publish_AlreadyPublished_DoesNotUpdateTimestamp()
    {
        // Arrange
        var faq =
            CreateFaq(
                isPublished: true);

        var oldUpdatedAt =
            new DateTime(
                2026,
                1,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

        SetMember(
            faq,
            nameof(Faq.UpdatedAt),
            oldUpdatedAt);

        // Act
        faq.Publish();

        // Assert
        Assert.True(faq.IsPublished);
        Assert.Equal(
            oldUpdatedAt,
            faq.UpdatedAt);
    }

    // =========================================================
    // Unpublish
    // =========================================================

    [Fact]
    public void Unpublish_PublishedFaq_UnpublishesAndUpdatesTimestamp()
    {
        // Arrange
        var faq =
            CreateFaq(
                isPublished: true);

        var oldUpdatedAt =
            new DateTime(
                2026,
                1,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

        SetMember(
            faq,
            nameof(Faq.UpdatedAt),
            oldUpdatedAt);

        // Act
        faq.Unpublish();

        // Assert
        Assert.False(faq.IsPublished);
        Assert.True(
            faq.UpdatedAt > oldUpdatedAt);
    }

    [Fact]
    public void Unpublish_AlreadyUnpublished_DoesNotUpdateTimestamp()
    {
        // Arrange
        var faq =
            CreateFaq(
                isPublished: false);

        var oldUpdatedAt =
            new DateTime(
                2026,
                1,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

        SetMember(
            faq,
            nameof(Faq.UpdatedAt),
            oldUpdatedAt);

        // Act
        faq.Unpublish();

        // Assert
        Assert.False(faq.IsPublished);
        Assert.Equal(
            oldUpdatedAt,
            faq.UpdatedAt);
    }

    // =========================================================
    // IncrementViewCount
    // =========================================================

    [Fact]
    public void IncrementViewCount_ValidFaq_IncrementsWithoutUpdatingTimestamp()
    {
        // Arrange
        var faq = CreateFaq();

        SetMember(
            faq,
            nameof(Faq.ViewCount),
            10);

        var oldUpdatedAt =
            new DateTime(
                2026,
                1,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

        SetMember(
            faq,
            nameof(Faq.UpdatedAt),
            oldUpdatedAt);

        // Act
        faq.IncrementViewCount();

        // Assert
        Assert.Equal(11, faq.ViewCount);

        Assert.Equal(
            oldUpdatedAt,
            faq.UpdatedAt);
    }

    // =========================================================
    // Delete
    // =========================================================

    [Fact]
    public void Delete_ExistingFaq_DeletesAndUnpublishesFaq()
    {
        // Arrange
        var faq =
            CreateFaq(
                isPublished: true);

        var before =
            DateTime.UtcNow;

        // Act
        faq.Delete();

        var after =
            DateTime.UtcNow;

        // Assert
        Assert.True(faq.IsDeleted);
        Assert.False(faq.IsPublished);
        Assert.NotNull(faq.DeletedAt);

        Assert.InRange(
            faq.DeletedAt.Value,
            before,
            after);

        Assert.Equal(
            faq.DeletedAt,
            faq.UpdatedAt);
    }

    [Fact]
    public void Delete_AlreadyDeleted_DoesNotChangeDeletionTimestamp()
    {
        // Arrange
        var faq =
            CreateFaq(
                isPublished: true);

        faq.Delete();

        var firstDeletedAt =
            faq.DeletedAt;

        var firstUpdatedAt =
            faq.UpdatedAt;

        // Act
        faq.Delete();

        // Assert
        Assert.Equal(
            firstDeletedAt,
            faq.DeletedAt);

        Assert.Equal(
            firstUpdatedAt,
            faq.UpdatedAt);

        Assert.False(faq.IsPublished);
    }

    // =========================================================
    // Deleted FAQ
    // =========================================================

    [Theory]
    [InlineData(DeletedOperation.UpdateContent)]
    [InlineData(DeletedOperation.ReplaceTags)]
    [InlineData(DeletedOperation.Publish)]
    [InlineData(DeletedOperation.Unpublish)]
    [InlineData(DeletedOperation.IncrementViewCount)]
    public void DeletedFaq_ModificationIsAttempted_ThrowsInvalidOperationException(
        DeletedOperation operation)
    {
        // Arrange
        var faq =
            CreateFaq(
                isPublished: true);

        faq.Delete();

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () => ExecuteDeletedOperation(
                    faq,
                    operation));

        // Assert
        Assert.Equal(
            "削除済みのFAQは変更できません。",
            exception.Message);
    }

    // =========================================================
    // Test helpers
    // =========================================================

    private static Faq CreateFaq(
        string title = "FAQタイトル",
        string body = "FAQ本文",
        int categoryId = 1,
        bool isPublished = false)
    {
        return new Faq(
            title,
            body,
            categoryId,
            isPublished);
    }

    private static Tag CreateTag(
        int id,
        string name,
        int displayOrder = 0)
    {
        var tag =
            (Tag)RuntimeHelpers.GetUninitializedObject(
                typeof(Tag));

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

    private static void ExecuteDeletedOperation(
        Faq faq,
        DeletedOperation operation)
    {
        switch (operation)
        {
            case DeletedOperation.UpdateContent:
                faq.UpdateContent(
                    "変更後タイトル",
                    "変更後本文",
                    categoryId: 2);
                break;

            case DeletedOperation.ReplaceTags:
                faq.ReplaceTags(
                [
                    CreateTag(
                        id: 10,
                        name: "タグ")
                ]);
                break;

            case DeletedOperation.Publish:
                faq.Publish();
                break;

            case DeletedOperation.Unpublish:
                faq.Unpublish();
                break;

            case DeletedOperation.IncrementViewCount:
                faq.IncrementViewCount();
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(operation),
                    operation,
                    null);
        }
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

    public enum DeletedOperation
    {
        UpdateContent,
        ReplaceTags,
        Publish,
        Unpublish,
        IncrementViewCount
    }
}