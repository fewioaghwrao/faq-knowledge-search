namespace FaqApp.Api.Dtos.Ai;

public class AiSearchHistorySourceDto
{
    public int FaqId { get; set; }

    public string FaqTitle { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public double? Score { get; set; }

    public string Url { get; set; } = string.Empty;
}
