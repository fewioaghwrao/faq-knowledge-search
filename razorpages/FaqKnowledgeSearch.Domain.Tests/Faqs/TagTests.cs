using FaqKnowledgeSearch.Domain.Faqs;

namespace FaqKnowledgeSearch.Domain.Tests.Faqs;

public sealed class TagTests
{
    // =========================================================
    // Constructor
    // =========================================================

    [Fact]
    public void Constructor_ValidValues_CreatesTag()
    {
        // Act
        var tag =
            new Tag(
                "ログイン",
                displayOrder: 1);

        // Assert
        Assert.Equal(
            "ログイン",
            tag.Name);

        Assert.Equal(
            1,
            tag.DisplayOrder);
    }

    [Fact]
    public void Constructor_NameHasSurroundingSpaces_TrimsName()
    {
        // Act
        var tag =
            new Tag(
                "  ログイン  ",
                displayOrder: 1);

        // Assert
        Assert.Equal(
            "ログイン",
            tag.Name);

        Assert.Equal(
            1,
            tag.DisplayOrder);
    }

    [Fact]
    public void Constructor_DisplayOrderIsZero_CreatesTag()
    {
        // Act
        var tag =
            new Tag(
                "ログイン",
                displayOrder: 0);

        // Assert
        Assert.Equal(
            0,
            tag.DisplayOrder);
    }

    [Fact]
    public void Constructor_NameIsNull_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new Tag(
                    null!,
                    displayOrder: 1));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);

        Assert.Contains(
            "タグ名は必須です。",
            exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public void Constructor_NameIsEmptyOrWhiteSpace_ThrowsArgumentException(
        string name)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new Tag(
                    name,
                    displayOrder: 1));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);

        Assert.Contains(
            "タグ名は必須です。",
            exception.Message);
    }

    [Fact]
    public void Constructor_NameIsExactly50Characters_CreatesTag()
    {
        // Arrange
        var name =
            new string('あ', 50);

        // Act
        var tag =
            new Tag(
                name,
                displayOrder: 1);

        // Assert
        Assert.Equal(
            50,
            tag.Name.Length);

        Assert.Equal(
            name,
            tag.Name);
    }

    [Fact]
    public void Constructor_TrimmedNameIsExactly50Characters_CreatesTag()
    {
        // Arrange
        var expectedName =
            new string('あ', 50);

        var name =
            $"  {expectedName}  ";

        // Act
        var tag =
            new Tag(
                name,
                displayOrder: 1);

        // Assert
        Assert.Equal(
            expectedName,
            tag.Name);

        Assert.Equal(
            50,
            tag.Name.Length);
    }

    [Fact]
    public void Constructor_NameExceeds50Characters_ThrowsArgumentException()
    {
        // Arrange
        var name =
            new string('あ', 51);

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new Tag(
                    name,
                    displayOrder: 1));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);

        Assert.Contains(
            "タグ名は50文字以内で入力してください。",
            exception.Message);
    }

    [Fact]
    public void Constructor_TrimmedNameExceeds50Characters_ThrowsArgumentException()
    {
        // Arrange
        var name =
            $"  {new string('あ', 51)}  ";

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new Tag(
                    name,
                    displayOrder: 1));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);

        Assert.Contains(
            "タグ名は50文字以内で入力してください。",
            exception.Message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Constructor_DisplayOrderIsNegative_ThrowsArgumentOutOfRangeException(
        int displayOrder)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new Tag(
                    "ログイン",
                    displayOrder));

        // Assert
        Assert.Equal(
            "displayOrder",
            exception.ParamName);

        Assert.Contains(
            "表示順は0以上で指定してください。",
            exception.Message);
    }

    // =========================================================
    // Change
    // =========================================================

    [Fact]
    public void Change_ValidValues_UpdatesTag()
    {
        // Arrange
        var tag =
            new Tag(
                "変更前",
                displayOrder: 1);

        // Act
        tag.Change(
            "  変更後  ",
            displayOrder: 10);

        // Assert
        Assert.Equal(
            "変更後",
            tag.Name);

        Assert.Equal(
            10,
            tag.DisplayOrder);
    }

    [Fact]
    public void Change_DisplayOrderIsZero_UpdatesTag()
    {
        // Arrange
        var tag =
            new Tag(
                "変更前",
                displayOrder: 5);

        // Act
        tag.Change(
            "変更後",
            displayOrder: 0);

        // Assert
        Assert.Equal(
            "変更後",
            tag.Name);

        Assert.Equal(
            0,
            tag.DisplayOrder);
    }

    [Fact]
    public void Change_NameIsNull_ThrowsArgumentExceptionAndKeepsExistingValues()
    {
        // Arrange
        var tag =
            new Tag(
                "変更前",
                displayOrder: 1);

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => tag.Change(
                    null!,
                    displayOrder: 10));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);

        Assert.Equal(
            "変更前",
            tag.Name);

        Assert.Equal(
            1,
            tag.DisplayOrder);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public void Change_NameIsEmptyOrWhiteSpace_ThrowsAndKeepsExistingValues(
        string name)
    {
        // Arrange
        var tag =
            new Tag(
                "変更前",
                displayOrder: 1);

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => tag.Change(
                    name,
                    displayOrder: 10));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);

        Assert.Equal(
            "変更前",
            tag.Name);

        Assert.Equal(
            1,
            tag.DisplayOrder);
    }

    [Fact]
    public void Change_NameExceeds50Characters_ThrowsAndKeepsExistingValues()
    {
        // Arrange
        var tag =
            new Tag(
                "変更前",
                displayOrder: 1);

        var invalidName =
            new string('あ', 51);

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => tag.Change(
                    invalidName,
                    displayOrder: 10));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);

        Assert.Contains(
            "タグ名は50文字以内で入力してください。",
            exception.Message);

        Assert.Equal(
            "変更前",
            tag.Name);

        Assert.Equal(
            1,
            tag.DisplayOrder);
    }

    [Fact]
    public void Change_DisplayOrderIsNegative_ThrowsAndKeepsExistingValues()
    {
        // Arrange
        var tag =
            new Tag(
                "変更前",
                displayOrder: 1);

        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => tag.Change(
                    "変更後",
                    displayOrder: -1));

        // Assert
        Assert.Equal(
            "displayOrder",
            exception.ParamName);

        Assert.Contains(
            "表示順は0以上で指定してください。",
            exception.Message);

        Assert.Equal(
            "変更前",
            tag.Name);

        Assert.Equal(
            1,
            tag.DisplayOrder);
    }

    [Fact]
    public void Change_ValidValuesCanBeAppliedMultipleTimes()
    {
        // Arrange
        var tag =
            new Tag(
                "最初",
                displayOrder: 1);

        // Act
        tag.Change(
            "二回目",
            displayOrder: 2);

        tag.Change(
            "三回目",
            displayOrder: 3);

        // Assert
        Assert.Equal(
            "三回目",
            tag.Name);

        Assert.Equal(
            3,
            tag.DisplayOrder);
    }
}