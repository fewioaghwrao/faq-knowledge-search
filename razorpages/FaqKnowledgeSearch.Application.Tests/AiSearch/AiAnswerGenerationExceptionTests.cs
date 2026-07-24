using FaqKnowledgeSearch.Application.AiSearch;

namespace FaqKnowledgeSearch.Application.Tests.AiSearch;

public sealed class AiAnswerGenerationExceptionTests
{
    [Fact]
    public void Constructor_MessageOnly_SetsMessage()
    {
        // Arrange
        const string message = "AI回答の生成に失敗しました。";

        // Act
        var exception =
            new AiAnswerGenerationException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void Constructor_MessageAndInnerException_SetsProperties()
    {
        // Arrange
        const string message = "AI回答の生成に失敗しました。";

        var innerException =
            new InvalidOperationException(
                "外部AIサービスでエラーが発生しました。");

        // Act
        var exception =
            new AiAnswerGenerationException(
                message,
                innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Same(
            innerException,
            exception.InnerException);
    }
}