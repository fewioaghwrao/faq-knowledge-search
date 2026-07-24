namespace FaqKnowledgeSearch.Domain.Faqs;

public sealed class Tag
{
    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int DisplayOrder { get; private set; }

    // EF Core用
    private Tag()
    {
    }

    public Tag(string name, int displayOrder)
    {
        Change(name, displayOrder);
    }

    public void Change(string name, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "タグ名は必須です。",
                nameof(name));
        }

        if (name.Trim().Length > 50)
        {
            throw new ArgumentException(
                "タグ名は50文字以内で入力してください。",
                nameof(name));
        }

        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(displayOrder),
                "表示順は0以上で指定してください。");
        }

        Name = name.Trim();
        DisplayOrder = displayOrder;
    }
}