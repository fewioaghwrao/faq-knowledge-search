using FaqKnowledgeSearch.Domain.AiSearch;

namespace FaqKnowledgeSearch.Domain.Tests.AiSearch;

public sealed class AiSearchHistoryTests
{
    // =========================================================
    // CreateSuccess
    // =========================================================

    [Fact]
    public void CreateSuccess_ValidValues_CreatesSuccessHistory()
    {
        // Arrange
        const string question = "パスワードの変更方法は？";
        const string answer = "設定画面から変更できます。";
        const string modelName = "gpt-test";

        var before = DateTime.UtcNow;

        // Act
        var history =
            AiSearchHistory.CreateSuccess(
                question,
                answer,
                modelName,
                usedExternalAi: true);

        var after = DateTime.UtcNow;

        // Assert
        Assert.Equal(question, history.Question);
        Assert.Equal(answer, history.Answer);
        Assert.True(history.IsSuccess);
        Assert.Null(history.ErrorMessage);
        Assert.Equal(modelName, history.ModelName);
        Assert.True(history.UsedExternalAi);
        Assert.Null(history.WasHelpful);
        Assert.Empty(history.References);

        Assert.InRange(
            history.CreatedAt,
            before,
            after);
    }

    [Fact]
    public void CreateSuccess_ValuesHaveSurroundingSpaces_TrimsValues()
    {
        // Act
        var history =
            AiSearchHistory.CreateSuccess(
                "  質問です  ",
                "  回答です  ",
                "  test-model  ",
                usedExternalAi: true);

        // Assert
        Assert.Equal("質問です", history.Question);
        Assert.Equal("回答です", history.Answer);
        Assert.Equal("test-model", history.ModelName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("\r\n")]
    public void CreateSuccess_AnswerIsNullOrWhiteSpace_ThrowsArgumentException(
        string answer)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => AiSearchHistory.CreateSuccess(
                    "質問",
                    answer,
                    "test-model",
                    usedExternalAi: true));

        // Assert
        Assert.Equal("answer", exception.ParamName);
        Assert.Contains(
            "成功履歴には回答が必要です。",
            exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void CreateSuccess_QuestionIsNullOrWhiteSpace_ThrowsArgumentException(
        string question)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => AiSearchHistory.CreateSuccess(
                    question,
                    "回答",
                    "test-model",
                    usedExternalAi: true));

        // Assert
        Assert.Equal("question", exception.ParamName);
        Assert.Contains(
            "質問は必須です。",
            exception.Message);
    }

    [Fact]
    public void CreateSuccess_QuestionIsExactly500Characters_CreatesHistory()
    {
        // Arrange
        var question = new string('あ', 500);

        // Act
        var history =
            AiSearchHistory.CreateSuccess(
                question,
                "回答",
                "test-model",
                usedExternalAi: true);

        // Assert
        Assert.Equal(500, history.Question.Length);
    }

    [Fact]
    public void CreateSuccess_QuestionExceeds500Characters_ThrowsArgumentException()
    {
        // Arrange
        var question = new string('あ', 501);

        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => AiSearchHistory.CreateSuccess(
                    question,
                    "回答",
                    "test-model",
                    usedExternalAi: true));

        // Assert
        Assert.Equal("question", exception.ParamName);
        Assert.Contains(
            "質問は500文字以内で入力してください。",
            exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\r\n")]
    public void CreateSuccess_ModelNameIsNullOrWhiteSpace_ThrowsArgumentException(
        string modelName)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => AiSearchHistory.CreateSuccess(
                    "質問",
                    "回答",
                    modelName,
                    usedExternalAi: true));

        // Assert
        Assert.Equal("modelName", exception.ParamName);
        Assert.Contains(
            "AIモデル名は必須です。",
            exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateSuccess_UsedExternalAiIsSpecified_SetsValue(
        bool usedExternalAi)
    {
        // Act
        var history =
            AiSearchHistory.CreateSuccess(
                "質問",
                "回答",
                "test-model",
                usedExternalAi);

        // Assert
        Assert.Equal(
            usedExternalAi,
            history.UsedExternalAi);
    }

    // =========================================================
    // CreateFailure
    // =========================================================

    [Fact]
    public void CreateFailure_ValidValues_CreatesFailureHistory()
    {
        // Act
        var history =
            AiSearchHistory.CreateFailure(
                "AI検索に失敗する質問",
                "外部AIとの通信に失敗しました。",
                "test-model",
                usedExternalAi: true);

        // Assert
        Assert.Equal(
            "AI検索に失敗する質問",
            history.Question);

        Assert.Null(history.Answer);
        Assert.False(history.IsSuccess);

        Assert.Equal(
            "外部AIとの通信に失敗しました。",
            history.ErrorMessage);

        Assert.Equal(
            "test-model",
            history.ModelName);

        Assert.True(history.UsedExternalAi);
        Assert.Null(history.WasHelpful);
    }

    [Fact]
    public void CreateFailure_ValuesHaveSurroundingSpaces_TrimsValues()
    {
        // Act
        var history =
            AiSearchHistory.CreateFailure(
                "  質問  ",
                "  エラー内容  ",
                "  test-model  ",
                usedExternalAi: true);

        // Assert
        Assert.Equal("質問", history.Question);
        Assert.Equal(
            "エラー内容",
            history.ErrorMessage);
        Assert.Equal(
            "test-model",
            history.ModelName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void CreateFailure_ErrorMessageIsNullOrWhiteSpace_ThrowsArgumentException(
        string errorMessage)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => AiSearchHistory.CreateFailure(
                    "質問",
                    errorMessage,
                    "test-model",
                    usedExternalAi: true));

        // Assert
        Assert.Equal(
            "errorMessage",
            exception.ParamName);

        Assert.Contains(
            "失敗履歴にはエラー内容が必要です。",
            exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CreateFailure_UsedExternalAiIsSpecified_SetsValue(
        bool usedExternalAi)
    {
        // Act
        var history =
            AiSearchHistory.CreateFailure(
                "質問",
                "エラー",
                "test-model",
                usedExternalAi);

        // Assert
        Assert.Equal(
            usedExternalAi,
            history.UsedExternalAi);
    }

    // =========================================================
    // AddReference
    // =========================================================

    [Fact]
    public void AddReference_ValidValues_AddsReference()
    {
        // Arrange
        var history = CreateEligibleHistory();

        // Act
        history.AddReference(
            faqId: 10,
            faqTitle: "パスワード変更方法",
            categoryName: "アカウント",
            displayOrder: 1,
            score: 100);

        // Assert
        var reference =
            Assert.Single(history.References);

        Assert.Equal(10, reference.FaqId);
        Assert.Equal(
            "パスワード変更方法",
            reference.FaqTitle);
        Assert.Equal(
            "アカウント",
            reference.CategoryName);
        Assert.Equal(1, reference.DisplayOrder);
        Assert.Equal(100, reference.Score);
    }

    [Fact]
    public void AddReference_DifferentFaqIds_AddsAllReferences()
    {
        // Arrange
        var history = CreateEligibleHistory();

        // Act
        history.AddReference(
            10,
            "FAQ10",
            "カテゴリ",
            1,
            100);

        history.AddReference(
            20,
            "FAQ20",
            "カテゴリ",
            2,
            80);

        // Assert
        Assert.Equal(
            2,
            history.References.Count);
    }

    [Fact]
    public void AddReference_SameFaqId_DoesNotAddDuplicate()
    {
        // Arrange
        var history = CreateEligibleHistory();

        history.AddReference(
            10,
            "最初のFAQ",
            "カテゴリ1",
            1,
            100);

        // Act
        history.AddReference(
            10,
            "重複したFAQ",
            "カテゴリ2",
            2,
            50);

        // Assert
        var reference =
            Assert.Single(history.References);

        Assert.Equal(
            "最初のFAQ",
            reference.FaqTitle);

        Assert.Equal(100, reference.Score);
    }

    // =========================================================
    // SetFeedback
    // =========================================================

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SetFeedback_SuccessAndExternalAiUsed_SetsWasHelpful(
        bool wasHelpful)
    {
        // Arrange
        var history = CreateEligibleHistory();

        // Act
        history.SetFeedback(wasHelpful);

        // Assert
        Assert.Equal(
            wasHelpful,
            history.WasHelpful);
    }

    [Fact]
    public void SetFeedback_CalledTwice_OverwritesPreviousValue()
    {
        // Arrange
        var history = CreateEligibleHistory();

        history.SetFeedback(true);

        // Act
        history.SetFeedback(false);

        // Assert
        Assert.False(history.WasHelpful);
    }

    [Fact]
    public void SetFeedback_FailureHistory_ThrowsInvalidOperationException()
    {
        // Arrange
        var history =
            AiSearchHistory.CreateFailure(
                "質問",
                "エラー内容",
                "test-model",
                usedExternalAi: true);

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () => history.SetFeedback(true));

        // Assert
        Assert.Equal(
            "失敗したAI検索にはフィードバックを登録できません。",
            exception.Message);

        Assert.Null(history.WasHelpful);
    }

    [Fact]
    public void SetFeedback_ExternalAiWasNotUsed_ThrowsInvalidOperationException()
    {
        // Arrange
        var history =
            AiSearchHistory.CreateSuccess(
                "質問",
                "参照FAQが見つかりませんでした。",
                "test-model",
                usedExternalAi: false);

        // Act
        var exception =
            Assert.Throws<InvalidOperationException>(
                () => history.SetFeedback(true));

        // Assert
        Assert.Equal(
            "外部AIを使用していない回答にはフィードバックを登録できません。",
            exception.Message);

        Assert.Null(history.WasHelpful);
    }

    private static AiSearchHistory CreateEligibleHistory()
    {
        return AiSearchHistory.CreateSuccess(
            "質問",
            "回答",
            "test-model",
            usedExternalAi: true);
    }
}