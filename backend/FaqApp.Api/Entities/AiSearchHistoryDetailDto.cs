namespace FaqApp.Api.Dtos.Ai;

public class AiSearchHistoryDetailDto
{
    public int Id { get; set; }

    public string Question { get; set; } = string.Empty;

    public string? SearchKeywords { get; set; }

    public string? AiAnswer { get; set; }

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public bool? IsHelpful { get; set; }

    public DateTime ExecutedAt { get; set; }

    public List<AiSearchHistorySourceDto> Sources { get; set; } = new();
}