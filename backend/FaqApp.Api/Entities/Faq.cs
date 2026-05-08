namespace FaqApp.Api.Entities;

public class Faq
{
    public int Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public bool IsPublished { get; private set; }

    public int CategoryId { get; private set; }

    public Category Category { get; private set; } = null!;

    public List<Tag> Tags { get; private set; } = new();

    public int ViewCount { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    private Faq()
    {
    }

    public Faq(string title, string body, int categoryId, bool isPublished)
    {
        Title = title;
        Body = body;
        CategoryId = categoryId;
        IsPublished = isPublished;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(string title, string body, int categoryId, bool isPublished)
    {
        Title = title;
        Body = body;
        CategoryId = categoryId;
        IsPublished = isPublished;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementViewCount()
    {
        ViewCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}