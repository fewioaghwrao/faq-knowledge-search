using System.ComponentModel.DataAnnotations;
using FaqKnowledgeSearch.Razor.Pages.Admin.Faqs;

namespace FaqKnowledgeSearch.Razor.Tests.Pages.Admin.Faqs;

public sealed class FaqFormInputTests
{
    [Fact]
    public void Validate_WhenAllValuesAreValid_ReturnsNoErrors()
    {
        // Arrange
        var sut = CreateValidInput();

        // Act
        var errors = Validate(sut);

        // Assert
        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Validate_WhenTitleIsEmpty_ReturnsRequiredError(
        string title)
    {
        // Arrange
        var sut = CreateValidInput();
        sut.Title = title;

        // Act
        var errors = Validate(sut);

        // Assert
        Assert.Contains(
            errors,
            error =>
                error.ErrorMessage
                == "タイトルを入力してください。");
    }

    [Fact]
    public void Validate_WhenTitleExceeds100Characters_ReturnsLengthError()
    {
        // Arrange
        var sut = CreateValidInput();
        sut.Title = new string('あ', 101);

        // Act
        var errors = Validate(sut);

        // Assert
        Assert.Contains(
            errors,
            error =>
                error.ErrorMessage
                == "タイトルは100文字以内で入力してください。");
    }

    [Fact]
    public void Validate_WhenTitleIsExactly100Characters_ReturnsNoTitleError()
    {
        // Arrange
        var sut = CreateValidInput();
        sut.Title = new string('あ', 100);

        // Act
        var errors = Validate(sut);

        // Assert
        Assert.DoesNotContain(
            errors,
            error =>
                error.MemberNames.Contains(
                    nameof(FaqFormInput.Title)));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Validate_WhenBodyIsEmpty_ReturnsRequiredError(
        string body)
    {
        // Arrange
        var sut = CreateValidInput();
        sut.Body = body;

        // Act
        var errors = Validate(sut);

        // Assert
        Assert.Contains(
            errors,
            error =>
                error.ErrorMessage
                == "本文を入力してください。");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Validate_WhenCategoryIdIsNotPositive_ReturnsRangeError(
        int categoryId)
    {
        // Arrange
        var sut = CreateValidInput();
        sut.CategoryId = categoryId;

        // Act
        var errors = Validate(sut);

        // Assert
        Assert.Contains(
            errors,
            error =>
                error.ErrorMessage
                == "カテゴリを選択してください。");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void TryParseTagIds_WhenValueIsEmpty_ReturnsTrueWithEmptyList(
        string tagIdsText)
    {
        // Arrange
        var sut = CreateValidInput();
        sut.TagIds = tagIdsText;

        // Act
        var result = sut.TryParseTagIds(
            out var tagIds);

        // Assert
        Assert.True(result);
        Assert.Empty(tagIds);
    }

    [Fact]
    public void TryParseTagIds_WhenCommaSeparated_ReturnsParsedIds()
    {
        // Arrange
        var sut = CreateValidInput();
        sut.TagIds = "1,2,3";

        // Act
        var result = sut.TryParseTagIds(
            out var tagIds);

        // Assert
        Assert.True(result);
        Assert.Equal([1, 2, 3], tagIds);
    }

    [Fact]
    public void TryParseTagIds_WhenDifferentSeparatorsAreUsed_ReturnsParsedIds()
    {
        // Arrange
        var sut = CreateValidInput();
        sut.TagIds = "1、2;3,4";

        // Act
        var result = sut.TryParseTagIds(
            out var tagIds);

        // Assert
        Assert.True(result);
        Assert.Equal([1, 2, 3, 4], tagIds);
    }

    [Fact]
    public void TryParseTagIds_WhenValuesContainSpaces_TrimsAndParsesValues()
    {
        // Arrange
        var sut = CreateValidInput();
        sut.TagIds = " 1, 2 、 3 ; 4 ";

        // Act
        var result = sut.TryParseTagIds(
            out var tagIds);

        // Assert
        Assert.True(result);
        Assert.Equal([1, 2, 3, 4], tagIds);
    }

    [Fact]
    public void TryParseTagIds_WhenIdsAreDuplicated_RemovesDuplicates()
    {
        // Arrange
        var sut = CreateValidInput();
        sut.TagIds = "1,2,1,3,2";

        // Act
        var result = sut.TryParseTagIds(
            out var tagIds);

        // Assert
        Assert.True(result);
        Assert.Equal([1, 2, 3], tagIds);
    }

    [Theory]
    [InlineData("1,test,3")]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("999999999999999999999")]
    public void TryParseTagIds_WhenValueIsNotInteger_ReturnsFalse(
        string tagIdsText)
    {
        // Arrange
        var sut = CreateValidInput();
        sut.TagIds = tagIdsText;

        // Act
        var result = sut.TryParseTagIds(
            out var tagIds);

        // Assert
        Assert.False(result);
        Assert.Empty(tagIds);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1,0,3")]
    [InlineData("1,-2,3")]
    public void TryParseTagIds_WhenIdIsNotPositive_ReturnsFalse(
        string tagIdsText)
    {
        // Arrange
        var sut = CreateValidInput();
        sut.TagIds = tagIdsText;

        // Act
        var result = sut.TryParseTagIds(
            out var tagIds);

        // Assert
        Assert.False(result);
        Assert.Empty(tagIds);
    }

    [Fact]
    public void TryParseTagIds_WhenSeparatorsAreRepeated_IgnoresEmptyValues()
    {
        // Arrange
        var sut = CreateValidInput();
        sut.TagIds = "1,,,2、、;3;";

        // Act
        var result = sut.TryParseTagIds(
            out var tagIds);

        // Assert
        Assert.True(result);
        Assert.Equal([1, 2, 3], tagIds);
    }

    private static FaqFormInput CreateValidInput()
    {
        return new FaqFormInput
        {
            Title = "ログイン方法について",
            Body = "ログイン画面から認証情報を入力してください。",
            CategoryId = 1,
            TagIds = "1,2",
            IsPublished = true
        };
    }

    private static IReadOnlyList<ValidationResult> Validate(
        FaqFormInput input)
    {
        var results = new List<ValidationResult>();

        Validator.TryValidateObject(
            input,
            new ValidationContext(input),
            results,
            validateAllProperties: true);

        return results;
    }
}
