namespace FaqKnowledgeSearch.Domain.AiSearch;

public sealed class AiSearchReference
{
    public long Id { get; private set; }

    public long AiSearchHistoryId { get; private set; }

    public int FaqId { get; private set; }

    public string FaqTitle { get; private set; }
        = string.Empty;

    public string CategoryName { get; private set; }
        = string.Empty;

    public int DisplayOrder { get; private set; }

    public int Score { get; private set; }

    // EF Core用
    private AiSearchReference()
    {
    }

    internal AiSearchReference(
        int faqId,
        string faqTitle,
        string categoryName,
        int displayOrder,
        int score)
    {
        if (faqId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(faqId));
        }

        if (string.IsNullOrWhiteSpace(faqTitle))
        {
            throw new ArgumentException(
                "参照FAQタイトルは必須です。",
                nameof(faqTitle));
        }

        if (string.IsNullOrWhiteSpace(categoryName))
        {
            throw new ArgumentException(
                "参照FAQのカテゴリ名は必須です。",
                nameof(categoryName));
        }

        if (displayOrder <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(displayOrder));
        }

        if (score < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(score));
        }

        FaqId = faqId;
        FaqTitle = faqTitle.Trim();
        CategoryName = categoryName.Trim();
        DisplayOrder = displayOrder;
        Score = score;
    }
}