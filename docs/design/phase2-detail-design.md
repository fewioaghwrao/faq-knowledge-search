# 詳細設計書

**社内FAQ検索アプリ（フェーズ1〜6 統合版）**

| プロジェクト名 | 社内FAQ検索アプリ | バージョン | 1.0 |
|---|---|---|---|
| 作成者 | — | 作成日 | 2026/05/09 |
| 承認者 | — | 承認日 | yyyy/mm/dd |

## 改訂履歴

| バージョン | 日付 | 変更内容 | 作成者 | 承認者 |
|---|---|---|---|---|
| 1.0 | 2026/05/09 | 初版作成（フェーズ1〜6 統合） | — | — |

---

## 1. バックエンド詳細設計

### 1.1 エンティティ定義

#### ApplicationUser.cs【フェーズ1】

```csharp
// FaqApp.Domain/Entities/ApplicationUser.cs
public class ApplicationUser : IdentityUser
{
    public string    DisplayName { get; set; } = string.Empty;
    public bool      IsActive    { get; set; } = true;
    public DateTime  CreatedAt   { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt   { get; set; }

    public ICollection<SearchHistory> SearchHistories { get; set; } = [];
    public ICollection<AiHistory>     AiHistories     { get; set; } = [];
}
```

#### Faq.cs【フェーズ1】

```csharp
// FaqApp.Domain/Entities/Faq.cs
public class Faq
{
    public int      Id             { get; private set; }
    public string   Title          { get; private set; } = string.Empty;
    public string   Body           { get; private set; } = string.Empty;
    public int      CategoryId     { get; private set; }
    public bool     IsPublished    { get; private set; }
    public int      ViewCount      { get; private set; }
    public int      HelpfulCount   { get; private set; }     // 余力
    public int      NotHelpfulCount{ get; private set; }     // 余力
    public DateTime  CreatedAt     { get; private set; }
    public DateTime? UpdatedAt     { get; private set; }
    public DateTime? DeletedAt     { get; private set; }

    public Category             Category { get; private set; } = null!;
    public ICollection<FaqTag>  FaqTags  { get; private set; } = [];

    // ファクトリメソッド
    public static Faq Create(string title, string body, int categoryId, bool isPublished)
        => new() {
            Title       = title,
            Body        = body,
            CategoryId  = categoryId,
            IsPublished = isPublished,
            CreatedAt   = DateTime.UtcNow
        };

    public void Update(string title, string body, int categoryId, bool isPublished)
    {
        Title       = title;
        Body        = body;
        CategoryId  = categoryId;
        IsPublished = isPublished;
        UpdatedAt   = DateTime.UtcNow;
    }

    public void IncrementViewCount()    => ViewCount++;
    public void IncrementHelpful()      => HelpfulCount++;      // 余力
    public void IncrementNotHelpful()   => NotHelpfulCount++;   // 余力
    public void Delete()                => DeletedAt = DateTime.UtcNow;
    public bool IsDeleted               => DeletedAt.HasValue;
}
```

#### Category.cs【フェーズ1】

```csharp
// FaqApp.Domain/Entities/Category.cs
public class Category
{
    public int      Id           { get; private set; }
    public string   Name         { get; private set; } = string.Empty;
    public int      DisplayOrder { get; private set; }
    public DateTime  CreatedAt  { get; private set; }
    public DateTime? UpdatedAt  { get; private set; }

    public ICollection<Faq> Faqs { get; private set; } = [];

    public static Category Create(string name, int displayOrder)
        => new() { Name = name, DisplayOrder = displayOrder, CreatedAt = DateTime.UtcNow };

    public void Update(string name, int displayOrder)
    {
        Name         = name;
        DisplayOrder = displayOrder;
        UpdatedAt    = DateTime.UtcNow;
    }
}
```

#### Tag.cs【フェーズ1】

```csharp
// FaqApp.Domain/Entities/Tag.cs
public class Tag
{
    public int      Id        { get; private set; }
    public string   Name      { get; private set; } = string.Empty;
    public DateTime  CreatedAt{ get; private set; }
    public DateTime? UpdatedAt{ get; private set; }

    public ICollection<FaqTag> FaqTags { get; private set; } = [];

    public static Tag Create(string name)
        => new() { Name = name, CreatedAt = DateTime.UtcNow };

    public void Update(string name) { Name = name; UpdatedAt = DateTime.UtcNow; }
}
```

#### FaqTag.cs【フェーズ1】

```csharp
// FaqApp.Domain/Entities/FaqTag.cs
public class FaqTag
{
    public int FaqId { get; set; }
    public int TagId { get; set; }
    public Faq Faq   { get; set; } = null!;
    public Tag Tag   { get; set; } = null!;
}
```

#### SearchHistory.cs【フェーズ1】

```csharp
// FaqApp.Domain/Entities/SearchHistory.cs
public class SearchHistory
{
    public int      Id         { get; set; }
    public string   Keyword    { get; set; } = string.Empty;
    public string?  UserId     { get; set; }   // FK → AspNetUsers.Id（ログイン時）
    public string?  SessionId  { get; set; }   // 未ログイン時のセッション識別子
    public DateTime SearchedAt { get; set; }

    public ApplicationUser? User { get; set; }

    public static SearchHistory Create(string keyword, string? userId, string? sessionId)
        => new() { Keyword = keyword, UserId = userId, SessionId = sessionId,
                   SearchedAt = DateTime.UtcNow };
}
```

#### AiHistory.cs【フェーズ5】

```csharp
// FaqApp.Domain/Entities/AiHistory.cs
public class AiHistory
{
    public int      Id             { get; set; }
    public string   Question       { get; set; } = string.Empty;
    public string?  SearchKeywords { get; set; }
    public string?  AiAnswer       { get; set; }
    public string?  UserId         { get; set; }   // FK → AspNetUsers.Id
    public bool     IsSuccess      { get; set; }
    public string?  ErrorMessage   { get; set; }
    public bool?    IsHelpful      { get; set; }   // null=未評価
    public DateTime ExecutedAt     { get; set; }

    public ApplicationUser?            User    { get; set; }
    public ICollection<AiHistorySource> Sources { get; set; } = [];

    public static AiHistory CreateSuccess(
        string question, string? keywords, string answer, string? userId,
        IEnumerable<(int FaqId, float Score, int Order)> sources)
    {
        var history = new AiHistory {
            Question       = question,
            SearchKeywords = keywords,
            AiAnswer       = answer,
            UserId         = userId,
            IsSuccess      = true,
            ExecutedAt     = DateTime.UtcNow
        };
        foreach (var (faqId, score, order) in sources)
            history.Sources.Add(new AiHistorySource {
                FaqId = faqId, Score = score, DisplayOrder = order });
        return history;
    }

    public static AiHistory CreateFailure(string question, string? userId, string errorMessage)
        => new() {
            Question     = question,
            UserId       = userId,
            IsSuccess    = false,
            ErrorMessage = errorMessage,
            ExecutedAt   = DateTime.UtcNow
        };

    public void SetFeedback(bool isHelpful) => IsHelpful = isHelpful;
}
```

#### AiHistorySource.cs【フェーズ5】

```csharp
// FaqApp.Domain/Entities/AiHistorySource.cs
public class AiHistorySource
{
    public int    Id           { get; set; }
    public int    AiHistoryId  { get; set; }
    public int    FaqId        { get; set; }
    public float? Score        { get; set; }
    public int    DisplayOrder { get; set; }

    public AiHistory AiHistory { get; set; } = null!;
    public Faq       Faq       { get; set; } = null!;
}
```

---

### 1.2 AppDbContext 詳細

```csharp
// FaqApp.Infrastructure/Data/AppDbContext.cs
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Faq>             Faqs             => Set<Faq>();
    public DbSet<Category>        Categories       => Set<Category>();
    public DbSet<Tag>             Tags             => Set<Tag>();
    public DbSet<FaqTag>          FaqTags          => Set<FaqTag>();
    public DbSet<SearchHistory>   SearchHistories  => Set<SearchHistory>();
    public DbSet<AiHistory>       AiHistories      => Set<AiHistory>();      // フェーズ5
    public DbSet<AiHistorySource> AiHistorySources => Set<AiHistorySource>(); // フェーズ5

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ── Faq ──────────────────────────────────────────
        mb.Entity<Faq>(e => {
            e.HasQueryFilter(f => f.DeletedAt == null);   // 論理削除グローバルフィルター
            e.Property(f => f.IsPublished).HasDefaultValue(false);
            e.Property(f => f.ViewCount).HasDefaultValue(0);
            e.Property(f => f.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasOne(f => f.Category)
             .WithMany(c => c.Faqs)
             .HasForeignKey(f => f.CategoryId)
             .OnDelete(DeleteBehavior.Restrict); // カテゴリ削除はアプリ層で制御
        });

        // ── FaqTag ────────────────────────────────────────
        mb.Entity<FaqTag>(e => {
            e.HasKey(ft => new { ft.FaqId, ft.TagId });
            e.HasOne(ft => ft.Faq).WithMany(f => f.FaqTags)
             .HasForeignKey(ft => ft.FaqId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(ft => ft.Tag).WithMany(t => t.FaqTags)
             .HasForeignKey(ft => ft.TagId).OnDelete(DeleteBehavior.Cascade);
        });

        // ── Category ──────────────────────────────────────
        mb.Entity<Category>(e => {
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ── Tag ───────────────────────────────────────────
        mb.Entity<Tag>(e => {
            e.HasIndex(t => t.Name).IsUnique();
            e.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ── SearchHistory ─────────────────────────────────
        mb.Entity<SearchHistory>(e => {
            e.Property(s => s.SearchedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasOne(s => s.User).WithMany(u => u.SearchHistories)
             .HasForeignKey(s => s.UserId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        // ── ApplicationUser（拡張） ───────────────────────
        mb.Entity<ApplicationUser>(e => {
            e.Property(u => u.IsActive).HasDefaultValue(true);
            e.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        // ── AiHistory（フェーズ5） ────────────────────────
        mb.Entity<AiHistory>(e => {
            e.Property(a => a.IsSuccess).HasDefaultValue(false);
            e.Property(a => a.ExecutedAt).HasDefaultValueSql("GETUTCDATE()");
            e.HasOne(a => a.User).WithMany(u => u.AiHistories)
             .HasForeignKey(a => a.UserId)
             .OnDelete(DeleteBehavior.SetNull)
             .IsRequired(false);
        });

        // ── AiHistorySource（フェーズ5） ──────────────────
        mb.Entity<AiHistorySource>(e => {
            e.HasOne(s => s.AiHistory).WithMany(h => h.Sources)
             .HasForeignKey(s => s.AiHistoryId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Faq).WithMany()
             .HasForeignKey(s => s.FaqId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
```

---

### 1.3 リポジトリインターフェース定義

```csharp
// FaqApp.Application/Interfaces/IFaqRepository.cs
public interface IFaqRepository
{
    Task<(List<Faq> Items, int Total)> SearchAsync(
        FaqSearchQuery query, bool includeUnpublished, CancellationToken ct = default);
    Task<Faq?>  GetByIdAsync(int id, bool includeUnpublished, CancellationToken ct = default);
    Task        AddAsync(Faq faq, CancellationToken ct = default);
    Task        UpdateTagsAsync(Faq faq, IEnumerable<int> newTagIds, CancellationToken ct = default);
    Task<bool>  ExistsAsync(int id, CancellationToken ct = default);
    Task        SaveChangesAsync(CancellationToken ct = default);
}

// FaqApp.Application/Interfaces/ICategoryRepository.cs
public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync(CancellationToken ct = default);
    Task<Category?>      GetByIdAsync(int id, CancellationToken ct = default);
    Task                 AddAsync(Category category, CancellationToken ct = default);
    Task<bool>           HasFaqsAsync(int categoryId, CancellationToken ct = default);
    Task<bool>           ExistsAsync(int id, CancellationToken ct = default);
    Task<bool>           NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
    Task                 SaveChangesAsync(CancellationToken ct = default);
}

// FaqApp.Application/Interfaces/ITagRepository.cs
public interface ITagRepository
{
    Task<List<Tag>> GetAllAsync(CancellationToken ct = default);
    Task<Tag?>      GetByIdAsync(int id, CancellationToken ct = default);
    Task            AddAsync(Tag tag, CancellationToken ct = default);
    Task<bool>      ExistsAsync(int id, CancellationToken ct = default);
    Task<bool>      NameExistsAsync(string name, int? excludeId = null, CancellationToken ct = default);
    Task<int>       GetFaqCountAsync(int tagId, CancellationToken ct = default);
    Task            SaveChangesAsync(CancellationToken ct = default);
}

// FaqApp.Application/Interfaces/ISearchHistoryRepository.cs
public interface ISearchHistoryRepository
{
    Task<List<SearchHistory>> GetRecentAsync(
        string? userId, string? sessionId, int limit = 10, CancellationToken ct = default);
    Task UpsertAsync(string keyword, string? userId, string? sessionId, CancellationToken ct = default);
}

// FaqApp.Application/Interfaces/IAiHistoryRepository.cs【フェーズ5】
public interface IAiHistoryRepository
{
    Task<AiHistory?>              GetByIdAsync(int id, CancellationToken ct = default);
    Task<(List<AiHistory>, int)>  SearchAsync(AiHistorySearchQuery query, CancellationToken ct = default);
    Task                          AddAsync(AiHistory history, CancellationToken ct = default);
    Task                          SaveChangesAsync(CancellationToken ct = default);
}

// FaqApp.Application/Interfaces/IAiApiClient.cs【フェーズ3〜】
public interface IAiApiClient
{
    Task<string> GenerateSummaryAsync(
        string question,
        IEnumerable<string> faqContents,
        CancellationToken ct = default);
}
```

---

### 1.4 リポジトリ実装

#### FaqRepository.cs

```csharp
// FaqApp.Infrastructure/Repositories/FaqRepository.cs
public class FaqRepository(AppDbContext ctx) : IFaqRepository
{
    // ── 検索（フェーズ1：LIKE / フェーズ2：FTS + スコア） ──
    public async Task<(List<Faq> Items, int Total)> SearchAsync(
        FaqSearchQuery q, bool includeUnpublished, CancellationToken ct = default)
    {
        var query = ctx.Faqs
            .Include(f => f.Category)
            .Include(f => f.FaqTags).ThenInclude(ft => ft.Tag)
            .AsQueryable();

        if (!includeUnpublished)
            query = query.Where(f => f.IsPublished);

        if (q.CategoryId.HasValue)
            query = query.Where(f => f.CategoryId == q.CategoryId);

        if (q.TagId.HasValue)
            query = query.Where(f => f.FaqTags.Any(ft => ft.TagId == q.TagId));

        // フェーズ1：LIKE検索（開発環境フォールバック含む）
        // フェーズ2以降：FTS（EF.Functions.FreeText or 生SQL）
        if (!string.IsNullOrWhiteSpace(q.Keyword))
        {
            // キーワードをスペース区切りで分割しAND検索
            var keywords = q.Keyword.Split(' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            foreach (var kw in keywords)
            {
                var k = kw; // ラムダキャプチャ用ローカル変数
                query = query.Where(f =>
                    f.Title.Contains(k) || f.Body.Contains(k) ||
                    f.Category.Name.Contains(k) ||
                    f.FaqTags.Any(ft => ft.Tag.Name.Contains(k)));
            }
        }

        var total = await query.CountAsync(ct);

        // ソート
        query = q.Sort == "score" && !string.IsNullOrWhiteSpace(q.Keyword)
            ? query  // スコアソートはアプリ層で再ソート（FTS利用時は生SQLに差替）
            : query.OrderByDescending(f => f.UpdatedAt ?? f.CreatedAt);

        var items = await query
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<Faq?> GetByIdAsync(int id, bool includeUnpublished, CancellationToken ct = default)
    {
        var query = ctx.Faqs
            .Include(f => f.Category)
            .Include(f => f.FaqTags).ThenInclude(ft => ft.Tag)
            .Where(f => f.Id == id);

        if (!includeUnpublished)
            query = query.Where(f => f.IsPublished);

        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(Faq faq, CancellationToken ct = default)
        => await ctx.Faqs.AddAsync(faq, ct);

    public async Task UpdateTagsAsync(Faq faq, IEnumerable<int> newTagIds, CancellationToken ct = default)
    {
        // 既存タグを削除してから新しいタグを追加
        var existing = ctx.FaqTags.Where(ft => ft.FaqId == faq.Id);
        ctx.FaqTags.RemoveRange(existing);
        foreach (var tagId in newTagIds)
            await ctx.FaqTags.AddAsync(new FaqTag { FaqId = faq.Id, TagId = tagId }, ct);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
        => await ctx.Faqs.AnyAsync(f => f.Id == id, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await ctx.SaveChangesAsync(ct);
}
```

#### SearchHistoryRepository.cs

```csharp
// FaqApp.Infrastructure/Repositories/SearchHistoryRepository.cs
public class SearchHistoryRepository(AppDbContext ctx) : ISearchHistoryRepository
{
    public async Task<List<SearchHistory>> GetRecentAsync(
        string? userId, string? sessionId, int limit = 10, CancellationToken ct = default)
    {
        var query = ctx.SearchHistories.AsQueryable();

        if (userId != null)
            query = query.Where(s => s.UserId == userId);
        else if (sessionId != null)
            query = query.Where(s => s.SessionId == sessionId);
        else
            return [];

        return await query
            .OrderByDescending(s => s.SearchedAt)
            .Take(limit)
            .ToListAsync(ct);
    }

    // 同一ユーザー・同一キーワードはUpsert（SearchedAtを更新）
    public async Task UpsertAsync(
        string keyword, string? userId, string? sessionId, CancellationToken ct = default)
    {
        SearchHistory? existing = null;

        if (userId != null)
            existing = await ctx.SearchHistories
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Keyword == keyword, ct);
        else if (sessionId != null)
            existing = await ctx.SearchHistories
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.Keyword == keyword, ct);

        if (existing is not null)
            existing.SearchedAt = DateTime.UtcNow;
        else
            await ctx.SearchHistories.AddAsync(
                SearchHistory.Create(keyword, userId, sessionId), ct);

        await ctx.SaveChangesAsync(ct);
    }
}
```

---

### 1.5 サービスクラス詳細

#### FaqService.cs

```csharp
// FaqApp.Application/Services/FaqService.cs
public class FaqService(
    IFaqRepository      faqRepo,
    ICategoryRepository catRepo,
    ITagRepository      tagRepo)
{
    // ── 一覧取得 ──────────────────────────────────────────
    public async Task<PagedResult<FaqSummaryDto>> GetListAsync(
        FaqSearchQuery query, bool includeUnpublished, CancellationToken ct = default)
    {
        var (items, total) = await faqRepo.SearchAsync(query, includeUnpublished, ct);
        var dtos = items.Select(FaqSummaryDto.From).ToList();

        // フェーズ2：スコアリング（sort=scoreの場合）
        if (query.Sort == "score" && !string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keywords = query.Keyword.Split(' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            dtos = dtos
                .Select(dto => { dto.Score = CalculateScore(dto, keywords); return dto; })
                .OrderByDescending(dto => dto.Score)
                .ToList();
        }

        // フェーズ2：ハイライト
        if (query.Highlight && !string.IsNullOrWhiteSpace(query.Keyword))
        {
            var keywords = query.Keyword.Split(' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var dto in dtos)
                dto.TitleHighlighted = ApplyHighlight(dto.Title, keywords);
        }

        return new PagedResult<FaqSummaryDto>(total, query.Page, query.PageSize, dtos);
    }

    // ── 詳細取得（ViewCount++） ───────────────────────────
    public async Task<FaqDetailDto?> GetByIdAsync(
        int id, bool includeUnpublished, CancellationToken ct = default)
    {
        var faq = await faqRepo.GetByIdAsync(id, includeUnpublished, ct);
        if (faq is null) return null;
        faq.IncrementViewCount();
        await faqRepo.SaveChangesAsync(ct);
        return FaqDetailDto.From(faq);
    }

    // ── 登録 ──────────────────────────────────────────────
    public async Task<FaqDetailDto> CreateAsync(
        FaqCreateRequest req, CancellationToken ct = default)
    {
        await ValidateCategoryAndTagsAsync(req.CategoryId, req.TagIds, ct);
        var faq = Faq.Create(req.Title, req.Body, req.CategoryId, req.IsPublished);
        foreach (var tagId in req.TagIds ?? [])
            faq.FaqTags.Add(new FaqTag { TagId = tagId });
        await faqRepo.AddAsync(faq, ct);
        await faqRepo.SaveChangesAsync(ct);
        return FaqDetailDto.From(faq);
    }

    // ── 更新 ──────────────────────────────────────────────
    public async Task<FaqDetailDto?> UpdateAsync(
        int id, FaqUpdateRequest req, CancellationToken ct = default)
    {
        var faq = await faqRepo.GetByIdAsync(id, includeUnpublished: true, ct);
        if (faq is null) return null;
        await ValidateCategoryAndTagsAsync(req.CategoryId, req.TagIds, ct);
        faq.Update(req.Title, req.Body, req.CategoryId, req.IsPublished);
        await faqRepo.UpdateTagsAsync(faq, req.TagIds ?? [], ct);
        await faqRepo.SaveChangesAsync(ct);
        return FaqDetailDto.From(faq);
    }

    // ── 論理削除 ──────────────────────────────────────────
    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var faq = await faqRepo.GetByIdAsync(id, includeUnpublished: true, ct);
        if (faq is null) return false;
        faq.Delete();
        await faqRepo.SaveChangesAsync(ct);
        return true;
    }

    // ── スコア計算（フェーズ2） ───────────────────────────
    private static float CalculateScore(FaqSummaryDto dto, string[] keywords)
    {
        float score = 0;
        foreach (var kw in keywords)
        {
            var k = kw.ToLower();
            if (dto.Title.ToLower() == k)               score += 1.0f;
            else if (dto.Title.ToLower().Contains(k))   score += 0.8f;
            if (dto.Tags.Any(t => t.Name.ToLower().Contains(k))) score += 0.7f;
            if (dto.CategoryName.ToLower().Contains(k)) score += 0.5f;
        }
        return score;
    }

    // ── ハイライト適用（フェーズ2） ──────────────────────
    private static string ApplyHighlight(string text, string[] keywords)
    {
        foreach (var kw in keywords)
            text = text.Replace(kw, $"**{kw}**",
                StringComparison.OrdinalIgnoreCase);
        return text;
    }

    private async Task ValidateCategoryAndTagsAsync(
        int categoryId, IEnumerable<int>? tagIds, CancellationToken ct)
    {
        if (!await catRepo.ExistsAsync(categoryId, ct))
            throw new ValidationException("指定されたカテゴリが存在しません");

        foreach (var tagId in tagIds ?? [])
            if (!await tagRepo.ExistsAsync(tagId, ct))
                throw new ValidationException($"指定されたタグ（ID:{tagId}）が存在しません");
    }
}
```

#### AuthService.cs

```csharp
// FaqApp.Application/Services/AuthService.cs
public class AuthService(
    UserManager<ApplicationUser>    userManager,
    IOptions<JwtSettings>           jwtOptions)
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<LoginResponse?> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(req.Email);
        if (user is null) return null;

        // アカウント無効チェック
        if (!user.IsActive)
            throw new ForbiddenException("このアカウントは無効化されています。管理者にお問い合わせください");

        // パスワード照合
        if (!await userManager.CheckPasswordAsync(user, req.Password))
            return null;

        // ロール取得
        var roles = await userManager.GetRolesAsync(user);
        var role  = roles.FirstOrDefault() ?? "User";

        // JWT生成
        var token = GenerateJwt(user, role);
        return new LoginResponse(token, _jwt.ExpiresInMinutes * 60, role, user.DisplayName);
    }

    private string GenerateJwt(ApplicationUser user, string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,  user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Role,              role),
            new Claim("displayName",                user.DisplayName),
        };
        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer:   _jwt.Issuer,
            audience: _jwt.Audience,
            claims:   claims,
            expires:  DateTime.UtcNow.AddMinutes(_jwt.ExpiresInMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

#### AiService.cs【フェーズ3〜5】

```csharp
// FaqApp.Application/Services/AiService.cs
public class AiService(
    IFaqRepository        faqRepo,
    IAiHistoryRepository  histRepo,
    IAiApiClient          aiClient,
    IOptions<AiSettings>  aiOptions)
{
    private readonly AiSettings _ai = aiOptions.Value;

    public async Task<AiSearchResponse> SearchAsync(
        AiSearchRequest req, string? userId, CancellationToken ct = default)
    {
        // ① キーワード抽出（質問文をそのまま使用。高度化はフェーズ3+で対応）
        var keywords = ExtractKeywords(req.Question);

        // ② FAQ検索
        var (faqs, _) = await faqRepo.SearchAsync(
            new FaqSearchQuery { Keyword = string.Join(" ", keywords),
                                 Page = 1, PageSize = 5 },
            includeUnpublished: false, ct);

        // ③ FAQ0件の場合はAI呼び出しをスキップ
        if (faqs.Count == 0)
        {
            var emptyHistory = AiHistory.CreateSuccess(
                req.Question, string.Join(" ", keywords), "", userId, []);
            emptyHistory.IsSuccess = false; // FAQ0件は「AI未呼び出し」として記録
            await histRepo.AddAsync(emptyHistory, ct);
            await histRepo.SaveChangesAsync(ct);

            return new AiSearchResponse(
                null, null, [],
                "該当するFAQが見つかりませんでした。管理者にご相談ください。",
                emptyHistory.Id);
        }

        // ④ コンテキスト生成
        var faqContents = faqs.Select((f, i) =>
            $"[FAQ{i + 1}] タイトル：{f.Title}\n本文：{f.Body}");

        // ⑤ AI API 呼び出し（タイムアウト制御）
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(_ai.TimeoutSeconds));

        string answer;
        AiHistory history;
        try
        {
            answer = await aiClient.GenerateSummaryAsync(req.Question, faqContents, cts.Token);

            var sources = faqs.Select((f, i) => (f.Id, Score: 0.9f - i * 0.1f, Order: i));
            history = AiHistory.CreateSuccess(
                req.Question, string.Join(" ", keywords), answer, userId, sources);
        }
        catch (OperationCanceledException)
        {
            history = AiHistory.CreateFailure(req.Question, userId, "AI APIタイムアウト");
            await histRepo.AddAsync(history, ct);
            await histRepo.SaveChangesAsync(ct);
            throw new TimeoutException("AI回答の生成がタイムアウトしました");
        }
        catch (Exception ex)
        {
            history = AiHistory.CreateFailure(req.Question, userId, ex.Message);
            await histRepo.AddAsync(history, ct);
            await histRepo.SaveChangesAsync(ct);
            throw;
        }

        await histRepo.AddAsync(history, ct);
        await histRepo.SaveChangesAsync(ct);

        const string disclaimer =
            "この回答はFAQをもとに生成されています。必ず参照元を確認してください。";

        var sources2 = faqs.Select(f => new AiSourceDto(f.Id, f.Title, $"/faqs/{f.Id}"));
        return new AiSearchResponse(answer, disclaimer, sources2.ToList(), null, history.Id);
    }

    public async Task SetFeedbackAsync(int historyId, bool isHelpful, CancellationToken ct = default)
    {
        var history = await histRepo.GetByIdAsync(historyId, ct)
            ?? throw new NotFoundException($"AI実行履歴（ID:{historyId}）が存在しません");
        history.SetFeedback(isHelpful);
        await histRepo.SaveChangesAsync(ct);
    }

    // キーワード抽出（シンプル実装：2文字以上の単語を抽出）
    private static string[] ExtractKeywords(string question)
        => question
            .Split(new[] { ' ', '　', '、', '。', '？', '!' },
                   StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length >= 2)
            .Distinct()
            .ToArray();
}
```

#### UserService.cs【フェーズ6】

```csharp
// FaqApp.Application/Services/UserService.cs
public class UserService(UserManager<ApplicationUser> userManager)
{
    public async Task<PagedResult<UserDto>> GetListAsync(
        UserSearchQuery query, CancellationToken ct = default)
    {
        var users = userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            users = users.Where(u =>
                u.DisplayName.Contains(query.Keyword) ||
                (u.Email != null && u.Email.Contains(query.Keyword)));

        if (query.IsActive.HasValue)
            users = users.Where(u => u.IsActive == query.IsActive);

        var total = await users.CountAsync(ct);
        var items = await users
            .OrderBy(u => u.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        // ロール情報を付与
        var dtos = new List<UserDto>();
        foreach (var user in items)
        {
            var roles = await userManager.GetRolesAsync(user);
            dtos.Add(UserDto.From(user, roles.FirstOrDefault() ?? "User"));
        }

        return new PagedResult<UserDto>(total, query.Page, query.PageSize, dtos);
    }

    public async Task ChangeRoleAsync(string targetUserId, string newRole, string requestUserId)
    {
        if (targetUserId == requestUserId)
            throw new ValidationException("自分自身のロールは変更できません");

        var user = await userManager.FindByIdAsync(targetUserId)
            ?? throw new NotFoundException("ユーザーが存在しません");

        var currentRoles = await userManager.GetRolesAsync(user);
        await userManager.RemoveFromRolesAsync(user, currentRoles);
        await userManager.AddToRoleAsync(user, newRole);
        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
    }

    public async Task ChangeStatusAsync(string targetUserId, bool isActive, string requestUserId)
    {
        if (targetUserId == requestUserId && !isActive)
            throw new ValidationException("自分自身を無効化することはできません");

        var user = await userManager.FindByIdAsync(targetUserId)
            ?? throw new NotFoundException("ユーザーが存在しません");

        user.IsActive  = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);
    }
}
```

---

### 1.6 コントローラー詳細

#### FaqsController.cs

```csharp
// FaqApp.Api/Controllers/FaqsController.cs
[ApiController]
[Route("api/v1/faqs")]
public class FaqsController(FaqService faqService, SearchHistoryService histService) : ControllerBase
{
    private bool IsAdminOrEditor =>
        User.IsInRole("Admin") || User.IsInRole("Editor");

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] FaqSearchQuery query, CancellationToken ct)
    {
        var result = await faqService.GetListAsync(query, IsAdminOrEditor, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var faq = await faqService.GetByIdAsync(id, IsAdminOrEditor, ct);
        return faq is null
            ? NotFound(new { message = "指定されたFAQは存在しません" })
            : Ok(faq);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> Create(
        [FromBody] FaqCreateRequest req, CancellationToken ct)
    {
        var created = await faqService.CreateAsync(req, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> Update(
        int id, [FromBody] FaqUpdateRequest req, CancellationToken ct)
    {
        var updated = await faqService.UpdateAsync(id, req, ct);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await faqService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}

// API v2（フェーズ2）
[ApiController]
[Route("api/v2/faqs")]
public class FaqsV2Controller(FaqService faqService) : ControllerBase
{
    private bool IsAdminOrEditor =>
        User.IsInRole("Admin") || User.IsInRole("Editor");

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] FaqSearchQueryV2 query, CancellationToken ct)
    {
        var result = await faqService.GetListAsync(query, IsAdminOrEditor, ct);
        return Ok(result);
    }
}
```

#### AuthController.cs

```csharp
// FaqApp.Api/Controllers/AuthController.cs
[ApiController]
[Route("api/v1/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest req, CancellationToken ct)
    {
        try
        {
            var response = await authService.LoginAsync(req, ct);
            if (response is null)
                return Unauthorized(new { message = "メールアドレスまたはパスワードが正しくありません" });

            // httpOnlyクッキーにセット
            Response.Cookies.Append("accessToken", response.AccessToken, new CookieOptions {
                HttpOnly = true,
                Secure   = true,
                SameSite = SameSiteMode.Strict,
                Expires  = DateTimeOffset.UtcNow.AddSeconds(response.ExpiresIn)
            });
            return Ok(response);
        }
        catch (ForbiddenException ex)
        {
            return StatusCode(403, new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("accessToken");
        return Ok(new { message = "ログアウトしました" });
    }
}
```

#### AiController.cs【フェーズ3〜5】

```csharp
// FaqApp.Api/Controllers/AiController.cs
[ApiController]
[Route("api/v1/ai")]
[Authorize]  // ログイン必須（User以上）
public class AiController(AiService aiService, AiHistoryService histService) : ControllerBase
{
    private string? UserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpPost("search")]
    public async Task<IActionResult> Search(
        [FromBody] AiSearchRequest req, CancellationToken ct)
    {
        try
        {
            var result = await aiService.SearchAsync(req, UserId, ct);
            return Ok(result);
        }
        catch (TimeoutException ex)
        {
            return StatusCode(504, new { message = ex.Message });
        }
    }

    [HttpPost("histories/{id:int}/feedback")]
    public async Task<IActionResult> Feedback(
        int id, [FromBody] AiFeedbackRequest req, CancellationToken ct)
    {
        await aiService.SetFeedbackAsync(id, req.IsHelpful, ct);
        return Ok(new { message = "フィードバックを受け付けました" });
    }

    [HttpGet("histories")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> GetHistories(
        [FromQuery] AiHistorySearchQuery query, CancellationToken ct)
    {
        var result = await histService.GetListAsync(query, ct);
        return Ok(result);
    }

    [HttpGet("histories/{id:int}")]
    [Authorize(Roles = "Admin,Editor")]
    public async Task<IActionResult> GetHistoryDetail(int id, CancellationToken ct)
    {
        var detail = await histService.GetByIdAsync(id, ct);
        return detail is null ? NotFound() : Ok(detail);
    }
}
```

#### UsersController.cs【フェーズ6】

```csharp
// FaqApp.Api/Controllers/UsersController.cs
[ApiController]
[Route("api/v1/users")]
[Authorize(Roles = "Admin")]
public class UsersController(UserService userService) : ControllerBase
{
    private string RequestUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] UserSearchQuery query, CancellationToken ct)
    {
        var result = await userService.GetListAsync(query, ct);
        return Ok(result);
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> ChangeRole(
        string id, [FromBody] ChangeRoleRequest req, CancellationToken ct)
    {
        try
        {
            await userService.ChangeRoleAsync(id, req.Role, RequestUserId);
            return Ok(new { message = "ロールを変更しました" });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> ChangeStatus(
        string id, [FromBody] ChangeStatusRequest req, CancellationToken ct)
    {
        try
        {
            await userService.ChangeStatusAsync(id, req.IsActive, RequestUserId);
            return Ok(new { message = "ステータスを変更しました" });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
```

---

### 1.7 DTO クラス定義

```csharp
// FaqApp.Application/DTOs/FaqDto.cs

// ── クエリパラメータ ──────────────────────────────────
public record FaqSearchQuery
{
    public string? Keyword    { get; init; }
    public int?    CategoryId { get; init; }
    public int?    TagId      { get; init; }
    public int     Page       { get; init; } = 1;
    public int     PageSize   { get; init; } = 20;
    public string  Sort       { get; init; } = "updatedAt"; // "score" or "updatedAt"
    public bool    Highlight  { get; init; } = false;       // フェーズ2
}

// ── リクエストボディ ──────────────────────────────────
public record FaqCreateRequest(
    [Required][MaxLength(100)] string Title,
    [Required]                 string Body,
    [Required]                 int    CategoryId,
    IEnumerable<int>?                 TagIds,
    bool                              IsPublished = false
);

public record FaqUpdateRequest(
    [Required][MaxLength(100)] string Title,
    [Required]                 string Body,
    [Required]                 int    CategoryId,
    IEnumerable<int>?                 TagIds,
    bool                              IsPublished = false
);

// ── レスポンス ────────────────────────────────────────
public class FaqSummaryDto
{
    public int          Id               { get; set; }
    public string       Title            { get; set; } = "";
    public string?      TitleHighlighted { get; set; }  // フェーズ2
    public string?      BodyExcerpt      { get; set; }  // フェーズ2（マッチ箇所抜粋）
    public int          CategoryId       { get; set; }
    public string       CategoryName     { get; set; } = "";
    public List<TagDto> Tags             { get; set; } = [];
    public bool         IsPublished      { get; set; }
    public int          ViewCount        { get; set; }
    public float        Score            { get; set; }  // フェーズ2
    public DateTime?    UpdatedAt        { get; set; }

    public static FaqSummaryDto From(Faq faq) => new()
    {
        Id           = faq.Id,
        Title        = faq.Title,
        CategoryId   = faq.CategoryId,
        CategoryName = faq.Category.Name,
        Tags         = faq.FaqTags.Select(ft => TagDto.From(ft.Tag)).ToList(),
        IsPublished  = faq.IsPublished,
        ViewCount    = faq.ViewCount,
        UpdatedAt    = faq.UpdatedAt,
    };
}

public class FaqDetailDto : FaqSummaryDto
{
    public string   Body            { get; set; } = "";
    public int      HelpfulCount    { get; set; }   // 余力
    public int      NotHelpfulCount { get; set; }   // 余力
    public DateTime CreatedAt       { get; set; }

    public new static FaqDetailDto From(Faq faq) => new()
    {
        Id              = faq.Id,
        Title           = faq.Title,
        Body            = faq.Body,
        CategoryId      = faq.CategoryId,
        CategoryName    = faq.Category.Name,
        Tags            = faq.FaqTags.Select(ft => TagDto.From(ft.Tag)).ToList(),
        IsPublished     = faq.IsPublished,
        ViewCount       = faq.ViewCount,
        HelpfulCount    = faq.HelpfulCount,
        NotHelpfulCount = faq.NotHelpfulCount,
        CreatedAt       = faq.CreatedAt,
        UpdatedAt       = faq.UpdatedAt,
    };
}

// ── ページネーション共通 ──────────────────────────────
public record PagedResult<T>(int Total, int Page, int PageSize, List<T> Items);

// FaqApp.Application/DTOs/AuthDto.cs
public record LoginRequest(
    [Required][EmailAddress] string Email,
    [Required]               string Password
);
public record LoginResponse(
    string AccessToken, int ExpiresIn, string Role, string DisplayName);

// FaqApp.Application/DTOs/AiDto.cs【フェーズ3〜】
public record AiSearchRequest(
    [Required][MaxLength(500)] string Question
);
public record AiSourceDto(int Id, string Title, string Url);
public record AiSearchResponse(
    string?          Answer,
    string?          Disclaimer,
    List<AiSourceDto> Sources,
    string?          Message,
    int              AiHistoryId
);
public record AiFeedbackRequest([Required] bool IsHelpful);

// FaqApp.Application/DTOs/UserDto.cs【フェーズ6】
public class UserDto
{
    public string   Id          { get; set; } = "";
    public string   DisplayName { get; set; } = "";
    public string   Email       { get; set; } = "";
    public string   Role        { get; set; } = "";
    public bool     IsActive    { get; set; }
    public DateTime CreatedAt   { get; set; }

    public static UserDto From(ApplicationUser user, string role) => new()
    {
        Id          = user.Id,
        DisplayName = user.DisplayName,
        Email       = user.Email ?? "",
        Role        = role,
        IsActive    = user.IsActive,
        CreatedAt   = user.CreatedAt,
    };
}
public record ChangeRoleRequest(
    [Required] string Role   // "User" | "Editor" | "Admin"
);
public record ChangeStatusRequest([Required] bool IsActive);
```

---

### 1.8 バリデーション実装

```csharp
// FaqApp.Application/Validators/FaqCreateRequestValidator.cs
public class FaqCreateRequestValidator : AbstractValidator<FaqCreateRequest>
{
    public FaqCreateRequestValidator(ICategoryRepository catRepo, ITagRepository tagRepo)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("タイトルは必須です")
            .MaximumLength(100).WithMessage("タイトルは100文字以内で入力してください");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("本文は必須です");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("カテゴリを選択してください")
            .MustAsync(async (id, ct) => await catRepo.ExistsAsync(id, ct))
            .WithMessage("指定されたカテゴリが存在しません");

        RuleForEach(x => x.TagIds)
            .MustAsync(async (id, ct) => await tagRepo.ExistsAsync(id, ct))
            .WithMessage("指定されたタグが存在しません");
    }
}

// FaqApp.Application/Validators/AiQuestionRequestValidator.cs【フェーズ3〜】
public class AiQuestionRequestValidator : AbstractValidator<AiSearchRequest>
{
    public AiQuestionRequestValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("質問文は必須です")
            .MaximumLength(500).WithMessage("質問文は500文字以内で入力してください");
    }
}
```

---

### 1.9 AI APIクライアント実装【フェーズ3〜】

```csharp
// FaqApp.Infrastructure/ExternalServices/AiApiClient.cs
public class AiApiClient(IOptions<AiSettings> options, ILogger<AiApiClient> logger) : IAiApiClient
{
    private readonly AiSettings _settings = options.Value;

    public async Task<string> GenerateSummaryAsync(
        string question, IEnumerable<string> faqContents, CancellationToken ct = default)
    {
        var systemPrompt = BuildSystemPrompt();
        var userMessage  = BuildUserMessage(question, faqContents);

        logger.LogInformation("AI API呼び出し開始 Provider={Provider}", _settings.Provider);

        return _settings.Provider switch {
            "AzureOpenAI" => await CallAzureOpenAIAsync(systemPrompt, userMessage, ct),
            "Claude"      => await CallClaudeAsync(systemPrompt, userMessage, ct),
            _ => throw new InvalidOperationException($"不明なAIプロバイダー: {_settings.Provider}")
        };
    }

    private static string BuildSystemPrompt() => """
        あなたは社内FAQ検索システムのアシスタントです。以下のルールを厳守してください。

        【許可する回答】
        - 提供されたFAQの内容をもとに、利用者向けにわかりやすく要約した回答
        - FAQに記載された手順・確認方法の説明

        【禁止する回答】
        - 提供されたFAQに記載のない情報の断言
        - FAQにない手順・操作方法の独自生成
        - 問い合わせ先・責任者・担当部署の推測や断言
        - 機密情報・個人情報を含む内容の生成
        - 「必ずしてください」「絶対に〜です」等の断定的な保証表現

        【回答の末尾】
        必ず「詳細は参照元FAQをご確認ください。」を付記してください。
        """;

    private static string BuildUserMessage(string question, IEnumerable<string> faqContents)
    {
        var context = string.Join("\n\n", faqContents);
        return $"【FAQコンテキスト】\n{context}\n\n【ユーザーの質問】\n{question}";
    }

    private async Task<string> CallAzureOpenAIAsync(
        string systemPrompt, string userMessage, CancellationToken ct)
    {
        var az  = _settings.AzureOpenAI;
        var client = new OpenAIClient(
            new Uri(az.Endpoint),
            new AzureKeyCredential(az.ApiKey));

        var options = new ChatCompletionsOptions(az.DeploymentName, [
            new ChatRequestSystemMessage(systemPrompt),
            new ChatRequestUserMessage(userMessage),
        ]) { MaxTokens = 1000 };

        var response = await client.GetChatCompletionsAsync(options, ct);
        return response.Value.Choices[0].Message.Content;
    }

    private async Task<string> CallClaudeAsync(
        string systemPrompt, string userMessage, CancellationToken ct)
    {
        using var http = new HttpClient();
        http.DefaultRequestHeaders.Add("x-api-key", _settings.Claude.ApiKey);
        http.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");

        var body = new {
            model      = "claude-sonnet-4-20250514",
            max_tokens = 1000,
            system     = systemPrompt,
            messages   = new[] { new { role = "user", content = userMessage } }
        };

        var res  = await http.PostAsJsonAsync("https://api.anthropic.com/v1/messages", body, ct);
        res.EnsureSuccessStatusCode();
        var json = await res.Content.ReadFromJsonAsync<ClaudeResponse>(ct);
        return json?.Content?[0]?.Text ?? throw new InvalidOperationException("Claude応答が空です");
    }

    private record ClaudeResponse(ClaudeContent[]? Content);
    private record ClaudeContent(string? Text);
}
```

---

### 1.10 Program.cs 設定

```csharp
// FaqApp.Api/Program.cs
var builder = WebApplication.CreateBuilder(args);

// ── DB ────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure(3)));

// ── Identity（ApplicationUser） ───────────────────────
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(opt => {
    opt.Password.RequireDigit           = true;
    opt.Password.RequiredLength         = 8;
    opt.Password.RequireNonAlphanumeric = false;
    opt.Lockout.MaxFailedAccessAttempts = 5;
    opt.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// ── JWT認証 ───────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.AddAuthentication(opt => {
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(opt => {
    opt.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSettings.Issuer,
        ValidAudience            = jwtSettings.Audience,
        IssuerSigningKey         = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Secret)),
    };
    // クッキーからJWTを取得
    opt.Events = new JwtBearerEvents {
        OnMessageReceived = ctx => {
            ctx.Token = ctx.Request.Cookies["accessToken"];
            return Task.CompletedTask;
        }
    };
});

// ── CORS ─────────────────────────────────────────────
var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(',') ?? [];
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p => p.WithOrigins(allowedOrigins)
        .AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

// ── DI登録 ───────────────────────────────────────────
builder.Services.AddScoped<IFaqRepository,           FaqRepository>();
builder.Services.AddScoped<ICategoryRepository,      CategoryRepository>();
builder.Services.AddScoped<ITagRepository,           TagRepository>();
builder.Services.AddScoped<ISearchHistoryRepository, SearchHistoryRepository>();
builder.Services.AddScoped<IAiHistoryRepository,     AiHistoryRepository>();   // フェーズ5
builder.Services.AddScoped<IAiApiClient,             AiApiClient>();           // フェーズ3
builder.Services.AddScoped<FaqService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<TagService>();
builder.Services.AddScoped<SearchHistoryService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AiService>();         // フェーズ3
builder.Services.AddScoped<AiHistoryService>();  // フェーズ5
builder.Services.AddScoped<UserService>();       // フェーズ6

// ── AI設定 ───────────────────────────────────────────
builder.Services.Configure<AiSettings>(builder.Configuration.GetSection("AiSettings"));

// ── FluentValidation ─────────────────────────────────
builder.Services.AddValidatorsFromAssemblyContaining<FaqCreateRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();

// ── メモリキャッシュ（カテゴリ・タグ） ───────────────
builder.Services.AddMemoryCache();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Seeder実行 ────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await RoleSeeder.SeedAsync(services);
    await CategorySeeder.SeedAsync(services);
    await AdminUserSeeder.SeedAsync(services, app.Configuration);
}

if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }

app.UseExceptionHandler(errApp => errApp.Run(async ctx => {
    ctx.Response.StatusCode  = 500;
    ctx.Response.ContentType = "application/json";
    await ctx.Response.WriteAsJsonAsync(
        new { message = "サーバーエラーが発生しました" });
}));

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

---

### 1.11 Seeder 実装

```csharp
// FaqApp.Infrastructure/Data/Seed/RoleSeeder.cs
public static class RoleSeeder
{
    // フェーズ1：User, Admin / フェーズ6：Editor を追加
    private static readonly string[] Roles = ["User", "Admin", "Editor"];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Roles)
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
    }
}

// FaqApp.Infrastructure/Data/Seed/AdminUserSeeder.cs
public static class AdminUserSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var email       = config["AdminSeed:Email"] ?? "admin@faq-app.local";
        var password    = config["AdminSeed:Password"] ?? throw new InvalidOperationException("AdminSeed:Password未設定");
        var displayName = config["AdminSeed:DisplayName"] ?? "システム管理者";

        if (await userManager.FindByEmailAsync(email) is not null) return;

        var admin = new ApplicationUser {
            UserName    = email,
            Email       = email,
            DisplayName = displayName,
            IsActive    = true,
            CreatedAt   = DateTime.UtcNow
        };
        var result = await userManager.CreateAsync(admin, password);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(admin, "Admin");
    }
}

// FaqApp.Infrastructure/Data/Seed/CategorySeeder.cs
public static class CategorySeeder
{
    private static readonly (string Name, int Order)[] Categories =
    [
        ("請求処理", 1), ("CSV取込", 2), ("ログイン障害", 3),
        ("API障害", 4),  ("PDF出力", 5), ("月次締め", 6),
        ("ユーザー権限", 7), ("メール通知", 8), ("システム設定", 9)
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var ctx = services.GetRequiredService<AppDbContext>();
        if (await ctx.Categories.AnyAsync()) return;

        foreach (var (name, order) in Categories)
            ctx.Categories.Add(Category.Create(name, order));
        await ctx.SaveChangesAsync();
    }
}
```

---

## 2. フロントエンド詳細設計

### 2.1 型定義

```typescript
// src/types/index.ts

// ── FAQ ─────────────────────────────────────────────
export type TagItem = { id: number; name: string };

export type FaqSummary = {
  id:               number;
  title:            string;
  titleHighlighted?: string;   // フェーズ2
  bodyExcerpt?:     string;    // フェーズ2
  categoryId:       number;
  categoryName:     string;
  tags:             TagItem[];
  isPublished:      boolean;
  viewCount:        number;
  score?:           number;    // フェーズ2
  updatedAt:        string;
};

export type FaqDetail = FaqSummary & {
  body:             string;
  helpfulCount:     number;    // 余力
  notHelpfulCount:  number;    // 余力
  createdAt:        string;
};

export type FaqSearchParams = {
  keyword?:    string;
  categoryId?: number;
  tagId?:      number;
  page?:       number;
  pageSize?:   number;
  sort?:       'updatedAt' | 'score';  // フェーズ2
  highlight?:  boolean;               // フェーズ2
};

export type FaqCreateInput = {
  title:       string;
  body:        string;
  categoryId:  number;
  tagIds?:     number[];
  isPublished: boolean;
};

// ── カテゴリ・タグ ────────────────────────────────────
export type Category = { id: number; name: string; displayOrder: number };

// ── ページネーション ──────────────────────────────────
export type PagedResult<T> = {
  total:    number;
  page:     number;
  pageSize: number;
  items:    T[];
};

// ── 認証 ─────────────────────────────────────────────
export type AuthUser = {
  displayName: string;
  role:        'User' | 'Editor' | 'Admin';
};

// ── AI（フェーズ3〜） ─────────────────────────────────
export type AiSource = { id: number; title: string; url: string };
export type AiSearchResult = {
  answer?:     string;
  disclaimer?: string;
  sources:     AiSource[];
  message?:    string;
  aiHistoryId: number;
};

// ── AI実行履歴（フェーズ5） ────────────────────────────
export type AiHistorySummary = {
  id:          number;
  question:    string;
  displayName: string;
  isSuccess:   boolean;
  isHelpful:   boolean | null;
  executedAt:  string;
};
export type AiHistoryDetail = AiHistorySummary & {
  searchKeywords?: string;
  aiAnswer?:       string;
  errorMessage?:   string;
  sources:         { faqId: number; title: string; score: number; displayOrder: number }[];
};

// ── ユーザー（フェーズ6） ─────────────────────────────
export type UserItem = {
  id:          string;
  displayName: string;
  email:       string;
  role:        'User' | 'Editor' | 'Admin';
  isActive:    boolean;
  createdAt:   string;
};
```

---

### 2.2 APIクライアント実装

```typescript
// src/lib/api.ts
const BASE_V1 = `${process.env.NEXT_PUBLIC_API_BASE_URL}/api/v1`;
const BASE_V2 = `${process.env.NEXT_PUBLIC_API_BASE_URL}/api/v2`;

type FetchOptions = RequestInit & { version?: 'v1' | 'v2' };

async function apiFetch<T>(
  path: string,
  options: FetchOptions = {}
): Promise<T> {
  const { version = 'v1', ...init } = options;
  const base = version === 'v2' ? BASE_V2 : BASE_V1;

  const res = await fetch(`${base}${path}`, {
    ...init,
    credentials: 'include',          // httpOnlyクッキーを自動送信
    headers: { 'Content-Type': 'application/json', ...init.headers },
  });

  if (!res.ok) {
    const body = await res.json().catch(() => ({}));
    throw new ApiError(res.status, body?.message ?? 'エラーが発生しました', body?.errors);
  }

  if (res.status === 204) return undefined as T;
  return res.json();
}

export class ApiError extends Error {
  constructor(
    public status: number,
    message: string,
    public errors?: Record<string, string[]>
  ) { super(message); }
}

// ── FAQ API ──────────────────────────────────────────
export const faqApi = {
  list: (params: FaqSearchParams, v2 = false) =>
    apiFetch<PagedResult<FaqSummary>>(
      `/faqs?${new URLSearchParams(params as Record<string, string>).toString()}`,
      { version: v2 ? 'v2' : 'v1' }
    ),

  get: (id: number) =>
    apiFetch<FaqDetail>(`/faqs/${id}`),

  create: (data: FaqCreateInput) =>
    apiFetch<FaqDetail>('/faqs', {
      method: 'POST', body: JSON.stringify(data) }),

  update: (id: number, data: FaqCreateInput) =>
    apiFetch<FaqDetail>(`/faqs/${id}`, {
      method: 'PUT', body: JSON.stringify(data) }),

  delete: (id: number) =>
    apiFetch<void>(`/faqs/${id}`, { method: 'DELETE' }),
};

// ── Auth API ─────────────────────────────────────────
export const authApi = {
  login: (email: string, password: string) =>
    apiFetch<{ accessToken: string; expiresIn: number; role: string; displayName: string }>(
      '/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) }),

  logout: () =>
    apiFetch<{ message: string }>('/auth/logout', { method: 'POST' }),
};

// ── AI API（フェーズ3〜） ────────────────────────────
export const aiApi = {
  search: (question: string) =>
    apiFetch<AiSearchResult>('/ai/search', {
      method: 'POST', body: JSON.stringify({ question }) }),

  feedback: (historyId: number, isHelpful: boolean) =>
    apiFetch<{ message: string }>(`/ai/histories/${historyId}/feedback`, {
      method: 'POST', body: JSON.stringify({ isHelpful }) }),

  getHistories: (params: Record<string, string>) =>
    apiFetch<PagedResult<AiHistorySummary>>(
      `/ai/histories?${new URLSearchParams(params).toString()}`),

  getHistoryDetail: (id: number) =>
    apiFetch<AiHistoryDetail>(`/ai/histories/${id}`),
};

// ── User API（フェーズ6） ────────────────────────────
export const userApi = {
  list: (params: Record<string, string>) =>
    apiFetch<PagedResult<UserItem>>(
      `/users?${new URLSearchParams(params).toString()}`),

  changeRole: (id: string, role: string) =>
    apiFetch<{ message: string }>(`/users/${id}/role`, {
      method: 'PUT', body: JSON.stringify({ role }) }),

  changeStatus: (id: string, isActive: boolean) =>
    apiFetch<{ message: string }>(`/users/${id}/status`, {
      method: 'PUT', body: JSON.stringify({ isActive }) }),
};
```

---

### 2.3 主要コンポーネント実装

#### SearchBar.tsx

```typescript
// src/components/faq/SearchBar.tsx
'use client';
import { useState, useEffect, useRef } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useDebouncedCallback } from 'use-debounce';

const HISTORY_KEY = 'faq_search_history';
const MAX_HISTORY  = 10;

export function SearchBar() {
  const router        = useRouter();
  const searchParams  = useSearchParams();
  const [value, setValue]       = useState(searchParams.get('keyword') ?? '');
  const [history, setHistory]   = useState<string[]>([]);
  const [showDrop, setShowDrop] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);

  // sessionStorageから履歴を読み込み
  useEffect(() => {
    const stored = sessionStorage.getItem(HISTORY_KEY);
    if (stored) setHistory(JSON.parse(stored));
  }, []);

  const navigate = useDebouncedCallback((kw: string) => {
    const params = new URLSearchParams(searchParams.toString());
    kw ? params.set('keyword', kw) : params.delete('keyword');
    params.set('page', '1');
    router.push(`/faqs?${params.toString()}`);

    // 検索履歴を更新
    if (kw.trim()) {
      const next = [kw, ...history.filter(h => h !== kw)].slice(0, MAX_HISTORY);
      setHistory(next);
      sessionStorage.setItem(HISTORY_KEY, JSON.stringify(next));
    }
  }, 500);

  return (
    <div className="relative w-full max-w-xl">
      <input
        ref={inputRef}
        type="text"
        value={value}
        placeholder="キーワードで検索（スペース区切りでAND検索）"
        className="w-full border rounded px-4 py-2 focus:outline-none"
        onChange={e => { setValue(e.target.value); navigate(e.target.value); }}
        onFocus={() => setShowDrop(true)}
        onBlur={() => setTimeout(() => setShowDrop(false), 150)}
      />
      {showDrop && history.length > 0 && (
        <ul className="absolute z-10 w-full bg-white border rounded shadow mt-1">
          {history.map(h => (
            <li
              key={h}
              className="px-4 py-2 hover:bg-gray-100 cursor-pointer text-sm"
              onMouseDown={() => { setValue(h); navigate(h); setShowDrop(false); }}
            >
              🕐 {h}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
```

#### FaqForm.tsx

```typescript
// src/components/faq/FaqForm.tsx
'use client';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useRouter } from 'next/navigation';
import { faqApi, ApiError } from '@/lib/api';
import type { FaqDetail, Category, TagItem, FaqCreateInput } from '@/types';

const schema = z.object({
  title:       z.string().min(1, '必須').max(100, '100文字以内'),
  body:        z.string().min(1, '必須'),
  categoryId:  z.number({ required_error: 'カテゴリを選択してください' }).min(1),
  tagIds:      z.number().array().optional(),
  isPublished: z.boolean(),
});

type FormValues = z.infer<typeof schema>;

type Props = {
  initialData?: FaqDetail;
  categories:   Category[];
  tags:         TagItem[];
};

export function FaqForm({ initialData, categories, tags }: Props) {
  const router = useRouter();
  const isEdit = !!initialData;

  const { register, handleSubmit, setValue, watch,
          formState: { errors, isSubmitting } } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      title:       initialData?.title       ?? '',
      body:        initialData?.body        ?? '',
      categoryId:  initialData?.categoryId  ?? 0,
      tagIds:      initialData?.tags.map(t => t.id) ?? [],
      isPublished: initialData?.isPublished ?? false,
    },
  });

  const [apiError, setApiError] = useState<string | null>(null);

  const onSubmit = async (values: FormValues) => {
    setApiError(null);
    try {
      if (isEdit) {
        await faqApi.update(initialData.id, values as FaqCreateInput);
        router.push(`/faqs/${initialData.id}`);
      } else {
        const created = await faqApi.create(values as FaqCreateInput);
        router.push('/faqs');
      }
      router.refresh();
    } catch (e) {
      if (e instanceof ApiError) setApiError(e.message);
    }
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
      {apiError && (
        <div className="bg-red-50 border border-red-300 text-red-700 px-4 py-3 rounded">
          {apiError}
        </div>
      )}

      {/* タイトル */}
      <div>
        <label className="block text-sm font-medium mb-1">タイトル *</label>
        <input {...register('title')}
          className="w-full border rounded px-3 py-2" />
        {errors.title && (
          <p className="text-red-500 text-xs mt-1">{errors.title.message}</p>
        )}
      </div>

      {/* カテゴリ */}
      <div>
        <label className="block text-sm font-medium mb-1">カテゴリ *</label>
        <select {...register('categoryId', { valueAsNumber: true })}
          className="w-full border rounded px-3 py-2">
          <option value={0}>選択してください</option>
          {categories.map(c => (
            <option key={c.id} value={c.id}>{c.name}</option>
          ))}
        </select>
        {errors.categoryId && (
          <p className="text-red-500 text-xs mt-1">{errors.categoryId.message}</p>
        )}
      </div>

      {/* 本文（Markdownエディタ） */}
      <div>
        <label className="block text-sm font-medium mb-1">本文（Markdown） *</label>
        {/* react-md-editor は 'use client' 必須のため動的インポート */}
        <textarea {...register('body')}
          rows={12}
          className="w-full border rounded px-3 py-2 font-mono text-sm" />
        {errors.body && (
          <p className="text-red-500 text-xs mt-1">{errors.body.message}</p>
        )}
      </div>

      {/* 公開状態 */}
      <div className="flex items-center gap-3">
        <input type="checkbox" {...register('isPublished')} id="isPublished" />
        <label htmlFor="isPublished" className="text-sm">公開する</label>
      </div>

      {/* ボタン */}
      <div className="flex gap-3">
        <button type="submit" disabled={isSubmitting}
          className="bg-blue-600 text-white px-6 py-2 rounded disabled:opacity-50">
          {isSubmitting ? '保存中...' : isEdit ? '更新' : '登録'}
        </button>
        <button type="button" onClick={() => router.back()}
          className="border px-6 py-2 rounded">
          キャンセル
        </button>
      </div>
    </form>
  );
}
```

#### AiQuestionForm.tsx【フェーズ3〜4】

```typescript
// src/components/ai/AiQuestionForm.tsx
'use client';
import { useState } from 'react';
import { aiApi, ApiError } from '@/lib/api';
import type { AiSearchResult } from '@/types';
import { AiAnswerPanel } from './AiAnswerPanel';

export function AiQuestionForm() {
  const [question, setQuestion]   = useState('');
  const [result, setResult]       = useState<AiSearchResult | null>(null);
  const [loading, setLoading]     = useState(false);
  const [error, setError]         = useState<string | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!question.trim()) return;
    setLoading(true);
    setError(null);
    setResult(null);
    try {
      const res = await aiApi.search(question);
      setResult(res);
    } catch (e) {
      if (e instanceof ApiError)
        setError(e.status === 504
          ? 'AI回答の生成がタイムアウトしました。しばらく経ってから再試行してください。'
          : e.message);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <form onSubmit={handleSubmit} className="space-y-3">
        <textarea
          value={question}
          onChange={e => setQuestion(e.target.value)}
          placeholder="業務上の疑問・エラー内容を入力してください（例：CSV取込でエラーが出た場合の対処法を教えてください）"
          rows={4}
          maxLength={500}
          className="w-full border rounded px-4 py-3 resize-none"
        />
        <div className="flex justify-between items-center">
          <span className="text-xs text-gray-400">{question.length}/500</span>
          <button type="submit" disabled={loading || !question.trim()}
            className="bg-blue-600 text-white px-6 py-2 rounded disabled:opacity-50">
            {loading ? '回答を生成中...' : '質問する'}
          </button>
        </div>
      </form>

      {error && (
        <div className="bg-red-50 border border-red-300 text-red-700 px-4 py-3 rounded">
          {error}
        </div>
      )}

      {result && <AiAnswerPanel result={result} />}
    </div>
  );
}
```

#### AiAnswerPanel.tsx【フェーズ3〜4】

```typescript
// src/components/ai/AiAnswerPanel.tsx
'use client';
import { useState } from 'react';
import Link from 'next/link';
import { aiApi } from '@/lib/api';
import type { AiSearchResult } from '@/types';

type Props = { result: AiSearchResult };

export function AiAnswerPanel({ result }: Props) {
  const [feedback, setFeedback] = useState<boolean | null>(null);
  const [feedbackSent, setFeedbackSent] = useState(false);

  const handleFeedback = async (isHelpful: boolean) => {
    if (feedbackSent) return;
    await aiApi.feedback(result.aiHistoryId, isHelpful);
    setFeedback(isHelpful);
    setFeedbackSent(true);
  };

  // FAQ0件の場合
  if (!result.answer) {
    return (
      <div className="border rounded p-4 bg-yellow-50 text-yellow-800">
        {result.message}
      </div>
    );
  }

  return (
    <div className="border rounded p-6 space-y-4 bg-white">
      {/* 免責注意文 */}
      <div className="text-xs text-gray-500 bg-gray-50 px-3 py-2 rounded">
        ⚠️ {result.disclaimer}
      </div>

      {/* AI回答 */}
      <div className="prose max-w-none text-sm leading-relaxed whitespace-pre-wrap">
        {result.answer}
      </div>

      {/* 参照元FAQ */}
      {result.sources.length > 0 && (
        <div>
          <p className="text-xs font-semibold text-gray-600 mb-2">参照元：</p>
          <ul className="space-y-1">
            {result.sources.map(s => (
              <li key={s.id} className="text-sm">
                <Link href={s.url} className="text-blue-600 hover:underline">
                  📄 {s.title}
                </Link>
              </li>
            ))}
          </ul>
        </div>
      )}

      {/* フィードバック */}
      <div className="border-t pt-3 flex items-center gap-3">
        <span className="text-xs text-gray-500">この回答は役に立ちましたか？</span>
        <button
          onClick={() => handleFeedback(true)}
          disabled={feedbackSent}
          className={`text-lg ${feedback === true ? 'opacity-100' : 'opacity-40'} disabled:cursor-default`}
        >👍</button>
        <button
          onClick={() => handleFeedback(false)}
          disabled={feedbackSent}
          className={`text-lg ${feedback === false ? 'opacity-100' : 'opacity-40'} disabled:cursor-default`}
        >👎</button>
        {feedbackSent && (
          <span className="text-xs text-green-600">フィードバックを送信しました</span>
        )}
      </div>
    </div>
  );
}
```

---

### 2.4 ページ実装

#### FAQ一覧ページ（app/faqs/page.tsx）

```typescript
// src/app/faqs/page.tsx（RSC）
import { Suspense } from 'react';
import { FaqCardList }    from '@/components/faq/FaqCardList';
import { SearchBar }      from '@/components/faq/SearchBar';
import { CategoryFilter } from '@/components/faq/CategoryFilter';
import { Pagination }     from '@/components/ui/Pagination';
import { faqApi }         from '@/lib/api';
import type { FaqSearchParams } from '@/types';

type Props = {
  searchParams: { keyword?: string; categoryId?: string; tagId?: string;
                  page?: string; sort?: string };
};

export default async function FaqListPage({ searchParams }: Props) {
  const params: FaqSearchParams = {
    keyword:    searchParams.keyword,
    categoryId: searchParams.categoryId ? Number(searchParams.categoryId) : undefined,
    tagId:      searchParams.tagId      ? Number(searchParams.tagId)      : undefined,
    page:       searchParams.page       ? Number(searchParams.page)       : 1,
    pageSize:   20,
    sort:       (searchParams.sort as 'score' | 'updatedAt') ?? 'updatedAt',
    highlight:  !!searchParams.keyword,  // キーワードありならハイライト
  };

  // フェーズ2：sort=scoreの場合はAPI v2を使用
  const useV2 = params.sort === 'score';
  const result = await faqApi.list(params, useV2);

  return (
    <div className="container mx-auto px-4 py-6">
      <div className="flex flex-col gap-4 mb-6">
        <Suspense><SearchBar /></Suspense>
        <div className="flex gap-3">
          <Suspense><CategoryFilter /></Suspense>
        </div>
      </div>
      <p className="text-sm text-gray-500 mb-4">{result.total}件</p>
      <FaqCardList items={result.items} />
      <Pagination total={result.total} page={params.page!} pageSize={20} />
    </div>
  );
}
```

#### FAQ詳細ページ（app/faqs/[id]/page.tsx）

```typescript
// src/app/faqs/[id]/page.tsx（RSC）
import { notFound }        from 'next/navigation';
import { MarkdownRenderer } from '@/components/faq/MarkdownRenderer';
import { TagChip }          from '@/components/ui/TagChip';
import { faqApi }           from '@/lib/api';

export async function generateMetadata({ params }: { params: { id: string } }) {
  const faq = await faqApi.get(Number(params.id)).catch(() => null);
  return { title: faq?.title ?? 'FAQ詳細' };
}

export default async function FaqDetailPage({ params }: { params: { id: string } }) {
  const faq = await faqApi.get(Number(params.id)).catch(() => null);
  if (!faq) notFound();

  return (
    <article className="container mx-auto px-4 py-6 max-w-3xl">
      <header className="mb-6">
        <h1 className="text-2xl font-bold mb-2">{faq.title}</h1>
        <div className="flex flex-wrap gap-2 items-center text-sm text-gray-500">
          <span className="bg-blue-100 text-blue-700 px-2 py-0.5 rounded">
            {faq.categoryName}
          </span>
          {faq.tags.map(t => <TagChip key={t.id} name={t.name} />)}
          <span>更新：{new Date(faq.updatedAt).toLocaleDateString('ja-JP')}</span>
          <span>閲覧数：{faq.viewCount}</span>
        </div>
      </header>
      <MarkdownRenderer content={faq.body} />
    </article>
  );
}
```

#### AI質問ページ（app/ai-search/page.tsx）【フェーズ3〜】

```typescript
// src/app/ai-search/page.tsx（RSCラッパー + CCコンポーネント）
import { AiQuestionForm } from '@/components/ai/AiQuestionForm';

export default function AiSearchPage() {
  return (
    <div className="container mx-auto px-4 py-6 max-w-3xl">
      <h1 className="text-2xl font-bold mb-2">AI質問</h1>
      <p className="text-sm text-gray-500 mb-6">
        業務上の疑問やエラー内容を自然文で入力すると、
        関連するFAQをもとにAIが回答を生成します。
      </p>
      <AiQuestionForm />
    </div>
  );
}
```

---

## 3. バリデーション詳細設計

### 3.1 バックエンドバリデーション一覧

| エンドポイント | フィールド | ルール | エラーメッセージ |
|---|---|---|---|
| POST/PUT /faqs | title | 必須・最大100文字 | 「タイトルは必須です」「タイトルは100文字以内」 |
| POST/PUT /faqs | body | 必須 | 「本文は必須です」 |
| POST/PUT /faqs | categoryId | 必須・存在するID | 「カテゴリを選択してください」「指定されたカテゴリが存在しません」 |
| POST/PUT /faqs | tagIds（各要素） | 存在するID | 「指定されたタグが存在しません」 |
| POST /categories | name | 必須・最大100文字・重複不可 | 「カテゴリ名は必須です」「同名のカテゴリが既に存在します」 |
| POST /tags | name | 必須・最大50文字・重複不可 | 「タグ名は必須です」「同名のタグが既に存在します」 |
| POST /auth/login | email | 必須・メール形式 | 「メールアドレスの形式が正しくありません」 |
| POST /auth/login | password | 必須 | 「パスワードは必須です」 |
| POST /ai/search | question | 必須・最大500文字 | 「質問文は必須です」「質問文は500文字以内」 |
| PUT /users/{id}/role | role | User/Editor/Admin いずれか | 「無効なロールです」 |
| DELETE /categories/{id} | — | FAQ紐づきなし | 「このカテゴリにはFAQが登録されています。先にFAQを移動または削除してください」 |

### 3.2 フロントエンドバリデーション（zodスキーマ）

```typescript
// src/lib/validators.ts
import { z } from 'zod';

export const faqSchema = z.object({
  title:       z.string().min(1, '必須').max(100, '100文字以内'),
  body:        z.string().min(1, '必須'),
  categoryId:  z.number().min(1, 'カテゴリを選択してください'),
  tagIds:      z.number().array().optional(),
  isPublished: z.boolean(),
});

export const loginSchema = z.object({
  email:    z.string().min(1, '必須').email('メールアドレスの形式が正しくありません'),
  password: z.string().min(1, '必須'),
});

export const categorySchema = z.object({
  name:         z.string().min(1, '必須').max(100, '100文字以内'),
  displayOrder: z.number().int().min(0),
});

export const tagSchema = z.object({
  name: z.string().min(1, '必須').max(50, '50文字以内'),
});

export const aiQuestionSchema = z.object({
  question: z.string().min(1, '必須').max(500, '500文字以内'),
});
```

---

## 4. テスト詳細設計

### 4.1 単体テスト（xUnit + Moq）

#### FaqServiceTests.cs

```csharp
// FaqApp.Tests/UnitTests/FaqServiceTests.cs
public class FaqServiceTests
{
    private readonly Mock<IFaqRepository>      _faqRepo  = new();
    private readonly Mock<ICategoryRepository> _catRepo  = new();
    private readonly Mock<ITagRepository>      _tagRepo  = new();
    private FaqService CreateService() =>
        new(_faqRepo.Object, _catRepo.Object, _tagRepo.Object);

    [Fact]
    public async Task GetListAsync_未認証時_公開FAQのみ返す()
    {
        var faqs = new List<Faq> {
            CreatePublishedFaq(1), CreateUnpublishedFaq(2) };
        _faqRepo.Setup(r => r.SearchAsync(
                It.IsAny<FaqSearchQuery>(), false, It.IsAny<CancellationToken>()))
            .ReturnsAsync((faqs.Where(f => f.IsPublished).ToList(), 1));

        var result = await CreateService()
            .GetListAsync(new FaqSearchQuery(), includeUnpublished: false);

        Assert.Equal(1, result.Total);
        Assert.All(result.Items, dto => Assert.True(dto.IsPublished)); // ← 確認不可（DTOにIsPublished無し）
    }

    [Fact]
    public async Task GetByIdAsync_取得成功時_ViewCountがインクリメントされる()
    {
        var faq = CreatePublishedFaq(1);
        _faqRepo.Setup(r => r.GetByIdAsync(1, false, default)).ReturnsAsync(faq);

        await CreateService().GetByIdAsync(1, includeUnpublished: false);

        _faqRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
        Assert.Equal(1, faq.ViewCount);
    }

    [Fact]
    public async Task GetByIdAsync_存在しないID_nullを返す()
    {
        _faqRepo.Setup(r => r.GetByIdAsync(999, false, default))
            .ReturnsAsync((Faq?)null);

        var result = await CreateService().GetByIdAsync(999, false);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_存在しないCategoryId_ValidationExceptionをスロー()
    {
        _catRepo.Setup(r => r.ExistsAsync(99, default)).ReturnsAsync(false);

        var req = new FaqCreateRequest("タイトル", "本文", 99, null, false);
        await Assert.ThrowsAsync<ValidationException>(
            () => CreateService().CreateAsync(req));
    }

    [Fact]
    public async Task DeleteAsync_論理削除_DeletedAtが設定される()
    {
        var faq = CreatePublishedFaq(1);
        _faqRepo.Setup(r => r.GetByIdAsync(1, true, default)).ReturnsAsync(faq);

        var result = await CreateService().DeleteAsync(1);

        Assert.True(result);
        Assert.NotNull(faq.DeletedAt);
        _faqRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    // ヘルパー
    private static Faq CreatePublishedFaq(int id)
    {
        var faq = Faq.Create("テスト", "本文", 1, true);
        typeof(Faq).GetProperty("Id")!.SetValue(faq, id);
        return faq;
    }
    private static Faq CreateUnpublishedFaq(int id)
    {
        var faq = Faq.Create("非公開FAQ", "本文", 1, false);
        typeof(Faq).GetProperty("Id")!.SetValue(faq, id);
        return faq;
    }
}
```

#### AiServiceTests.cs

```csharp
// FaqApp.Tests/UnitTests/AiServiceTests.cs
public class AiServiceTests
{
    private readonly Mock<IFaqRepository>       _faqRepo  = new();
    private readonly Mock<IAiHistoryRepository> _histRepo = new();
    private readonly Mock<IAiApiClient>         _aiClient = new();
    private readonly IOptions<AiSettings> _aiOptions =
        Options.Create(new AiSettings { Provider = "Claude", TimeoutSeconds = 30 });

    private AiService CreateService() =>
        new(_faqRepo.Object, _histRepo.Object, _aiClient.Object, _aiOptions);

    [Fact]
    public async Task SearchAsync_FAQ0件_AI未呼び出しでmessageを返す()
    {
        _faqRepo.Setup(r => r.SearchAsync(
                It.IsAny<FaqSearchQuery>(), false, default))
            .ReturnsAsync((new List<Faq>(), 0));

        var result = await CreateService()
            .SearchAsync(new AiSearchRequest("テスト"), userId: null);

        Assert.Null(result.Answer);
        Assert.NotNull(result.Message);
        _aiClient.Verify(c => c.GenerateSummaryAsync(
            It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), default), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_正常_AI回答と参照元を返す()
    {
        var faqs = new List<Faq> { CreateFaq(1, "CSV取込手順") };
        _faqRepo.Setup(r => r.SearchAsync(It.IsAny<FaqSearchQuery>(), false, default))
            .ReturnsAsync((faqs, 1));
        _aiClient.Setup(c => c.GenerateSummaryAsync(It.IsAny<string>(),
            It.IsAny<IEnumerable<string>>(), default))
            .ReturnsAsync("CSVの文字コードを確認してください。");

        var result = await CreateService()
            .SearchAsync(new AiSearchRequest("CSV取込エラー"), userId: "user-1");

        Assert.NotNull(result.Answer);
        Assert.Single(result.Sources);
        Assert.NotNull(result.Disclaimer);
        _histRepo.Verify(r => r.SaveChangesAsync(default), Times.Once);
    }

    private static Faq CreateFaq(int id, string title)
    {
        var faq = Faq.Create(title, "本文", 1, true);
        typeof(Faq).GetProperty("Id")!.SetValue(faq, id);
        var cat = Category.Create("CSV取込", 1);
        typeof(Faq).GetProperty("Category")!.SetValue(faq, cat);
        typeof(Faq).GetProperty("FaqTags")!.SetValue(faq, new List<FaqTag>());
        return faq;
    }
}
```

### 4.2 統合テスト（WebApplicationFactory）

```csharp
// FaqApp.Tests/IntegrationTests/FaqsApiTests.cs
public class FaqsApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GET_v1_faqs_未認証_200_公開FAQのみ返す()
    {
        var client   = factory.CreateClient();
        var response = await client.GetAsync("/api/v1/faqs");

        response.EnsureSuccessStatusCode();
        var body = await response.Content
            .ReadFromJsonAsync<PagedResult<FaqSummaryDto>>();
        Assert.NotNull(body);
        Assert.All(body.Items, faq => Assert.True(faq.IsPublished));
    }

    [Fact]
    public async Task POST_v1_faqs_未認証_401を返す()
    {
        var client   = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/faqs",
            new FaqCreateRequest("タイトル", "本文", 1, null, false));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task POST_v1_auth_login_無効アカウント_403を返す()
    {
        // テスト用無効ユーザーをSeederで準備
        var client   = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login",
            new LoginRequest("inactive@faq-app.local", "Password123"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
```

### 4.3 E2Eテスト（Playwright）

```typescript
// e2e/faq-search.spec.ts
import { test, expect } from '@playwright/test';

test.describe('FAQ検索→詳細表示', () => {
  test('キーワード検索でFAQが絞り込まれる', async ({ page }) => {
    await page.goto('/faqs');
    await page.getByPlaceholder('キーワードで検索').fill('CSV');
    await page.waitForTimeout(600); // デバウンス待機

    const cards = page.locator('[data-testid="faq-card"]');
    await expect(cards.first()).toBeVisible();
    const title = await cards.first().locator('h2').textContent();
    expect(title).toContain('CSV');
  });

  test('FAQカードクリックで詳細画面に遷移する', async ({ page }) => {
    await page.goto('/faqs');
    await page.locator('[data-testid="faq-card"]').first().click();
    await expect(page).toHaveURL(/\/faqs\/\d+/);
    await expect(page.locator('article h1')).toBeVisible();
  });
});

test.describe('FAQ登録（管理者）', () => {
  test.beforeEach(async ({ page }) => {
    // 管理者ログイン
    await page.goto('/login');
    await page.getByLabel('メールアドレス').fill('admin@faq-app.local');
    await page.getByLabel('パスワード').fill(process.env.ADMIN_PASSWORD!);
    await page.getByRole('button', { name: 'ログイン' }).click();
    await page.waitForURL('/faqs');
  });

  test('FAQ登録後に一覧に表示される', async ({ page }) => {
    await page.goto('/admin/faqs/new');
    await page.getByLabel('タイトル').fill('E2Eテスト用FAQ');
    await page.getByLabel('カテゴリ').selectOption({ label: 'CSV取込' });
    await page.getByLabel('本文').fill('これはE2Eテスト用のFAQです。');
    await page.getByRole('button', { name: '登録' }).click();
    await page.waitForURL('/faqs');

    await expect(page.locator('text=E2Eテスト用FAQ')).toBeVisible();
  });
});

test.describe('権限制御', () => {
  test('未認証でadminページにアクセスするとloginにリダイレクトされる', async ({ page }) => {
    await page.goto('/admin/faqs/new');
    await expect(page).toHaveURL('/login');
  });

  test('AI質問画面は未認証でloginにリダイレクトされる', async ({ page }) => {
    await page.goto('/ai-search');
    await expect(page).toHaveURL('/login');
  });
});
```

---

## 5. FTS（フルテキスト検索）Migration詳細【フェーズ2】

```csharp
// FaqApp.Infrastructure/Data/Migrations/AddFtsIndexes.cs
public partial class AddFtsIndexes : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // FTSカタログ作成
        migrationBuilder.Sql("""
            IF NOT EXISTS (
                SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'FaqCatalog'
            )
            CREATE FULLTEXT CATALOG FaqCatalog AS DEFAULT;
            """);

        // FTSインデックス作成（Title・Body を対象）
        migrationBuilder.Sql("""
            IF NOT EXISTS (
                SELECT 1 FROM sys.fulltext_indexes
                WHERE object_id = OBJECT_ID('dbo.Faqs')
            )
            CREATE FULLTEXT INDEX ON dbo.Faqs(Title, Body)
              KEY INDEX PK_Faqs
              ON FaqCatalog
              WITH CHANGE_TRACKING AUTO;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP FULLTEXT INDEX ON dbo.Faqs;");
        migrationBuilder.Sql("DROP FULLTEXT CATALOG FaqCatalog;");
    }
}
```

---

## 6. 設定ファイル詳細

### 6.1 appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=FaqAppDb;Trusted_Connection=True;"
  },
  "JwtSettings": {
    "Secret":            "your-super-secret-key-32chars-minimum",
    "Issuer":            "https://faq-app-api.azurewebsites.net",
    "Audience":          "https://faq-app.vercel.app",
    "ExpiresInMinutes":  60
  },
  "AiSettings": {
    "Provider":        "AzureOpenAI",
    "TimeoutSeconds":  30,
    "AzureOpenAI": {
      "Endpoint":        "https://your-resource.openai.azure.com/",
      "ApiKey":          "YOUR_AZURE_OPENAI_KEY",
      "DeploymentName":  "gpt-4o"
    },
    "Claude": {
      "ApiKey": "YOUR_CLAUDE_API_KEY"
    }
  },
  "AdminSeed": {
    "Email":       "admin@faq-app.local",
    "Password":    "Admin@1234",
    "DisplayName": "システム管理者"
  },
  "AllowedOrigins": "https://faq-app.vercel.app",
  "Logging": {
    "LogLevel": {
      "Default":               "Information",
      "Microsoft.AspNetCore":  "Warning"
    }
  }
}
```

### 6.2 .env.local（フロントエンド）

```bash
# フロントエンド環境変数
NEXT_PUBLIC_API_BASE_URL=http://localhost:5000
JWT_SECRET=your-super-secret-key-32chars-minimum
```

### 6.3 設定クラス定義

```csharp
// FaqApp.Application/Settings/JwtSettings.cs
public class JwtSettings
{
    public string Secret           { get; set; } = "";
    public string Issuer           { get; set; } = "";
    public string Audience         { get; set; } = "";
    public int    ExpiresInMinutes { get; set; } = 60;
}

// FaqApp.Application/Settings/AiSettings.cs
public class AiSettings
{
    public string            Provider       { get; set; } = "AzureOpenAI";
    public int               TimeoutSeconds { get; set; } = 30;
    public AzureOpenAIConfig AzureOpenAI    { get; set; } = new();
    public ClaudeConfig      Claude         { get; set; } = new();
}
public class AzureOpenAIConfig
{
    public string Endpoint       { get; set; } = "";
    public string ApiKey         { get; set; } = "";
    public string DeploymentName { get; set; } = "gpt-4o";
}
public class ClaudeConfig
{
    public string ApiKey { get; set; } = "";
}
```

---

## 7. 例外クラス定義

```csharp
// FaqApp.Application/Exceptions/AppExceptions.cs
public class ValidationException(string message) : Exception(message);
public class NotFoundException(string message)   : Exception(message);
public class ForbiddenException(string message)  : Exception(message);
```
