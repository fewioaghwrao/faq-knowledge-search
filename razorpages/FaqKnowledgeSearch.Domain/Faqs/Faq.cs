namespace FaqKnowledgeSearch.Domain.Faqs;

public sealed class Faq
{
    private readonly List<Tag> _tags = [];

    public int Id { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Body { get; private set; } = string.Empty;

    public int CategoryId { get; private set; }

    public Category Category { get; private set; } = null!;

    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    public bool IsPublished { get; private set; }

    public int ViewCount { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public bool IsDeleted => DeletedAt.HasValue;

    // EF Core用
    private Faq()
    {
    }

    public Faq(
        string title,
        string body,
        int categoryId,
        bool isPublished = false)
    {
        ValidateContent(title, body, categoryId);

        var now = DateTime.UtcNow;

        Title = title.Trim();
        Body = body.Trim();
        CategoryId = categoryId;
        IsPublished = isPublished;
        CreatedAt = now;
        UpdatedAt = now;
    }

    public void UpdateContent(
        string title,
        string body,
        int categoryId)
    {
        EnsureNotDeleted();
        ValidateContent(title, body, categoryId);

        Title = title.Trim();
        Body = body.Trim();
        CategoryId = categoryId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReplaceTags(IEnumerable<Tag> tags)
    {
        ArgumentNullException.ThrowIfNull(tags);
        EnsureNotDeleted();

        _tags.Clear();

        foreach (var tag in tags)
        {
            ArgumentNullException.ThrowIfNull(tag);

            if (_tags.Any(existingTag => IsSameTag(existingTag, tag)))
            {
                continue;
            }

            _tags.Add(tag);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void Publish()
    {
        EnsureNotDeleted();

        if (IsPublished)
        {
            return;
        }

        IsPublished = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Unpublish()
    {
        EnsureNotDeleted();

        if (!IsPublished)
        {
            return;
        }

        IsPublished = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementViewCount()
    {
        EnsureNotDeleted();

        // 閲覧はFAQ内容の更新ではないため、
        // UpdatedAtは変更しない。
        ViewCount++;
    }

    public void Delete()
    {
        if (IsDeleted)
        {
            return;
        }

        var now = DateTime.UtcNow;

        IsPublished = false;
        DeletedAt = now;
        UpdatedAt = now;
    }

    private static void ValidateContent(
        string title,
        string body,
        int categoryId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "FAQタイトルは必須です。",
                nameof(title));
        }

        if (title.Trim().Length > 100)
        {
            throw new ArgumentException(
                "FAQタイトルは100文字以内で入力してください。",
                nameof(title));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException(
                "FAQ本文は必須です。",
                nameof(body));
        }

        if (categoryId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(categoryId),
                "有効なカテゴリを指定してください。");
        }
    }

    private static bool IsSameTag(Tag first, Tag second)
    {
        // DB登録後のTag同士ならIDで判定
        if (first.Id > 0 && second.Id > 0)
        {
            return first.Id == second.Id;
        }

        // DB登録前なら名前で重複判定
        return string.Equals(
            first.Name,
            second.Name,
            StringComparison.OrdinalIgnoreCase);
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException(
                "削除済みのFAQは変更できません。");
        }
    }
}