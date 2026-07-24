using FaqKnowledgeSearch.Domain.Faqs;

namespace FaqKnowledgeSearch.Domain.Tests.Faqs;

public sealed class CategoryTests
{
    [Fact]
    public void Constructor_ValidValues_CreatesCategory()
    {
        // Act
        var category =
            new Category(
                "アカウント",
                displayOrder: 1);

        // Assert
        Assert.Equal(
            "アカウント",
            category.Name);

        Assert.Equal(
            1,
            category.DisplayOrder);
    }

    [Fact]
    public void Constructor_NameHasSurroundingSpaces_TrimsName()
    {
        // Act
        var category =
            new Category(
                "  アカウント  ",
                displayOrder: 1);

        // Assert
        Assert.Equal(
            "アカウント",
            category.Name);
    }

    [Fact]
    public void Constructor_DisplayOrderIsZero_CreatesCategory()
    {
        // Act
        var category =
            new Category(
                "アカウント",
                displayOrder: 0);

        // Assert
        Assert.Equal(
            0,
            category.DisplayOrder);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public void Constructor_NameIsNullOrWhiteSpace_ThrowsArgumentException(
        string name)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new Category(
                    name,
                    displayOrder: 1));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);

        Assert.Contains(
            "カテゴリ名は必須です。",
            exception.Message);
    }

    [Fact]
    public void Constructor_NameIsExactly50Characters_CreatesCategory()
    {
        // Arrange
        var name =
            new string('あ', 50);

        // Act
        var category =
            new Category(
                name,
                displayOrder: 1);

        // Assert
        Assert.Equal(
            50,
            category.Name.Length);
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
                () => new Category(
                    name,
                    displayOrder: 1));

        // Assert
        Assert.Equal(
            "name",
            exception.ParamName);

        Assert.Contains(
            "カテゴリ名は50文字以内で入力してください。",
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
                () => new Category(
                    "アカウント",
                    displayOrder));

        // Assert
        Assert.Equal(
            "displayOrder",
            exception.ParamName);

        Assert.Contains(
            "表示順は0以上で指定してください。",
            exception.Message);
    }

    [Fact]
    public void Change_ValidValues_UpdatesCategory()
    {
        // Arrange
        var category =
            new Category(
                "変更前",
                displayOrder: 1);

        // Act
        category.Change(
            "  変更後  ",
            displayOrder: 5);

        // Assert
        Assert.Equal(
            "変更後",
            category.Name);

        Assert.Equal(
            5,
            category.DisplayOrder);
    }

    [Fact]
    public void Change_InvalidName_DoesNotChangeExistingValues()
    {
        // Arrange
        var category =
            new Category(
                "変更前",
                displayOrder: 1);

        // Act
        Assert.Throws<ArgumentException>(
            () => category.Change(
                " ",
                displayOrder: 5));

        // Assert
        Assert.Equal(
            "変更前",
            category.Name);

        Assert.Equal(
            1,
            category.DisplayOrder);
    }

    [Fact]
    public void Change_InvalidDisplayOrder_DoesNotChangeExistingValues()
    {
        // Arrange
        var category =
            new Category(
                "変更前",
                displayOrder: 1);

        // Act
        Assert.Throws<ArgumentOutOfRangeException>(
            () => category.Change(
                "変更後",
                displayOrder: -1));

        // Assert
        Assert.Equal(
            "変更前",
            category.Name);

        Assert.Equal(
            1,
            category.DisplayOrder);
    }
}