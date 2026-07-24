using FaqKnowledgeSearch.Domain.AiSearch;

namespace FaqKnowledgeSearch.Domain.Tests.AiSearch;

public sealed class AiSearchReferenceTests
{
    [Fact]
    public void Constructor_ValidValues_SetsProperties()
    {
        var reference =
            new AiSearchReference(
                faqId: 10,
                faqTitle: "パスワード変更",
                categoryName: "アカウント",
                displayOrder: 1,
                score: 100);

        Assert.Equal(10, reference.FaqId);
        Assert.Equal("パスワード変更", reference.FaqTitle);
        Assert.Equal("アカウント", reference.CategoryName);
        Assert.Equal(1, reference.DisplayOrder);
        Assert.Equal(100, reference.Score);
    }

    [Fact]
    public void Constructor_ValuesHaveSpaces_TrimsStrings()
    {
        var reference =
            new AiSearchReference(
                faqId: 10,
                faqTitle: "  FAQタイトル  ",
                categoryName: "  カテゴリ  ",
                displayOrder: 1,
                score: 0);

        Assert.Equal("FAQタイトル", reference.FaqTitle);
        Assert.Equal("カテゴリ", reference.CategoryName);
        Assert.Equal(0, reference.Score);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_FaqIdIsNotPositive_ThrowsArgumentOutOfRangeException(
        int faqId)
    {
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new AiSearchReference(
                    faqId,
                    "FAQタイトル",
                    "カテゴリ",
                    1,
                    100));

        Assert.Equal("faqId", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Constructor_FaqTitleIsWhiteSpace_ThrowsArgumentException(
        string faqTitle)
    {
        var exception =
            Assert.Throws<ArgumentException>(
                () => new AiSearchReference(
                    1,
                    faqTitle,
                    "カテゴリ",
                    1,
                    100));

        Assert.Equal("faqTitle", exception.ParamName);
        Assert.Contains(
            "参照FAQタイトルは必須です。",
            exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\r\n")]
    public void Constructor_CategoryNameIsWhiteSpace_ThrowsArgumentException(
        string categoryName)
    {
        var exception =
            Assert.Throws<ArgumentException>(
                () => new AiSearchReference(
                    1,
                    "FAQタイトル",
                    categoryName,
                    1,
                    100));

        Assert.Equal("categoryName", exception.ParamName);
        Assert.Contains(
            "参照FAQのカテゴリ名は必須です。",
            exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_DisplayOrderIsNotPositive_ThrowsArgumentOutOfRangeException(
        int displayOrder)
    {
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new AiSearchReference(
                    1,
                    "FAQタイトル",
                    "カテゴリ",
                    displayOrder,
                    100));

        Assert.Equal("displayOrder", exception.ParamName);
    }

    [Fact]
    public void Constructor_ScoreIsNegative_ThrowsArgumentOutOfRangeException()
    {
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new AiSearchReference(
                    1,
                    "FAQタイトル",
                    "カテゴリ",
                    1,
                    score: -1));

        Assert.Equal("score", exception.ParamName);
    }
}
