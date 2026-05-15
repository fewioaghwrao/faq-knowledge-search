namespace FaqApp.Api.Dtos.Ai;

public class AiSearchHistoryListItemDto
{
    public int Id { get; set; }

    public string Question { get; set; } = string.Empty;

    public string? AnswerPreview { get; set; }

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public bool? IsHelpful { get; set; }

    public int SourceCount { get; set; }

    public DateTime ExecutedAt { get; set; }
}