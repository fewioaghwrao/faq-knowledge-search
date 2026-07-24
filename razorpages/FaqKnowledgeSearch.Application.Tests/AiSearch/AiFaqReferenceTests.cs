using FaqKnowledgeSearch.Application.AiSearch;

namespace FaqKnowledgeSearch.Application.Tests.AiSearch;

public sealed class AiFaqReferenceTests
{
    [Fact]
    public void BodyPreview_BodyIsShorterThan140Characters_ReturnsOriginalBody()
    {
        // Arrange
        const string body = "短いFAQ本文です。";

        var reference = CreateReference(body);

        // Act
        var result = reference.BodyPreview;

        // Assert
        Assert.Equal(body, result);
    }

    [Fact]
    public void BodyPreview_BodyIsExactly140Characters_ReturnsOriginalBody()
    {
        // Arrange
        var body = new string('あ', 140);

        var reference = CreateReference(body);

        // Act
        var result = reference.BodyPreview;

        // Assert
        Assert.Equal(body, result);
        Assert.Equal(140, result.Length);
    }

    [Fact]
    public void BodyPreview_BodyIs141Characters_ReturnsFirst140CharactersWithEllipsis()
    {
        // Arrange
        var body = new string('あ', 141);

        var reference = CreateReference(body);

        // Act
        var result = reference.BodyPreview;

        // Assert
        Assert.Equal(
            new string('あ', 140) + "…",
            result);

        Assert.Equal(141, result.Length);
    }

    [Fact]
    public void BodyPreview_BodyIsLongerThan140Characters_TruncatesBody()
    {
        // Arrange
        var body =
            new string('あ', 140) +
            new string('い', 60);

        var reference = CreateReference(body);

        // Act
        var result = reference.BodyPreview;

        // Assert
        Assert.Equal(
            new string('あ', 140) + "…",
            result);

        Assert.DoesNotContain("い", result);
    }

    [Fact]
    public void BodyPreview_BodyIsEmpty_ReturnsEmptyString()
    {
        // Arrange
        var reference = CreateReference(string.Empty);

        // Act
        var result = reference.BodyPreview;

        // Assert
        Assert.Empty(result);
    }

    private static AiFaqReference CreateReference(
        string body)
    {
        return new AiFaqReference(
            Id: 1,
            Title: "テストFAQ",
            Body: body,
            CategoryName: "テストカテゴリ",
            Tags: ["テスト"],
            Score: 100);
    }
}