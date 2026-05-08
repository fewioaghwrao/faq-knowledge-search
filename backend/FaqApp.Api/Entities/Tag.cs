namespace FaqApp.Api.Entities;

public class Tag
{
    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public int DisplayOrder { get; private set; }

    public List<Faq> Faqs { get; private set; } = new();

    private Tag()
    {
    }

    public Tag(string name, int displayOrder)
    {
        Name = name;
        DisplayOrder = displayOrder;
    }
}
