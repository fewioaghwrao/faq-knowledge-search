namespace FaqApp.Api.Dtos.Ai;

public class AiSearchHistoryQuery
{
    public string? Keyword { get; set; }

    public bool? IsSuccess { get; set; }

    public bool? IsHelpful { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}