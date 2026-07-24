namespace FaqKnowledgeSearch.Domain.AiSearch;

public sealed class AiSearchHistory
{
    private readonly List<AiSearchReference> _references = [];

    public long Id { get; private set; }

    public string Question { get; private set; }
        = string.Empty;

    public string? Answer { get; private set; }

    public bool IsSuccess { get; private set; }

    public string? ErrorMessage { get; private set; }

    public string ModelName { get; private set; }
        = string.Empty;

    public bool UsedExternalAi { get; private set; }

    public bool? WasHelpful { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<AiSearchReference> References =>
        _references.AsReadOnly();

    // EF Core用
    private AiSearchHistory()
    {
    }

    private AiSearchHistory(
        string question,
        string? answer,
        bool isSuccess,
        string? errorMessage,
        string modelName,
        bool usedExternalAi)
    {
        ValidateQuestion(question);

        if (string.IsNullOrWhiteSpace(modelName))
        {
            throw new ArgumentException(
                "AIモデル名は必須です。",
                nameof(modelName));
        }

        Question = question.Trim();
        Answer = string.IsNullOrWhiteSpace(answer)
            ? null
            : answer.Trim();

        IsSuccess = isSuccess;

        ErrorMessage =
            string.IsNullOrWhiteSpace(errorMessage)
                ? null
                : errorMessage.Trim();

        ModelName = modelName.Trim();
        UsedExternalAi = usedExternalAi;
        CreatedAt = DateTime.UtcNow;
    }

    public static AiSearchHistory CreateSuccess(
        string question,
        string answer,
        string modelName,
        bool usedExternalAi)
    {
        if (string.IsNullOrWhiteSpace(answer))
        {
            throw new ArgumentException(
                "成功履歴には回答が必要です。",
                nameof(answer));
        }

        return new AiSearchHistory(
            question,
            answer,
            isSuccess: true,
            errorMessage: null,
            modelName,
            usedExternalAi);
    }

    public static AiSearchHistory CreateFailure(
        string question,
        string errorMessage,
        string modelName,
        bool usedExternalAi)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException(
                "失敗履歴にはエラー内容が必要です。",
                nameof(errorMessage));
        }

        return new AiSearchHistory(
            question,
            answer: null,
            isSuccess: false,
            errorMessage,
            modelName,
            usedExternalAi);
    }

    public void AddReference(
        int faqId,
        string faqTitle,
        string categoryName,
        int displayOrder,
        int score)
    {
        if (_references.Any(
                reference => reference.FaqId == faqId))
        {
            return;
        }

        _references.Add(
            new AiSearchReference(
                faqId,
                faqTitle,
                categoryName,
                displayOrder,
                score));
    }

    // フィードバック実装時に使用
    public void SetFeedback(bool wasHelpful)
    {
        if (!IsSuccess)
        {
            throw new InvalidOperationException(
                "失敗したAI検索にはフィードバックを登録できません。");
        }

        if (!UsedExternalAi)
        {
            throw new InvalidOperationException(
                "外部AIを使用していない回答にはフィードバックを登録できません。");
        }

        WasHelpful = wasHelpful;
    }

    private static void ValidateQuestion(string question)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException(
                "質問は必須です。",
                nameof(question));
        }

        if (question.Trim().Length > 500)
        {
            throw new ArgumentException(
                "質問は500文字以内で入力してください。",
                nameof(question));
        }
    }
}