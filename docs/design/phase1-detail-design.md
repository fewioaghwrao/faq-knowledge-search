# 詳細設計書

**社内FAQ検索アプリ（フェーズ1）**

| プロジェクト名 | 社内FAQ検索アプリ | バージョン | 1.0 |
|---|---|---|---|
| 作成者 | — | 作成日 | 2026/05/07 |
| 承認者 | — | 承認日 | yyyy/mm/dd |

## 改訂履歴

| バージョン | 日付 | 変更内容 | 作成者 | 承認者 |
|---|---|---|---|---|
| 1.0 | 2026/05/07 | 初版作成 | — | — |

---

## 1. フロントエンド詳細設計

### 1.1 技術スタック・バージョン

| パッケージ | バージョン | 用途 |
|---|---|---|
| Next.js | 16.x | App Router / RSC / Server Actions |
| React | 19.x | UIフレームワーク（Next.js 16に同梱） |
| TypeScript | 5.x | 型安全な開発 |
| Tailwind CSS | 4.x | ユーティリティCSSフレームワーク |
| react-markdown | 最新 | FAQ本文のMarkdownレンダリング |
| react-md-editor | 最新 | FAQ登録・編集用Markdownエディタ |
| rehype-sanitize | 最新 | MarkdownのXSS対策サニタイズ |
| swr または TanStack Query | 最新 | クライアントサイドのデータフェッチ・キャッシュ |
| zod | 最新 | フォームバリデーションスキーマ定義 |
| react-hook-form | 最新 | フォーム状態管理・バリデーション連携 |
| jose | 最新 | JWTのデコード・検証（Edge Runtime対応） |

### 1.2 ルーティング設計（App Router）

Next.js 16 の App Router を使用する。各ルートは以下のファイル構成で実装する。

| ルート（URL） | ファイルパス | RSC/CC | 説明 |
|---|---|---|---|
| / | app/page.tsx | RSC | /faqs にリダイレクト |
| /faqs | app/faqs/page.tsx | RSC | FAQ一覧（サーバーサイドフェッチ） |
| /faqs/[id] | app/faqs/[id]/page.tsx | RSC | FAQ詳細（generateMetadataでOGP設定） |
| /login | app/login/page.tsx | CC | ログインフォーム（クライアントコンポーネント） |
| /admin/faqs/new | app/admin/faqs/new/page.tsx | CC | FAQ登録フォーム |
| /admin/faqs/[id]/edit | app/admin/faqs/[id]/edit/page.tsx | CC | FAQ編集フォーム（初期値をRSCで取得） |
| /admin/categories | app/admin/categories/page.tsx | RSC | カテゴリ管理（一覧取得はRSC） |
| /admin/tags | app/admin/tags/page.tsx | RSC | タグ管理（一覧取得はRSC） |
| 404 | app/not-found.tsx | RSC | カスタム404ページ |
| error | app/error.tsx | CC | グローバルエラーバウンダリ |

> RSC = React Server Component（サーバーサイドレンダリング）  
> CC = Client Component（`'use client'` ディレクティブを付与）

### 1.3 ミドルウェア設計（middleware.ts）

Next.js 16 の `middleware.ts` で認証チェックとルートガードを実装する。

```typescript
// middleware.ts（プロジェクトルート）
import { NextRequest, NextResponse } from 'next/server';
import { jwtVerify } from 'jose';

const ADMIN_ROUTES = ['/admin'];
const AUTH_ROUTES  = ['/login'];

export async function middleware(req: NextRequest) {
  const token = req.cookies.get('accessToken')?.value;
  const { pathname } = req.nextUrl;

  // /admin 以下は管理者JWTが必須
  if (ADMIN_ROUTES.some(r => pathname.startsWith(r))) {
    if (!token) return NextResponse.redirect(new URL('/login', req.url));
    try {
      const { payload } = await jwtVerify(
        token, new TextEncoder().encode(process.env.JWT_SECRET!)
      );
      if (payload.role !== 'Admin')
        return NextResponse.redirect(new URL('/403', req.url));
    } catch {
      return NextResponse.redirect(new URL('/login', req.url));
    }
  }

  // ログイン済みは /login に入れない
  if (AUTH_ROUTES.includes(pathname) && token) {
    return NextResponse.redirect(new URL('/faqs', req.url));
  }
  return NextResponse.next();
}

export const config = { matcher: ['/admin/:path*', '/login'] };
```

### 1.4 コンポーネント設計

#### 1.4.1 共通コンポーネント

| コンポーネント名 | ファイルパス | RSC/CC | 説明 |
|---|---|---|---|
| Header | components/layout/Header.tsx | CC | ロゴ・検索バー・ログイン状態表示・ログアウトボタン |
| Sidebar | components/layout/Sidebar.tsx | RSC | 管理者のみ表示。管理メニューリンク |
| SearchBar | components/faq/SearchBar.tsx | CC | キーワード入力・500msデバウンス・履歴ドロップダウン |
| CategoryFilter | components/faq/CategoryFilter.tsx | CC | カテゴリセレクト。変更時に即時URLクエリパラメータを更新 |
| TagFilter | components/faq/TagFilter.tsx | CC | タグ複数選択チップ。選択状態をURLクエリパラメータで管理 |
| FaqCard | components/faq/FaqCard.tsx | RSC | タイトル・カテゴリ・タグ・更新日時・閲覧数を表示するカード |
| FaqCardList | components/faq/FaqCardList.tsx | RSC | FaqCard の一覧。件数表示・0件メッセージを含む |
| Pagination | components/ui/Pagination.tsx | CC | ページ切替コンポーネント。URLクエリパラメータ page を更新 |
| MarkdownRenderer | components/faq/MarkdownRenderer.tsx | CC | react-markdown + rehype-sanitize でMarkdownをHTMLに変換 |
| TagChip | components/ui/TagChip.tsx | RSC | タグを表示するチップUI |
| Toast | components/ui/Toast.tsx | CC | 成功・エラー通知トースト。Context経由で呼び出し |
| LoadingSpinner | components/ui/LoadingSpinner.tsx | CC | ローディング中の表示 |
| ConfirmDialog | components/ui/ConfirmDialog.tsx | CC | 削除確認ダイアログ |

#### 1.4.2 FAQ登録・編集フォームコンポーネント

```typescript
// components/faq/FaqForm.tsx ('use client')
type Props = {
  initialData?: FaqDetail;   // 編集時は既存データを渡す
  categories:   Category[];
  tags:         Tag[];
};

// react-hook-form + zod でバリデーション
const schema = z.object({
  title:       z.string().min(1, '必須').max(100, '100文字以内'),
  body:        z.string().min(1, '必須'),
  categoryId:  z.number({ required_error: 'カテゴリを選択してください' }),
  tagIds:      z.number().array().optional(),
  isPublished: z.boolean(),
});

// フォーム送信は Server Actions または APIクライアント経由で実行
```

### 1.5 データフェッチ設計

| 方式 | 内容 |
|---|---|
| RSC（サーバーコンポーネント） | 一覧・詳細などの初回描画データはRSCでfetchする。キャッシュはNext.js 16のfetchキャッシュ（revalidate設定）を利用 |
| Client Component（SWR） | 検索バーからの絞り込み・ページネーションなど、インタラクション後の再フェッチはSWRを利用 |
| Server Actions | FAQ登録・編集・削除などのミューテーション処理。フォームのaction属性またはhandleSubmitから呼び出し |
| APIクライアント | lib/api.ts にfetch wrapperを実装。全APIコールのベースURL・認証ヘッダー付与・エラーハンドリングを集約 |
| 環境変数 | `NEXT_PUBLIC_API_BASE_URL`（バックエンドAPIのベースURL）、`JWT_SECRET`（ミドルウェア用・サーバーサイドのみ） |

### 1.6 状態管理設計

| 状態の種類 | 管理方法 | 具体例 |
|---|---|---|
| サーバーデータキャッシュ | SWR / Next.js fetchキャッシュ | FAQ一覧・カテゴリ・タグ一覧 |
| URL状態 | URLクエリパラメータ（useSearchParams） | 検索キーワード・カテゴリ・タグ・ページ番号 |
| フォーム状態 | react-hook-form | FAQ登録・編集フォームの入力値・バリデーション状態 |
| 認証状態 | httpOnlyクッキー + ContextまたはJotai | JWTトークン・ログイン中ユーザー情報・ロール |
| トースト通知 | React Context（ToastContext） | 保存成功・削除完了・エラーメッセージ |
| 検索履歴（表示用キャッシュ） | localStorage | 検索バードロップダウン用の直近10件（DB保存と同期） |

---

## 2. バックエンド詳細設計

### 2.1 プロジェクト構成

```
FaqApp.sln
├── FaqApp.Api/              # ASP.NET Core Web API（エントリポイント）
│   ├── Controllers/
│   ├── Middlewares/         # グローバル例外ハンドラー
│   ├── Extensions/          # DI登録用 ServiceCollection拡張
│   ├── Program.cs
│   └── appsettings.json
├── FaqApp.Application/      # ユースケース・DTOs・インターフェース
│   ├── Services/
│   ├── DTOs/
│   └── Interfaces/          # IFaqRepository等
├── FaqApp.Domain/           # エンティティ・ドメインルール
│   └── Entities/
├── FaqApp.Infrastructure/   # EF Core・リポジトリ実装
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── Migrations/
│   │   └── Seed/
│   └── Repositories/
└── FaqApp.Tests/            # xUnit テストプロジェクト
    ├── UnitTests/
    └── IntegrationTests/
```

### 2.2 エンティティ定義

#### Faq.cs

```csharp
// FaqApp.Domain/Entities/Faq.cs
public class Faq
{
    public int     Id          { get; private set; }
    public string  Title       { get; private set; } = string.Empty;
    public string  Body        { get; private set; } = string.Empty;
    public int     CategoryId  { get; private set; }
    public bool    IsPublished { get; private set; }
    public int     ViewCount   { get; private set; }
    public DateTime  CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public Category        Category { get; private set; } = null!;
    public ICollection<FaqTag> FaqTags { get; private set; } = [];

    // ファクトリメソッド（new禁止・ドメインロジックを集約）
    public static Faq Create(string title, string body, int categoryId,
                             bool isPublished)
        => new() { Title = title, Body = body, CategoryId = categoryId,
                   IsPublished = isPublished, CreatedAt = DateTime.UtcNow };

    public void Update(string title, string body, int categoryId, bool isPublished)
    {
        Title = title; Body = body; CategoryId = categoryId;
        IsPublished = isPublished; UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementViewCount() => ViewCount++;
    public void Delete()             => DeletedAt = DateTime.UtcNow;
    public bool IsDeleted            => DeletedAt.HasValue;
}
```

#### AppDbContext.cs

```csharp
// FaqApp.Infrastructure/Data/AppDbContext.cs
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Faq>           Faqs            => Set<Faq>();
    public DbSet<Category>      Categories      => Set<Category>();
    public DbSet<Tag>           Tags            => Set<Tag>();
    public DbSet<FaqTag>        FaqTags         => Set<FaqTag>();
    public DbSet<SearchHistory> SearchHistories => Set<SearchHistory>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // Faq: 論理削除フィルター
        mb.Entity<Faq>().HasQueryFilter(f => f.DeletedAt == null);

        // FaqTag: 複合PK
        mb.Entity<FaqTag>().HasKey(ft => new { ft.FaqId, ft.TagId });

        // Category: 一意制約
        mb.Entity<Category>().HasIndex(c => c.Name).IsUnique();

        // Tag: 一意制約
        mb.Entity<Tag>().HasIndex(t => t.Name).IsUnique();

        // Faq フルテキストインデックス（EF Coreでは生SQLで追加）
        mb.Entity<Faq>().ToTable(t =>
            t.HasCheckConstraint("CK_Faq_Title", "[Title] != ''"));
    }
}
```

### 2.3 サービスクラス詳細

#### FaqService.cs

```csharp
// FaqApp.Application/Services/FaqService.cs
public class FaqService(IFaqRepository repo, ICategoryRepository catRepo,
                         ITagRepository tagRepo)
{
    // FAQ一覧取得（絞り込み・ページネーション）
    public async Task<PagedResult<FaqSummaryDto>> GetListAsync(
        FaqSearchQuery query, bool includeUnpublished = false)
    {
        var faqs = await repo.SearchAsync(query, includeUnpublished);
        return faqs.ToPagedResult(query.Page, query.PageSize);
    }

    // FAQ詳細取得（ViewCount++）
    public async Task<FaqDetailDto?> GetByIdAsync(int id, bool includeUnpublished)
    {
        var faq = await repo.GetByIdAsync(id, includeUnpublished);
        if (faq is null) return null;
        faq.IncrementViewCount();
        await repo.SaveChangesAsync();
        return FaqDetailDto.From(faq);
    }

    // FAQ登録
    public async Task<FaqDetailDto> CreateAsync(FaqCreateRequest req)
    {
        await ValidateCategoryAndTagsAsync(req.CategoryId, req.TagIds);
        var faq = Faq.Create(req.Title, req.Body, req.CategoryId, req.IsPublished);
        foreach (var tagId in req.TagIds ?? [])
            faq.FaqTags.Add(new FaqTag { TagId = tagId });
        await repo.AddAsync(faq);
        await repo.SaveChangesAsync();
        return FaqDetailDto.From(faq);
    }

    // FAQ更新
    public async Task<FaqDetailDto?> UpdateAsync(int id, FaqUpdateRequest req)
    {
        var faq = await repo.GetByIdAsync(id, includeUnpublished: true);
        if (faq is null) return null;
        await ValidateCategoryAndTagsAsync(req.CategoryId, req.TagIds);
        faq.Update(req.Title, req.Body, req.CategoryId, req.IsPublished);
        await repo.UpdateTagsAsync(faq, req.TagIds ?? []);
        await repo.SaveChangesAsync();
        return FaqDetailDto.From(faq);
    }

    // FAQ削除（論理削除）
    public async Task<bool> DeleteAsync(int id)
    {
        var faq = await repo.GetByIdAsync(id, includeUnpublished: true);
        if (faq is null) return false;
        faq.Delete();
        await repo.SaveChangesAsync();
        return true;
    }
}
```

### 2.4 コントローラー詳細

#### FaqsController.cs

```csharp
// FaqApp.Api/Controllers/FaqsController.cs
[ApiController]
[Route("api/v1/faqs")]
public class FaqsController(FaqService faqService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] FaqSearchQuery query)
    {
        var isAdmin = User.IsInRole("Admin");
        var result  = await faqService.GetListAsync(query, isAdmin);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var isAdmin = User.IsInRole("Admin");
        var faq     = await faqService.GetByIdAsync(id, isAdmin);
        return faq is null ? NotFound(new { message = "指定されたFAQは存在しません" })
                           : Ok(faq);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] FaqCreateRequest req)
    {
        var created = await faqService.CreateAsync(req);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] FaqUpdateRequest req)
    {
        var updated = await faqService.UpdateAsync(id, req);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await faqService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
```

### 2.5 DTOクラス定義

| DTOクラス名 | 用途 | 主なプロパティ |
|---|---|---|
| FaqSummaryDto | FAQ一覧の各アイテム | Id, Title, CategoryId, CategoryName, Tags[], IsPublished, ViewCount, UpdatedAt |
| FaqDetailDto | FAQ詳細・登録・更新レスポンス | FaqSummaryDto の全項目 + Body, CreatedAt |
| FaqCreateRequest | FAQ登録リクエストボディ | Title（必須・100字）, Body（必須）, CategoryId（必須）, TagIds[], IsPublished |
| FaqUpdateRequest | FAQ更新リクエストボディ | FaqCreateRequest と同形式 |
| FaqSearchQuery | FAQ一覧のクエリパラメータ | Keyword, CategoryId?, TagId?, Page（default 1）, PageSize（default 20） |
| PagedResult\<T\> | ページネーション付き一覧レスポンス | Total, Page, PageSize, Items: T[] |
| CategoryDto | カテゴリ情報 | Id, Name, DisplayOrder |
| TagDto | タグ情報 | Id, Name |
| LoginRequest | ログインリクエスト | Email, Password |
| LoginResponse | JWT発行レスポンス | AccessToken, ExpiresIn, Role |
| SearchHistoryDto | 検索履歴 | Id, Keyword, SearchedAt |

### 2.6 バリデーション設計

リクエストボディのバリデーションはDataAnnotations + FluentValidationで実装する。

```csharp
// FaqApp.Application/Validators/FaqCreateRequestValidator.cs
public class FaqCreateRequestValidator : AbstractValidator<FaqCreateRequest>
{
    public FaqCreateRequestValidator(ICategoryRepository catRepo,
                                     ITagRepository tagRepo)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("タイトルは必須です")
            .MaximumLength(100).WithMessage("タイトルは100文字以内で入力してください");

        RuleFor(x => x.Body)
            .NotEmpty().WithMessage("本文は必須です");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("カテゴリを選択してください")
            .MustAsync(async (id, _) => await catRepo.ExistsAsync(id))
            .WithMessage("指定されたカテゴリが存在しません");

        RuleForEach(x => x.TagIds)
            .MustAsync(async (id, _) => await tagRepo.ExistsAsync(id))
            .WithMessage("指定されたタグが存在しません");
    }
}
```

---

## 3. 主要処理フロー（シーケンス）

### 3.1 FAQ検索フロー

```
ユーザー → Next.js(RSC) → ASP.NET Core API → FaqService → FaqRepository → SQL Server

1. ユーザーが /faqs?keyword=CSV&categoryId=2 にアクセス
2. Next.js RSC が fetch('GET /api/v1/faqs?keyword=CSV&categoryId=2') を実行
3. FaqsController.GetList() が FaqSearchQuery を受け取る
4. FaqService.GetListAsync() がリポジトリに検索条件を渡す
5. FaqRepository が EF Core + FTS で SQL Server をクエリ
   SELECT ... FROM Faqs WHERE DeletedAt IS NULL
     AND IsPublished = 1                    -- 未認証の場合
     AND CONTAINS((Title, Body), '"CSV*"')  -- フルテキスト検索
     AND CategoryId = 2
   ORDER BY UpdatedAt DESC
   OFFSET 0 ROWS FETCH NEXT 20 ROWS ONLY
6. PagedResult<FaqSummaryDto> をレスポンス
7. RSC がHTMLをレンダリングしてブラウザに返す

（クライアントサイド絞り込み）
8. ユーザーがSearchBarにキーワードを入力
9. 500ms デバウンス後に useRouter.push() でURLクエリパラメータを更新
10. Next.js がページを再レンダリング（RSC → API再コール）
```

### 3.2 FAQ登録フロー

```
管理者 → Next.js(CC) → Server Action / API → FaqService → DB

1. 管理者が /admin/faqs/new にアクセス（middlewareがAdminロールを確認）
2. フォームに入力 → 送信ボタン押下
3. react-hook-form + zod がクライアントサイドバリデーション実行
4. バリデーション通過 → POST /api/v1/faqs を fetch
   Authorization: Bearer {JWT}
   Body: { title, body, categoryId, tagIds, isPublished }
5. FaqsController がJWTのAdminロールを確認（[Authorize(Roles='Admin')]）
6. FluentValidation が CategoryId・TagIds の存在確認（DB照合）
7. FaqService.CreateAsync() で Faq.Create() → DB保存
8. 201 Created + FaqDetailDto を返す
9. フロントエンドが /faqs に router.push() でリダイレクト
10. SWR / Next.js キャッシュを revalidate して一覧を更新
```

### 3.3 JWT認証フロー

```
ユーザー → Next.js(/login) → POST /api/v1/auth/login → AuthService → DB

1. ユーザーがメール・パスワードを入力して送信
2. POST /api/v1/auth/login にリクエスト
3. AuthService が ASP.NET Core Identity でパスワードHash照合
4. 照合成功 → JwtSecurityTokenHandler でJWT生成
   Payload: { sub, email, role, iat, exp(+60分) }
   署名アルゴリズム: HS256（JWT_SECRET は appsettings から取得）
5. { accessToken, expiresIn: 3600, role } をレスポンス
6. フロントエンドが accessToken を httpOnlyクッキーにセット
7. middleware.ts が以降のリクエストでクッキーからJWTを取得・検証
```

---

## 4. データベース詳細設計

### 4.1 マイグレーション管理

| 項目 | 内容 |
|---|---|
| ツール | Entity Framework Core（dotnet-ef CLI） |
| マイグレーション生成 | `dotnet ef migrations add {MigrationName} --project FaqApp.Infrastructure --startup-project FaqApp.Api` |
| DB更新 | `dotnet ef database update --project FaqApp.Infrastructure --startup-project FaqApp.Api` |
| ロールバック | `dotnet ef database update {前のMigrationName}` |
| 初期Migration名 | InitialCreate |

### 4.2 フルテキスト検索のSQL設定

EF Core Migration では FTS カタログとインデックスをRaw SQLで作成する。

```sql
-- FTS カタログ作成（Migration の Up() 内で実行）
CREATE FULLTEXT CATALOG FaqCatalog AS DEFAULT;

-- Faqs テーブルに FTS インデックスを作成
CREATE FULLTEXT INDEX ON Faqs(Title, Body)
  KEY INDEX PK_Faqs ON FaqCatalog
  WITH CHANGE_TRACKING AUTO;

-- 検索クエリ例（プレフィックス一致）
SELECT Id, Title FROM Faqs
WHERE CONTAINS((Title, Body), '"CSV*"')
  AND IsPublished = 1
  AND DeletedAt IS NULL;

-- 複数キーワード AND 検索
-- keyword = 'CSV 取込' → '"CSV*" AND "取込*"' に変換
```

### 4.3 主要クエリ定義

#### FAQ一覧取得クエリ（EF Core）

```csharp
// FaqRepository.cs
public async Task<(List<Faq> Items, int Total)> SearchAsync(
    FaqSearchQuery q, bool includeUnpublished)
{
    var query = _ctx.Faqs
        .Include(f => f.Category)
        .Include(f => f.FaqTags).ThenInclude(ft => ft.Tag)
        .AsQueryable();

    if (!includeUnpublished)
        query = query.Where(f => f.IsPublished);

    if (q.CategoryId.HasValue)
        query = query.Where(f => f.CategoryId == q.CategoryId);

    if (q.TagId.HasValue)
        query = query.Where(f => f.FaqTags.Any(ft => ft.TagId == q.TagId));

    if (!string.IsNullOrWhiteSpace(q.Keyword))
    {
        // FTS が使えない場合のフォールバック（開発環境）
        var kw = q.Keyword;
        query = query.Where(f => f.Title.Contains(kw) || f.Body.Contains(kw));
        // 本番は EF.Functions.Contains() または生SQL に差し替え
    }

    var total = await query.CountAsync();
    var items = await query
        .OrderByDescending(f => f.UpdatedAt ?? f.CreatedAt)
        .Skip((q.Page - 1) * q.PageSize)
        .Take(q.PageSize)
        .ToListAsync();

    return (items, total);
}
```

---

## 5. テスト詳細設計

### 5.1 単体テスト（xUnit + Moq）

#### FaqService テストケース

| テストケース | 種別 | 確認内容 |
|---|---|---|
| GetListAsync_公開FAQのみ返す | 正常系 | includeUnpublished=falseのとき非公開FAQが含まれないこと |
| GetListAsync_キーワード絞り込みが機能する | 正常系 | keywordに一致するFAQのみ返すこと |
| GetListAsync_ページネーションが正しく動作する | 正常系 | page=2・pageSize=5のとき6〜10件目が返ること |
| GetByIdAsync_存在しないIDはnullを返す | 異常系 | 存在しないIDを指定したときnullが返ること |
| GetByIdAsync_取得成功時にViewCountが増加する | 正常系 | 取得後にViewCount+1されてSaveChangesが呼ばれること |
| CreateAsync_正常登録でFaqDetailDtoを返す | 正常系 | DBに保存されレスポンスのIDが0より大きいこと |
| CreateAsync_存在しないCategoryIdで例外を投げる | 異常系 | InvalidOperationExceptionがスローされること |
| CreateAsync_存在しないTagIdで例外を投げる | 異常系 | InvalidOperationExceptionがスローされること |
| UpdateAsync_存在しないIDはnullを返す | 異常系 | nullが返ること |
| DeleteAsync_論理削除でDeletedAtが設定される | 正常系 | DeletedAtがnullでないこと・SaveChangesが呼ばれること |

### 5.2 統合テスト（WebApplicationFactory）

```csharp
// FaqApp.Tests/IntegrationTests/FaqsApiTests.cs
public class FaqsApiTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GET_faqs_returns_200_with_published_faqs()
    {
        var client   = factory.CreateClient();
        var response = await client.GetAsync("/api/v1/faqs");
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<PagedResult<FaqSummaryDto>>();
        Assert.NotNull(body);
        Assert.All(body.Items, faq => Assert.True(faq.IsPublished));
    }

    [Fact]
    public async Task POST_faqs_without_token_returns_401()
    {
        var client   = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/faqs",
            new FaqCreateRequest { Title = "Test", Body = "Body", CategoryId = 1 });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
```

### 5.3 E2Eテスト（Playwright）

| シナリオ | 手順 | 検証内容 |
|---|---|---|
| FAQ検索→詳細表示 | ①/faqs を開く → ②検索バーに「CSV」入力 → ③FAQカードをクリック | 詳細画面のタイトルに「CSV」が含まれること。URLが /faqs/{id} になること |
| FAQ登録→一覧反映 | ①/login で管理者ログイン → ②/admin/faqs/new でFAQ登録 → ③/faqs に遷移 | 登録したFAQのタイトルが一覧に表示されること |
| バリデーションエラー表示 | ①/admin/faqs/new でタイトル未入力のまま保存 | 「タイトルは必須です」のエラーメッセージが表示されること |
| カテゴリ追加→FAQ絞り込み | ①/admin/categories でカテゴリ追加 → ②/faqs でカテゴリ絞り込み | 追加したカテゴリがセレクトに表示され、選択後に該当FAQのみ表示されること |
| 未認証で /admin にアクセス | ①ログアウト状態で /admin/faqs/new にアクセス | /login にリダイレクトされること |

---

## 6. デプロイ・環境設定

### 6.1 環境変数一覧

#### バックエンド（appsettings.json / 環境変数）

| キー | 説明 | 例 |
|---|---|---|
| ConnectionStrings__DefaultConnection | DB接続文字列 | `Server=(localdb)\mssqllocaldb;Database=FaqAppDb;...` |
| JwtSettings__Secret | JWT署名シークレット（32文字以上） | your-super-secret-key-32chars |
| JwtSettings__Issuer | JWTの発行者 | https://faq-app.azurewebsites.net |
| JwtSettings__Audience | JWTの受信者 | https://faq-app.vercel.app |
| JwtSettings__ExpiresInMinutes | JWTの有効期間（分） | 60 |
| AdminSeed__Email | 初期管理者メールアドレス | admin@faq-app.local |
| AdminSeed__Password | 初期管理者パスワード | （本番環境は環境変数で管理） |
| AllowedOrigins | CORS許可オリジン | https://faq-app.vercel.app |

#### フロントエンド（.env.local）

| キー | 説明 | 例 |
|---|---|---|
| NEXT_PUBLIC_API_BASE_URL | バックエンドAPIのベースURL | https://faq-app-api.azurewebsites.net |
| JWT_SECRET | ミドルウェアのJWT検証用シークレット（サーバーサイドのみ） | your-super-secret-key-32chars |

### 6.2 GitHub Actions CI/CDワークフロー

```yaml
# .github/workflows/deploy.yml（概略）
name: CI/CD
on:
  push:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '8.x' }
      - run: dotnet test FaqApp.Tests/
      - uses: actions/setup-node@v4
        with: { node-version: '22' }
      - run: npm ci && npm run test && npm run build
        working-directory: frontend/

  deploy-api:
    needs: test
    runs-on: ubuntu-latest
    steps:
      - run: dotnet publish -c Release -o ./publish
      - uses: azure/webapps-deploy@v3
        with:
          app-name: faq-app-api-stg
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
          package: ./publish

  deploy-frontend:
    needs: test
    runs-on: ubuntu-latest
    steps:
      - uses: amondnet/vercel-action@v25
        with:
          vercel-token: ${{ secrets.VERCEL_TOKEN }}
          vercel-org-id: ${{ secrets.VERCEL_ORG_ID }}
          vercel-project-id: ${{ secrets.VERCEL_PROJECT_ID }}
```

### 6.3 ローカル開発起動手順

| 手順 | コマンド | 備考 |
|---|---|---|
| 1. リポジトリクローン | `git clone {repo-url}` | |
| 2. DBセットアップ | `dotnet ef database update`（FaqApp.Api から実行） | LocalDBが自動作成される |
| 3. バックエンド起動 | `dotnet run --project FaqApp.Api` | localhost:5000 で起動 |
| 4. フロントエンド起動 | `npm install && npm run dev`（frontendディレクトリ） | localhost:3000 で起動 |
| 5. 管理者ログイン | admin@faq-app.local / appsettings の初期パスワード | Seederで自動作成 |