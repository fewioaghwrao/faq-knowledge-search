namespace FaqApp.Api.Entities;

public class AiSearchHistorySource
{
    public int Id { get; set; }

    public int AiSearchHistoryId { get; set; }

    public int FaqId { get; set; }

    public string FaqTitle { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public double? Score { get; set; }

    public AiSearchHistory AiSearchHistory { get; set; } = null!;

    public Faq Faq { get; set; } = null!;
}