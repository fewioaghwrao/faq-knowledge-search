namespace FaqApp.Api.Dtos.Faqs;

public class FaqListItemDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    // 詳細画面でも同じDTOを使っているため残す
    public string Body { get; set; } = string.Empty;

    // フェーズ2: ハイライト済みタイトル
    public string? TitleHighlighted { get; set; }

    // フェーズ2: 本文抜粋
    public string? BodyExcerpt { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    // 現在のFaqServiceで使っているため必要
    public List<string> Tags { get; set; } = new();

    public bool IsPublished { get; set; }

    public int ViewCount { get; set; }

    // フェーズ2: 簡易スコア
    public double Score { get; set; }

    public DateTime UpdatedAt { get; set; }
}