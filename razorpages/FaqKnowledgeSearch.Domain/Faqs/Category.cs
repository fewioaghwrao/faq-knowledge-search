namespace FaqKnowledgeSearch.Domain.Faqs;

public sealed class Category
{
    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int DisplayOrder { get; private set; }

    // EF Core用
    private Category()
    {
    }

    public Category(string name, int displayOrder)
    {
        Change(name, displayOrder);
    }

    public void Change(string name, int displayOrder)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "カテゴリ名は必須です。",
                nameof(name));
        }

        if (name.Trim().Length > 50)
        {
            throw new ArgumentException(
                "カテゴリ名は50文字以内で入力してください。",
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