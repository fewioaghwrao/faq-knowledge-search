namespace FaqApp.Api.Dtos.Ai;

public class AiSearchResponse
{
    public string? Answer { get; set; }

    public string? Disclaimer { get; set; }

    public List<AiSourceDto> Sources { get; set; } = new();

    public string? Message { get; set; }

    // フェーズ5で履歴テーブルを作るまでの仮ID
    public int AiHistoryId { get; set; }
}