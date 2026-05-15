namespace FaqApp.Api.Dtos.Ai;

public class AiSearchResponse
{
    public string? Answer { get; set; }

    public string? Disclaimer { get; set; }

    public List<AiSourceDto> Sources { get; set; } = new();

    public string? Message { get; set; }

    // AI検索履歴ID。フィードバック送信時に使用する。
    public int AiHistoryId { get; set; }
}