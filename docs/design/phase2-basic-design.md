# 基本設計書

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

## 1. システム構成・アーキテクチャ

### 1.1 全体アーキテクチャ

本システムはフロントエンド・バックエンドAPI・データベースの3層構成とし、フロントエンドとバックエンドは独立してデプロイ可能な構成とする。フェーズ3以降はAIサービス（Azure OpenAI Service または Anthropic Claude API）を外部連携として追加する。

| 項目 | 内容 |
|---|---|
| フロントエンド | Next.js 16（App Router）/ React 19 / TypeScript |
| バックエンドAPI | ASP.NET Core 8 Web API / C# |
| データベース | SQL Server（本番：Azure SQL Database、開発：SQL Server LocalDB） |
| 認証 | JWT（JSON Web Token）/ ASP.NET Core Identity（ApplicationUser拡張） |
| AIサービス | Azure OpenAI Service（GPT-4o）または Anthropic Claude API（フェーズ3〜、設定で切替） |
| ホスティング | フロント：Vercel または Azure Static Web Apps / API：Azure App Service |
| CI/CD | GitHub Actions（mainブランチマージ → Staging自動デプロイ） |

### 1.2 フェーズ別アーキテクチャ追加要素

| フェーズ | 追加・変更要素 |
|---|---|
| フェーズ1 | 3層構成（フロント / API / DB）・JWT認証（User/Admin 2ロール） |
| フェーズ2 | SQL Server FTS（Full-Text Search）有効化・API v2追加・スコアリングロジック |
| フェーズ3 | AIサービス連携レイヤー追加（AiService）・RAGパイプライン |
| フェーズ4 | AiHistorySources テーブル追加・参照元表示コンポーネント |
| フェーズ5 | AiHistories テーブル追加・AI実行履歴管理画面・Application Insights 必須化 |
| フェーズ6 | 3ロール（User/Editor/Admin）へ拡張・ユーザー管理画面・ロール別ルートガード強化 |
| 余力追加 | Azure Blob Storage（ファイルアップロード）・Slack/Teams Webhook連携 |

### 1.3 レイヤー構成（バックエンド）

バックエンドはClean Architectureに近い4層構成とする。

| レイヤー | 役割 | 主なクラス・ファイル |
|---|---|---|
| API層（Controllers） | HTTPリクエストの受付・レスポンス返却・バリデーション | FaqsController, CategoriesController, TagsController, AuthController, AiController, UsersController |
| アプリケーション層（Services） | ビジネスロジック・ユースケースの実装 | FaqService, CategoryService, TagService, SearchHistoryService, AuthService, AiService, UserService |
| ドメイン層（Entities） | エンティティ・ドメインルールの定義 | Faq, Category, Tag, FaqTag, SearchHistory, AiHistory, AiHistorySource, ApplicationUser |
| インフラ層（Repositories） | DBアクセス・外部サービス連携 | FaqRepository, CategoryRepository, TagRepository, SearchHistoryRepository, AiHistoryRepository, UserRepository, AiApiClient |

### 1.4 フロントエンドディレクトリ構成

```
src/
  app/
    faqs/
      page.tsx                    # FAQ一覧（S-001）
      [id]/page.tsx               # FAQ詳細（S-002）
    ai-search/
      page.tsx                    # AI質問画面（S-201）【フェーズ3】
    admin/
      faqs/
        new/page.tsx              # FAQ登録（S-003）
        [id]/edit/page.tsx        # FAQ編集（S-004）
      categories/page.tsx         # カテゴリ管理（S-005）
      tags/page.tsx               # タグ管理（S-006）
      ai-histories/
        page.tsx                  # AI実行履歴一覧（S-401）【フェーズ5】
        [id]/page.tsx             # AI実行履歴詳細（S-402）【フェーズ5】
      users/page.tsx              # ユーザー管理（S-501）【フェーズ6】
      dashboard/page.tsx          # ダッシュボード（S-601）【余力】
    login/page.tsx                # ログイン画面
    not-found.tsx                 # カスタム404ページ
    error.tsx                     # グローバルエラーバウンダリ
  components/
    faq/                          # FAQ関連コンポーネント
      FaqCard.tsx
      FaqCardList.tsx
      FaqForm.tsx
      MarkdownRenderer.tsx
      SearchBar.tsx
      CategoryFilter.tsx
      TagFilter.tsx
    ai/                           # AI関連コンポーネント【フェーズ3〜】
      AiQuestionForm.tsx
      AiAnswerPanel.tsx
      AiSourceList.tsx
      AiFeedbackButtons.tsx
    layout/                       # ヘッダー・サイドバー等
      Header.tsx
      Sidebar.tsx
      Footer.tsx
    ui/                           # 汎用UIコンポーネント
      Pagination.tsx
      TagChip.tsx
      Toast.tsx
      LoadingSpinner.tsx
      ConfirmDialog.tsx
      Modal.tsx
  lib/
    api.ts                        # APIクライアント（fetch wrapper・v1/v2切替）
    auth.ts                       # JWT取得・保持ユーティリティ
    score.ts                      # スコアリングユーティリティ【フェーズ2】
  types/
    index.ts                      # 型定義（Faq, Category, Tag, AiHistory, User等）
  middleware.ts                   # 認証・ロールガード
```

### 1.5 バックエンドディレクトリ構成

```
FaqApp.sln
├── FaqApp.Api/                   # ASP.NET Core Web API（エントリポイント）
│   ├── Controllers/
│   │   ├── FaqsController.cs
│   │   ├── CategoriesController.cs
│   │   ├── TagsController.cs
│   │   ├── AuthController.cs
│   │   ├── AiController.cs       # 【フェーズ3〜】
│   │   └── UsersController.cs    # 【フェーズ6】
│   ├── Middlewares/
│   │   └── ExceptionHandlerMiddleware.cs
│   ├── Extensions/
│   │   └── ServiceCollectionExtensions.cs
│   ├── Program.cs
│   └── appsettings.json / appsettings.Development.json
│
├── FaqApp.Application/           # ユースケース・DTOs・インターフェース
│   ├── Services/
│   │   ├── FaqService.cs
│   │   ├── CategoryService.cs
│   │   ├── TagService.cs
│   │   ├── SearchHistoryService.cs
│   │   ├── AuthService.cs
│   │   ├── AiService.cs          # 【フェーズ3〜】RAGパイプライン
│   │   └── UserService.cs        # 【フェーズ6】
│   ├── DTOs/
│   │   ├── FaqDto.cs / FaqCreateRequest.cs / FaqUpdateRequest.cs / FaqSearchQuery.cs
│   │   ├── CategoryDto.cs
│   │   ├── TagDto.cs
│   │   ├── AuthDto.cs
│   │   ├── AiDto.cs              # 【フェーズ3〜】
│   │   └── UserDto.cs            # 【フェーズ6】
│   ├── Validators/
│   │   ├── FaqCreateRequestValidator.cs
│   │   └── AiQuestionRequestValidator.cs  # 【フェーズ3〜】
│   └── Interfaces/
│       ├── IFaqRepository.cs
│       ├── ICategoryRepository.cs
│       ├── ITagRepository.cs
│       ├── ISearchHistoryRepository.cs
│       ├── IAiHistoryRepository.cs   # 【フェーズ5】
│       └── IAiApiClient.cs           # 【フェーズ3〜】
│
├── FaqApp.Domain/                # エンティティ・ドメインルール
│   └── Entities/
│       ├── Faq.cs
│       ├── Category.cs
│       ├── Tag.cs
│       ├── FaqTag.cs
│       ├── SearchHistory.cs
│       ├── AiHistory.cs          # 【フェーズ5】
│       ├── AiHistorySource.cs    # 【フェーズ5】
│       └── ApplicationUser.cs    # IdentityUser継承
│
├── FaqApp.Infrastructure/        # EF Core・リポジトリ実装・外部API
│   ├── Data/
│   │   ├── AppDbContext.cs
│   │   ├── Migrations/
│   │   └── Seed/
│   │       ├── RoleSeeder.cs
│   │       ├── CategorySeeder.cs
│   │       └── AdminUserSeeder.cs
│   ├── Repositories/
│   │   ├── FaqRepository.cs
│   │   ├── CategoryRepository.cs
│   │   ├── TagRepository.cs
│   │   ├── SearchHistoryRepository.cs
│   │   └── AiHistoryRepository.cs  # 【フェーズ5】
│   └── ExternalServices/
│       └── AiApiClient.cs          # 【フェーズ3〜】OpenAI/Claude API呼び出し
│
└── FaqApp.Tests/
    ├── UnitTests/
    └── IntegrationTests/
```

---

## 2. 認証・認可設計

### 2.1 認証方式

| 項目 | 内容 |
|---|---|
| 認証方式 | JWT（JSON Web Token）Bearer認証 |
| ライブラリ | ASP.NET Core Identity + Microsoft.AspNetCore.Authentication.JwtBearer |
| ユーザー管理 | ApplicationUser（IdentityUser継承）でDB管理。DisplayName・IsActive・CreatedAt等を拡張追加 |
| トークン有効期限 | アクセストークン：60分 / リフレッシュトークン：フェーズ1未実装 |
| トークン保持 | フロントエンドはhttpOnlyクッキーに保持（XSS対策） |
| ロール（フェーズ1） | User・Admin の2ロール |
| ロール（フェーズ6〜） | User・Editor・Admin の3ロールに拡張 |

### 2.2 ApplicationUser 拡張定義

```csharp
// FaqApp.Domain/Entities/ApplicationUser.cs
public class ApplicationUser : IdentityUser
{
    // 業務固有の拡張カラム
    public string    DisplayName { get; set; } = string.Empty; // 画面表示名
    public bool      IsActive    { get; set; } = true;         // 有効=true / 無効=false
    public DateTime  CreatedAt   { get; set; }                 // 作成日時（UTC）
    public DateTime? UpdatedAt   { get; set; }                 // 更新日時（UTC）

    // ナビゲーションプロパティ
    public ICollection<SearchHistory> SearchHistories { get; set; } = [];
    public ICollection<AiHistory>     AiHistories     { get; set; } = [];
}
```

> ロール管理はIdentityの `AspNetRoles` / `AspNetUserRoles` に委ねる。ApplicationUserにロールカラムは持たない。

### 2.3 ロール定義と段階的拡張

| フェーズ | ロール構成 | 変更内容 |
|---|---|---|
| フェーズ1〜5 | User / Admin（2ロール） | Adminが全管理操作を担う |
| フェーズ6 | User / Editor / Admin（3ロール） | AdminからFAQ管理権限をEditorに分離 |

### 2.4 ロール別アクセス制御マトリクス（フェーズ6完成時）

| 操作 | User | Editor | Admin |
|---|---|---|---|
| FAQ一覧・詳細閲覧（公開） | ○ | ○ | ○ |
| FAQ一覧・詳細閲覧（非公開） | × | ○ | ○ |
| FAQ登録・編集・削除 | × | ○ | ○ |
| カテゴリ/タグ管理 | × | ○ | ○ |
| AI質問・AI回答閲覧 | ○ | ○ | ○ |
| AI回答フィードバック | ○ | ○ | ○ |
| AI実行履歴閲覧 | × | ○ | ○ |
| 検索履歴閲覧（自分） | ○ | ○ | ○ |
| 検索履歴閲覧（全体） | × | ○ | ○ |
| ユーザー管理・ロール変更 | × | × | ○ |
| システム設定 | × | × | ○ |
| ダッシュボード・統計 | × | ○ | ○ |

### 2.5 認証フロー

| ステップ | 処理内容 |
|---|---|
| 1. ログイン要求 | ユーザーがメールアドレスとパスワードを POST /api/v1/auth/login に送信 |
| 2. 認証処理 | ApplicationUserを取得。IsActive=falseの場合は401を返す。ASP.NET Core IdentityでパスワードHash照合 |
| 3. JWT発行 | 認証成功時にJWT生成。Claimsにsub・email・role・displayNameを含める |
| 4. トークン返却 | `{ accessToken, expiresIn, role, displayName }` をレスポンスで返す |
| 5. クッキー保存 | フロントエンドがaccessTokenをhttpOnlyクッキーにセット |
| 6. APIアクセス | 以降のリクエストはクッキーからJWTを自動送信 |
| 7. ロール制御 | `[Authorize(Roles = "Admin,Editor")]` でController/Actionを保護 |

### 2.6 JWT ペイロード設計

```json
{
  "sub":         "user-guid-string",
  "email":       "user@example.com",
  "role":        "Editor",
  "displayName": "山田 太郎",
  "iat":         1700000000,
  "exp":         1700003600
}
```

### 2.7 ミドルウェア設計（middleware.ts）

Next.js 16 の `middleware.ts` でルートガードを実装する。

```typescript
// middleware.ts
import { NextRequest, NextResponse } from 'next/server';
import { jwtVerify } from 'jose';

// ロール別保護ルート
const ADMIN_ONLY_ROUTES  = ['/admin/users'];                    // Adminのみ
const EDITOR_ROUTES      = ['/admin/faqs', '/admin/categories',
                             '/admin/tags', '/admin/ai-histories',
                             '/admin/dashboard'];               // Editor以上
const AUTH_REQUIRED      = ['/ai-search'];                     // ログイン必須

export async function middleware(req: NextRequest) {
  const token = req.cookies.get('accessToken')?.value;
  const { pathname } = req.nextUrl;

  const payload = token ? await verifyToken(token) : null;
  const role = payload?.role as string | undefined;

  // Adminのみルート
  if (ADMIN_ONLY_ROUTES.some(r => pathname.startsWith(r))) {
    if (role !== 'Admin')
      return NextResponse.redirect(new URL('/login', req.url));
  }

  // Editor以上ルート
  if (EDITOR_ROUTES.some(r => pathname.startsWith(r))) {
    if (!role || !['Admin', 'Editor'].includes(role))
      return NextResponse.redirect(new URL('/login', req.url));
  }

  // ログイン必須ルート（IsActive チェックはAPI側で実施）
  if (AUTH_REQUIRED.some(r => pathname.startsWith(r))) {
    if (!token)
      return NextResponse.redirect(new URL('/login', req.url));
  }

  // ログイン済みは /login に入れない
  if (pathname === '/login' && token)
    return NextResponse.redirect(new URL('/faqs', req.url));

  return NextResponse.next();
}
```

---

## 3. 画面設計詳細

### 3.1 共通レイアウト

| 項目 | 内容 |
|---|---|
| ヘッダー | サービス名ロゴ・検索バー・ログインユーザー表示名・ログアウトボタン |
| サイドバー | Editor/Admin のみ表示。FAQ管理・カテゴリ管理・タグ管理・AI履歴・ユーザー管理へのリンク（ロール別表示制御） |
| フッター | バージョン・著作権表示 |
| レスポンシブ | デスクトップ（1024px以上）を主対象。モバイルはフェーズ2以降で検討 |

### 3.2 画面別詳細

#### S-001: FAQ一覧（フェーズ1〜2で拡張）

| 項目 | 内容 |
|---|---|
| URL | /faqs?keyword=xxx&categoryId=yyy&tagId=zzz&sort=score&page=1 |
| 主要コンポーネント | SearchBar（キーワード入力・履歴ドロップダウン）、CategoryFilter、TagFilter、FaqCardList、Pagination |
| 表示データ | FAQカード：タイトル・カテゴリ名・タグチップ・更新日時・閲覧数 |
| 初期表示 | 公開中FAQ全件を更新日時降順で表示。ページサイズ20件 |
| 検索動作（フェーズ1） | キーワード入力は500msのデバウンス後にAPIコール（GET /api/v1/faqs） |
| 検索動作（フェーズ2） | sortパラメータにscoreを追加。タイトル・本文・カテゴリ・タグを横断検索。ハイライト表示 |
| 権限差異 | Editor/Admin ログイン時は非公開FAQも表示（「非公開」バッジを明示） |
| バリデーション | キーワードは最大100文字 |

#### S-002: FAQ詳細（フェーズ1）

| 項目 | 内容 |
|---|---|
| URL | /faqs/[id] |
| 主要コンポーネント | FaqHeader（タイトル・カテゴリ・タグ・更新日時）、MarkdownRenderer（本文）、RelatedFaqList（同カテゴリの関連FAQ）、編集ボタン（Editor/Admin のみ）、評価ボタン（👍👎、余力） |
| 本文表示 | react-markdown + rehype-sanitize でMarkdownをHTMLにレンダリング。コードブロックはsyntax-highlighter適用 |
| 閲覧数 | 画面表示時にViewCountをインクリメント（GET /api/v1/faqs/{id} の副作用） |
| エラー | 存在しないIDは404ページにリダイレクト |

#### S-003 / S-004: FAQ登録・編集（フェーズ1）

| 項目 | 内容 |
|---|---|
| URL | /admin/faqs/new（登録） / /admin/faqs/[id]/edit（編集） |
| アクセス制御 | Editor/Admin ロールのみ。未認証または User はリダイレクト |
| 主要コンポーネント | TitleInput、MarkdownEditor（react-md-editor）、CategorySelect、TagMultiSelect、PublishedToggle、SaveButton / CancelButton |
| バリデーション | タイトル必須・100文字以内。本文必須。カテゴリ必須。クライアント側（zod）とサーバー側（FluentValidation）の二重チェック |
| 保存動作 | 登録：POST /api/v1/faqs → 成功後は一覧遷移。編集：PUT /api/v1/faqs/{id} → 成功後は詳細遷移 |
| エラー表示 | フィールド単位のエラーは各入力欄の下に表示。API エラーはフォーム上部にバナー表示 |

#### S-005: カテゴリ管理（フェーズ1）

| 項目 | 内容 |
|---|---|
| URL | /admin/categories |
| アクセス制御 | Editor/Admin ロールのみ |
| 主要コンポーネント | CategoryList（名称・表示順・操作ボタン）、CategoryFormModal（追加・編集用インラインダイアログ） |
| 操作 | 追加・編集・削除（削除は確認ダイアログを表示） |
| 削除制約 | FAQが1件以上紐づくカテゴリは削除不可。エラーメッセージを表示（400 Bad Request） |

#### S-006: タグ管理（フェーズ1）

| 項目 | 内容 |
|---|---|
| URL | /admin/tags |
| アクセス制御 | Editor/Admin ロールのみ |
| 主要コンポーネント | TagList（タグ名・FAQ件数・操作ボタン）、TagFormModal |
| 削除動作 | タグ削除時はFaqTagsの紐づきもカスケード削除 |

#### S-201: AI質問画面（フェーズ3〜4）

| 項目 | 内容 |
|---|---|
| URL | /ai-search |
| アクセス制御 | ログイン必須（User以上）。未認証は /login にリダイレクト |
| 主要コンポーネント | AiQuestionForm（質問入力・送信ボタン）、AiAnswerPanel（回答テキスト・注意文）、AiSourceList（参照元FAQリンク一覧）、AiFeedbackButtons（👍👎、フェーズ4） |
| 操作フロー | ①質問を入力 → ②「質問する」ボタン → ③ローディング（ストリーミング可） → ④AI回答＋参照元FAQ表示 |
| 注意文（固定） | 「この回答はFAQをもとに生成されています。必ず参照元を確認してください」 |
| 参照元0件の場合 | AI回答を生成せず「該当するFAQが見つかりませんでした。管理者にご相談ください」を表示 |
| エラー時 | タイムアウト（30秒）・API エラー時はトースト通知を表示 |

#### S-401: AI実行履歴一覧（フェーズ5）

| 項目 | 内容 |
|---|---|
| URL | /admin/ai-histories?from=yyyy-mm-dd&to=yyyy-mm-dd&isSuccess=true |
| アクセス制御 | Editor/Admin ロールのみ |
| 表示項目 | 実行日時・ユーザー表示名・質問文（先頭50文字）・成功/失敗・評価（👍👎/未評価） |
| 絞り込み | 日付範囲・成功/失敗・評価フィルタ |
| ページネーション | 20件/ページ |

#### S-402: AI実行履歴詳細（フェーズ5）

| 項目 | 内容 |
|---|---|
| URL | /admin/ai-histories/[id] |
| 表示項目 | 実行日時・ユーザー・質問文（全文）・AI回答（全文）・参照元FAQ一覧・評価結果・エラーメッセージ（失敗時） |

#### S-501: ユーザー管理（フェーズ6）

| 項目 | 内容 |
|---|---|
| URL | /admin/users |
| アクセス制御 | Admin ロールのみ |
| 表示項目 | 表示名・メールアドレス・ロール・有効/無効・登録日時 |
| 操作 | ロール変更（セレクトボックス）・有効/無効切替（トグル）・確認ダイアログ表示後に実行 |
| 制約 | 自分自身のロール変更・無効化は不可 |

#### ログイン画面

| 項目 | 内容 |
|---|---|
| URL | /login |
| 主要コンポーネント | EmailInput、PasswordInput、LoginButton |
| 成功時 | JWTをhttpOnlyクッキーにセット → ロールに応じてリダイレクト（Admin/Editor → /admin/faqs、User → /faqs） |
| 失敗時（認証エラー） | 「メールアドレスまたはパスワードが正しくありません」を表示 |
| 失敗時（無効アカウント） | 「このアカウントは無効化されています。管理者にお問い合わせください」を表示 |

---

## 4. API詳細設計

### 4.1 共通仕様

| 項目 | 内容 |
|---|---|
| ベースURL | /api/v1（フェーズ1〜）、/api/v2（フェーズ2〜、検索拡張） |
| Content-Type | application/json |
| 認証ヘッダー | httpOnlyクッキーの accessToken を自動送信（フロントエンド）。Bearer形式でも受付可 |
| レスポンス形式 | JSON。200/201/204/400/401/403/404/409/500 |
| ページネーション | クエリパラメータ page（1始まり）・pageSize（最大100）。レスポンスに total・page・pageSize・items を含む |
| 日時形式 | ISO 8601形式（例：2026-05-09T09:00:00Z）、UTC統一 |
| エラー形式 | `{ "errors": { "fieldName": ["エラーメッセージ"] } }`（400時） |

### 4.2 認証API

#### POST /api/v1/auth/login

| 項目 | 内容 |
|---|---|
| 概要 | ログイン認証・JWTトークン発行 |
| 認証 | 不要 |
| リクエストボディ | `{ "email": "string", "password": "string" }` |
| レスポンス（200） | `{ "accessToken": "string", "expiresIn": 3600, "role": "Admin"\|"Editor"\|"User", "displayName": "string" }` |
| レスポンス（401） | `{ "message": "メールアドレスまたはパスワードが正しくありません" }` |
| レスポンス（403） | `{ "message": "このアカウントは無効化されています。管理者にお問い合わせください" }` |
| 備考 | IsActive=falseのユーザーは403を返す。パスワードはIdentityのPBKDF2ハッシュで検証 |

#### POST /api/v1/auth/logout

| 項目 | 内容 |
|---|---|
| 概要 | ログアウト（クッキー削除） |
| 認証 | 不要 |
| レスポンス（200） | `{ "message": "ログアウトしました" }` |

### 4.3 FAQ API

#### GET /api/v1/faqs

| 項目 | 内容 |
|---|---|
| 概要 | FAQ一覧取得（フェーズ1：部分一致検索・ページネーション） |
| 認証 | 任意（JWTなし→公開のみ、Editor/Admin JWT→全件） |
| クエリパラメータ | keyword: string / categoryId: int / tagId: int / page: int（default 1）/ pageSize: int（default 20） |
| レスポンス（200） | `{ "total": int, "page": int, "pageSize": int, "items": [ FaqSummary ] }` |
| FaqSummary形式 | `{ "id": int, "title": string, "categoryId": int, "categoryName": string, "tags": [{id, name}], "isPublished": bool, "viewCount": int, "updatedAt": datetime }` |
| ソート | 更新日時降順（固定） |

#### GET /api/v2/faqs【フェーズ2追加】

| 項目 | 内容 |
|---|---|
| 概要 | FAQ一覧取得（フェーズ2：複数フィールド横断・スコア順・ハイライト） |
| 認証 | 任意（v1と同様） |
| 追加クエリパラメータ | sort: `score`\|`updatedAt`（default: updatedAt）/ highlight: bool（default: false） |
| 追加レスポンスフィールド | `"score": float`（0〜1）、`"titleHighlighted": string`（ハイライトタグ付きタイトル）、`"bodyExcerpt": string`（マッチ箇所の抜粋、最大200文字） |
| スコアリング | タイトル完全一致:1.0 / タイトル部分一致:0.8 / タグ名:0.7 / カテゴリ名:0.5 / 本文:0.3。複数キーワードは合計スコア |

#### GET /api/v1/faqs/{id}

| 項目 | 内容 |
|---|---|
| 概要 | FAQ詳細取得 |
| 認証 | 任意（非公開FAQはEditor/Admin JWT必要。未認証または User は404を返す） |
| レスポンス（200） | `{ "id": int, "title": string, "body": string, "categoryId": int, "categoryName": string, "tags": [{id, name}], "isPublished": bool, "viewCount": int, "helpfulCount": int, "notHelpfulCount": int, "createdAt": datetime, "updatedAt": datetime }` |
| 副作用 | 取得成功時にViewCountを+1インクリメント |
| レスポンス（404） | `{ "message": "指定されたFAQは存在しません" }` |

#### POST /api/v1/faqs

| 項目 | 内容 |
|---|---|
| 概要 | FAQ新規登録 |
| 認証 | 要（JWT・Editor以上） |
| リクエストボディ | `{ "title": string（必須・最大100文字）, "body": string（必須）, "categoryId": int（必須）, "tagIds": int[]（任意）, "isPublished": bool（default: false）}` |
| レスポンス（201） | 登録されたFAQ詳細オブジェクト |
| バリデーション | title必須・100文字以内。body必須。categoryIdは存在するカテゴリIDであること。tagIdsは存在するタグIDのみ |

#### PUT /api/v1/faqs/{id}

| 項目 | 内容 |
|---|---|
| 概要 | FAQ更新 |
| 認証 | 要（JWT・Editor以上） |
| リクエストボディ | POST /api/v1/faqs と同形式 |
| レスポンス（200） | 更新後のFAQ詳細オブジェクト |

#### DELETE /api/v1/faqs/{id}

| 項目 | 内容 |
|---|---|
| 概要 | FAQ削除（論理削除） |
| 認証 | 要（JWT・Editor以上） |
| レスポンス（204） | No Content |
| 削除仕様 | DeletedAtに現在日時を設定。以降のGET APIには含まれない。物理削除は行わない |

### 4.4 カテゴリ・タグ API

| エンドポイント | 概要 | 認証 | レスポンス形式 |
|---|---|---|---|
| GET /api/v1/categories | カテゴリ一覧取得（DisplayOrder昇順） | 任意 | `[{ "id": int, "name": string, "displayOrder": int }]` |
| POST /api/v1/categories | カテゴリ登録 | 要（Editor以上） | `{ "id": int, "name": string, "displayOrder": int }` |
| PUT /api/v1/categories/{id} | カテゴリ更新 | 要（Editor以上） | 更新後のカテゴリオブジェクト |
| DELETE /api/v1/categories/{id} | カテゴリ削除 | 要（Editor以上） | 204 / 400（FAQ紐づきあり） |
| GET /api/v1/tags | タグ一覧取得（名前昇順） | 任意 | `[{ "id": int, "name": string, "faqCount": int }]` |
| POST /api/v1/tags | タグ登録 | 要（Editor以上） | `{ "id": int, "name": string }` |
| PUT /api/v1/tags/{id} | タグ更新 | 要（Editor以上） | 更新後のタグオブジェクト |
| DELETE /api/v1/tags/{id} | タグ削除（FaqTagsカスケード削除） | 要（Editor以上） | 204 No Content |

### 4.5 検索履歴 API

#### POST /api/v1/search-histories

| 項目 | 内容 |
|---|---|
| 概要 | 検索キーワードをDB保存 |
| 認証 | 任意（ログイン時はUserId、未ログイン時はSessionIdで識別） |
| リクエストボディ | `{ "keyword": string, "sessionId": string（任意）}` |
| レスポンス（201） | `{ "id": int, "keyword": string, "searchedAt": datetime }` |
| 重複排除 | 同一UserId（またはSessionId）・同一keywordはSearchedAtをUpsert更新 |

#### GET /api/v1/search-histories

| 項目 | 内容 |
|---|---|
| 概要 | 検索履歴一覧取得（直近10件、SearchedAt降順） |
| 認証 | 任意 |
| クエリパラメータ | sessionId: string（未ログイン時）/ ログイン時はJWTのuserIdを自動使用 |
| レスポンス（200） | `[{ "id": int, "keyword": string, "searchedAt": datetime }]` |

### 4.6 AI API【フェーズ3〜4】

#### POST /api/v1/ai/search

| 項目 | 内容 |
|---|---|
| 概要 | AI要約回答生成（質問文→FAQ検索→AI要約→参照元） |
| 認証 | ログイン必須（User以上） |
| リクエストボディ | `{ "question": string（必須・最大500文字）}` |
| レスポンス（200）正常 | `{ "answer": string, "disclaimer": string, "sources": [{"id": int, "title": string, "url": string}], "aiHistoryId": int }` |
| レスポンス（200）FAQ0件 | `{ "answer": null, "disclaimer": null, "sources": [], "message": "該当するFAQが見つかりませんでした。管理者にご相談ください。", "aiHistoryId": int }` |
| レスポンス（504） | タイムアウト（30秒超過）`{ "message": "AI回答の生成がタイムアウトしました" }` |
| 処理フロー | ①質問文からキーワード抽出 → ②FTS検索でFAQ上位3〜5件を取得 → ③FAQをコンテキストとしてAI APIに送信 → ④AI回答を受信 → ⑤AiHistories・AiHistorySources に保存 → ⑥レスポンス返却 |

#### POST /api/v1/ai/histories/{id}/feedback【フェーズ4】

| 項目 | 内容 |
|---|---|
| 概要 | AI回答へのフィードバック登録 |
| 認証 | ログイン必須 |
| リクエストボディ | `{ "isHelpful": bool }` |
| レスポンス（200） | `{ "message": "フィードバックを受け付けました" }` |
| 制約 | 同一aiHistoryIdへの2回目以降はUpsert（上書き） |

### 4.7 AI実行履歴 API【フェーズ5】

#### GET /api/v1/ai/histories

| 項目 | 内容 |
|---|---|
| 概要 | AI実行履歴一覧取得（フィルタ・ページネーション） |
| 認証 | 要（Editor以上） |
| クエリパラメータ | from: date / to: date / isSuccess: bool / isHelpful: bool / page: int / pageSize: int |
| レスポンス（200） | `{ "total": int, "page": int, "pageSize": int, "items": [ AiHistorySummary ] }` |
| AiHistorySummary形式 | `{ "id": int, "question": string（先頭50文字）, "displayName": string, "isSuccess": bool, "isHelpful": bool\|null, "executedAt": datetime }` |

#### GET /api/v1/ai/histories/{id}

| 項目 | 内容 |
|---|---|
| 概要 | AI実行履歴詳細取得 |
| 認証 | 要（Editor以上） |
| レスポンス（200） | `{ "id": int, "question": string, "searchKeywords": string, "aiAnswer": string, "isSuccess": bool, "errorMessage": string\|null, "isHelpful": bool\|null, "executedAt": datetime, "user": {"id": string, "displayName": string, "email": string}, "sources": [{"faqId": int, "title": string, "score": float, "displayOrder": int}] }` |

### 4.8 ユーザー管理 API【フェーズ6】

#### GET /api/v1/users

| 項目 | 内容 |
|---|---|
| 概要 | ユーザー一覧取得 |
| 認証 | 要（Admin） |
| クエリパラメータ | keyword: string / role: string / isActive: bool / page: int / pageSize: int |
| レスポンス（200） | `{ "total": int, "items": [{ "id": string, "displayName": string, "email": string, "role": string, "isActive": bool, "createdAt": datetime }] }` |

#### PUT /api/v1/users/{id}/role

| 項目 | 内容 |
|---|---|
| 概要 | ユーザーのロール変更 |
| 認証 | 要（Admin） |
| リクエストボディ | `{ "role": "User"\|"Editor"\|"Admin" }` |
| レスポンス（200） | `{ "message": "ロールを変更しました" }` |
| 制約 | 自分自身のロール変更は不可（400を返す） |

#### PUT /api/v1/users/{id}/status

| 項目 | 内容 |
|---|---|
| 概要 | ユーザーの有効/無効切替 |
| 認証 | 要（Admin） |
| リクエストボディ | `{ "isActive": bool }` |
| レスポンス（200） | `{ "message": "ステータスを変更しました" }` |
| 制約 | 自分自身の無効化は不可（400を返す） |

---

## 5. データベース詳細設計

### 5.1 ER図（テーブル関連）

```
【Identity管理（自動生成）】
AspNetRoles ──< AspNetUserRoles >── AspNetUsers（ApplicationUser）

【業務テーブル】
AspNetUsers（ApplicationUser）
    ├──< SearchHistories（UserId FK、未ログイン時はSessionId）
    ├──< AiHistories（UserId FK）
    │         └──< AiHistorySources >── Faqs
    └──< FaqRatings（UserId FK、未ログイン時はSessionId）【余力】

Categories ──< Faqs >──< FaqTags >── Tags
                 └──< FaqRatings >【余力】

【独立テーブル】
CategoryStats（余力）
```

### 5.2 テーブル定義

#### AspNetUsers（ApplicationUser拡張）【フェーズ1】

| No | カラム名 | データ型 | NULL | キー | デフォルト | 備考 |
|---|---|---|---|---|---|---|
| 1 | Id | nvarchar(450) | NOT NULL | PK | | IdentityのGUID文字列 |
| 2 | DisplayName | nvarchar(100) | NOT NULL | | | 画面表示名 |
| 3 | IsActive | bit | NOT NULL | | 1 | 有効=1 / 無効=0 |
| 4 | CreatedAt | datetime2 | NOT NULL | | GETUTC | 作成日時（UTC） |
| 5 | UpdatedAt | datetime2 | NULL | | | 更新日時（UTC） |
| — | UserName | nvarchar(256) | NULL | | | Identity標準（Emailと同値） |
| — | Email | nvarchar(256) | NULL | UQ | | Identity標準 |
| — | PasswordHash | nvarchar(max) | NULL | | | Identity標準（PBKDF2） |
| — | LockoutEnabled | bit | NOT NULL | | 1 | Identity標準 |
| — | AccessFailedCount | int | NOT NULL | | 0 | Identity標準 |

#### AspNetRoles / AspNetUserRoles【フェーズ1 / Identityが自動生成】

| ロール名 | 説明 | フェーズ |
|---|---|---|
| User | 一般利用者 | 1〜 |
| Admin | システム管理者（フェーズ1〜5は全管理操作を担う） | 1〜 |
| Editor | ナレッジ管理者（フェーズ6でAdminから分離） | 6〜 |

#### Faqs（FAQテーブル）【フェーズ1】

| No | カラム名 | データ型 | NULL | キー | デフォルト | 備考 |
|---|---|---|---|---|---|---|
| 1 | Id | int | NOT NULL | PK | IDENTITY | 自動採番 |
| 2 | Title | nvarchar(100) | NOT NULL | | | FAQタイトル |
| 3 | Body | nvarchar(max) | NOT NULL | | | FAQ本文（Markdown可） |
| 4 | CategoryId | int | NOT NULL | FK | | Categories.Id参照 |
| 5 | IsPublished | bit | NOT NULL | | 0 | 公開=1 / 非公開=0 |
| 6 | ViewCount | int | NOT NULL | | 0 | 閲覧数カウント |
| 7 | HelpfulCount | int | NOT NULL | | 0 | 役に立った評価数【余力】 |
| 8 | NotHelpfulCount | int | NOT NULL | | 0 | 役に立たなかった評価数【余力】 |
| 9 | CreatedAt | datetime2 | NOT NULL | | GETUTC | 作成日時（UTC） |
| 10 | UpdatedAt | datetime2 | NULL | | | 更新日時（UTC） |
| 11 | DeletedAt | datetime2 | NULL | | | 論理削除日時。NULLで有効 |

#### Categories（カテゴリテーブル）【フェーズ1】

| No | カラム名 | データ型 | NULL | キー | デフォルト | 備考 |
|---|---|---|---|---|---|---|
| 1 | Id | int | NOT NULL | PK | IDENTITY | 自動採番 |
| 2 | Name | nvarchar(100) | NOT NULL | UQ | | カテゴリ名（一意） |
| 3 | DisplayOrder | int | NOT NULL | | 0 | 表示順 |
| 4 | CreatedAt | datetime2 | NOT NULL | | GETUTC | 作成日時（UTC） |
| 5 | UpdatedAt | datetime2 | NULL | | | 更新日時（UTC） |

#### Tags（タグテーブル）【フェーズ1】

| No | カラム名 | データ型 | NULL | キー | デフォルト | 備考 |
|---|---|---|---|---|---|---|
| 1 | Id | int | NOT NULL | PK | IDENTITY | 自動採番 |
| 2 | Name | nvarchar(50) | NOT NULL | UQ | | タグ名（一意） |
| 3 | CreatedAt | datetime2 | NOT NULL | | GETUTC | 作成日時（UTC） |
| 4 | UpdatedAt | datetime2 | NULL | | | 更新日時（UTC） |

#### FaqTags（FAQタグ中間テーブル）【フェーズ1】

| No | カラム名 | データ型 | NULL | キー | 備考 |
|---|---|---|---|---|---|
| 1 | FaqId | int | NOT NULL | PK / FK | Faqs.Id参照。カスケード削除 |
| 2 | TagId | int | NOT NULL | PK / FK | Tags.Id参照。カスケード削除 |

#### SearchHistories（検索履歴テーブル）【フェーズ1】

| No | カラム名 | データ型 | NULL | キー | デフォルト | 備考 |
|---|---|---|---|---|---|---|
| 1 | Id | int | NOT NULL | PK | IDENTITY | 自動採番 |
| 2 | Keyword | nvarchar(200) | NOT NULL | | | 検索キーワード |
| 3 | UserId | nvarchar(450) | NULL | FK | | AspNetUsers.Id参照。未ログイン時はNULL |
| 4 | SessionId | nvarchar(100) | NULL | | | 未ログイン時のセッション識別子 |
| 5 | SearchedAt | datetime2 | NOT NULL | | GETUTC | 検索実行日時（UTC） |

> 同一UserId（またはSessionId）・同一Keywordは重複登録せず、SearchedAtをUpsert更新する。

#### AiHistories（AI実行履歴テーブル）【フェーズ5】

| No | カラム名 | データ型 | NULL | キー | デフォルト | 備考 |
|---|---|---|---|---|---|---|
| 1 | Id | int | NOT NULL | PK | IDENTITY | 自動採番 |
| 2 | Question | nvarchar(1000) | NOT NULL | | | ユーザーの質問文 |
| 3 | SearchKeywords | nvarchar(500) | NULL | | | 抽出した検索キーワード |
| 4 | AiAnswer | nvarchar(max) | NULL | | | AI生成回答文 |
| 5 | UserId | nvarchar(450) | NULL | FK | | AspNetUsers.Id参照 |
| 6 | IsSuccess | bit | NOT NULL | | 0 | 成功=1 / 失敗=0 |
| 7 | ErrorMessage | nvarchar(2000) | NULL | | | 失敗時のエラーメッセージ |
| 8 | IsHelpful | bit | NULL | | NULL | NULL=未評価 / 1=役立った / 0=役立たなかった |
| 9 | ExecutedAt | datetime2 | NOT NULL | | GETUTC | 実行日時（UTC） |

#### AiHistorySources（AI参照元テーブル）【フェーズ5】

| No | カラム名 | データ型 | NULL | キー | デフォルト | 備考 |
|---|---|---|---|---|---|---|
| 1 | Id | int | NOT NULL | PK | IDENTITY | 自動採番 |
| 2 | AiHistoryId | int | NOT NULL | FK | | AiHistories.Id参照。カスケード削除 |
| 3 | FaqId | int | NOT NULL | FK | | Faqs.Id参照 |
| 4 | Score | float | NULL | | | 関連度スコア（0〜1） |
| 5 | DisplayOrder | int | NOT NULL | | 0 | 表示順（スコア降順） |

#### FaqRatings（FAQ評価テーブル）【余力】

| No | カラム名 | データ型 | NULL | キー | デフォルト | 備考 |
|---|---|---|---|---|---|---|
| 1 | Id | int | NOT NULL | PK | IDENTITY | 自動採番 |
| 2 | FaqId | int | NOT NULL | FK | | Faqs.Id参照 |
| 3 | UserId | nvarchar(450) | NULL | FK | | AspNetUsers.Id参照。未ログイン時はNULL |
| 4 | SessionId | nvarchar(100) | NULL | | | 未ログイン時のセッション識別子 |
| 5 | IsHelpful | bit | NOT NULL | | | 役立った=1 / 役立たなかった=0 |
| 6 | RatedAt | datetime2 | NOT NULL | | GETUTC | 評価日時（UTC） |

> 同一ユーザー（UserId or SessionId）・同一FAQへの重複評価はUpsertで最新値に上書き。

### 5.3 インデックス設計

| テーブル | カラム | 種別 | 目的 |
|---|---|---|---|
| AspNetUsers | Email | 非クラスター化（UQ） | ログイン時のEmail検索 |
| AspNetUsers | IsActive | 非クラスター化 | 有効ユーザーの絞り込み |
| Faqs | CategoryId | 非クラスター化 | カテゴリ絞り込みの高速化 |
| Faqs | IsPublished, DeletedAt | 非クラスター化 | 公開中・有効FAQの絞り込み |
| Faqs | Title（全文） | フルテキスト（FTS） | タイトルキーワード検索【フェーズ2〜】 |
| Faqs | Body（全文） | フルテキスト（FTS） | 本文キーワード検索【フェーズ2〜】 |
| FaqTags | TagId | 非クラスター化 | タグ絞り込みの高速化 |
| SearchHistories | UserId, SearchedAt | 非クラスター化 | ユーザー別履歴取得の高速化 |
| SearchHistories | SessionId, SearchedAt | 非クラスター化 | 未ログインセッション別履歴の高速化 |
| AiHistories | UserId, ExecutedAt | 非クラスター化 | ユーザー別・日付フィルタの高速化 |
| AiHistories | IsSuccess | 非クラスター化 | 成否フィルタの高速化 |
| AiHistorySources | AiHistoryId | 非クラスター化 | 実行履歴ごとの参照元取得の高速化 |

### 5.4 AppDbContext 設計

```csharp
// FaqApp.Infrastructure/Data/AppDbContext.cs
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Faq>              Faqs              => Set<Faq>();
    public DbSet<Category>         Categories        => Set<Category>();
    public DbSet<Tag>              Tags              => Set<Tag>();
    public DbSet<FaqTag>           FaqTags           => Set<FaqTag>();
    public DbSet<SearchHistory>    SearchHistories   => Set<SearchHistory>();
    public DbSet<AiHistory>        AiHistories       => Set<AiHistory>();       // フェーズ5
    public DbSet<AiHistorySource>  AiHistorySources  => Set<AiHistorySource>(); // フェーズ5
    public DbSet<FaqRating>        FaqRatings        => Set<FaqRating>();       // 余力

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // Faq: 論理削除グローバルフィルター
        mb.Entity<Faq>().HasQueryFilter(f => f.DeletedAt == null);

        // FaqTag: 複合PK
        mb.Entity<FaqTag>().HasKey(ft => new { ft.FaqId, ft.TagId });

        // Category・Tag: 一意制約
        mb.Entity<Category>().HasIndex(c => c.Name).IsUnique();
        mb.Entity<Tag>().HasIndex(t => t.Name).IsUnique();

        // AspNetUsers: 拡張カラムのデフォルト値
        mb.Entity<ApplicationUser>()
          .Property(u => u.IsActive).HasDefaultValue(true);
        mb.Entity<ApplicationUser>()
          .Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        // AiHistorySource: カスケード削除
        mb.Entity<AiHistorySource>()
          .HasOne(s => s.AiHistory)
          .WithMany(h => h.Sources)
          .HasForeignKey(s => s.AiHistoryId)
          .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### 5.5 マイグレーション管理

| 項目 | 内容 |
|---|---|
| ツール | Entity Framework Core（dotnet-ef CLI） |
| マイグレーション生成 | `dotnet ef migrations add {Name} --project FaqApp.Infrastructure --startup-project FaqApp.Api` |
| DB更新 | `dotnet ef database update --project FaqApp.Infrastructure --startup-project FaqApp.Api` |
| フェーズ別マイグレーション名 | InitialCreate（フェーズ1）/ AddFtsIndexes（フェーズ2）/ AddAiTables（フェーズ5）/ AddEditorRole（フェーズ6） |

### 5.6 フルテキスト検索のSQL設定【フェーズ2】

```sql
-- FTS カタログ作成（Migration の Up() で実行）
CREATE FULLTEXT CATALOG FaqCatalog AS DEFAULT;

-- Faqs テーブルに FTS インデックスを作成
CREATE FULLTEXT INDEX ON Faqs(Title, Body)
  KEY INDEX PK_Faqs ON FaqCatalog
  WITH CHANGE_TRACKING AUTO;

-- 複数キーワードAND検索（例："CSV 取込 エラー" → '"CSV*" AND "取込*" AND "エラー*"'）
SELECT Id, Title,
  (CASE WHEN CONTAINS(Title, '"CSV*"') THEN 1.0 ELSE 0 END
   + CASE WHEN CONTAINS(Body,  '"CSV*"') THEN 0.3 ELSE 0 END) AS Score
FROM Faqs
WHERE CONTAINS((Title, Body), '"CSV*" AND "取込*" AND "エラー*"')
  AND IsPublished = 1
  AND DeletedAt IS NULL
ORDER BY Score DESC;
```

### 5.7 初期データ（Seeder）

**ロール初期データ（AspNetRoles）**

| ロール名 | 説明 |
|---|---|
| User | 一般利用者 |
| Admin | システム管理者 |
| Editor | ナレッジ管理者（フェーズ6で追加） |

**カテゴリ初期データ（Categories）**

| Id | Name | DisplayOrder |
|---|---|---|
| 1 | 請求処理 | 1 |
| 2 | CSV取込 | 2 |
| 3 | ログイン障害 | 3 |
| 4 | API障害 | 4 |
| 5 | PDF出力 | 5 |
| 6 | 月次締め | 6 |
| 7 | ユーザー権限 | 7 |
| 8 | メール通知 | 8 |
| 9 | システム設定 | 9 |

**管理者ユーザー初期データ（AspNetUsers / ApplicationUser）**

| 項目 | 値 |
|---|---|
| Email | admin@faq-app.local |
| DisplayName | システム管理者 |
| Password | appsettings.json の AdminSeed:Password から取得 |
| IsActive | true |
| ロール（AspNetUserRoles） | Admin |

---

## 6. エラーハンドリング設計

### 6.1 HTTPステータスコード定義

| ステータス | 発生条件 | レスポンスボディ例 |
|---|---|---|
| 200 OK | 正常取得・正常更新 | 各リソースのJSONオブジェクト |
| 201 Created | リソース新規作成成功 | 作成されたリソースのJSONオブジェクト |
| 204 No Content | 削除成功 | 空 |
| 400 Bad Request | バリデーションエラー・不正パラメータ・自己操作禁止違反 | `{ "errors": { "title": ["タイトルは必須です"] } }` |
| 401 Unauthorized | JWTなし・トークン期限切れ・不正 | `{ "message": "認証が必要です" }` |
| 403 Forbidden | 認証済みだがロール不足・アカウント無効 | `{ "message": "この操作には権限が必要です" }` |
| 404 Not Found | 対象リソースが存在しない・非公開FAQに非権限アクセス | `{ "message": "指定されたFAQは存在しません" }` |
| 409 Conflict | 一意制約違反（カテゴリ名・タグ名の重複） | `{ "message": "同名のカテゴリが既に存在します" }` |
| 504 Gateway Timeout | AI API タイムアウト（30秒超過） | `{ "message": "AI回答の生成がタイムアウトしました" }` |
| 500 Internal Server Error | 予期しないサーバーエラー | `{ "message": "サーバーエラーが発生しました" }` |

### 6.2 グローバル例外ハンドラー

```csharp
// Program.cs
app.UseExceptionHandler(errApp => {
    errApp.Run(async ctx => {
        var feature = ctx.Features.Get<IExceptionHandlerFeature>();
        var ex = feature?.Error;

        // 本番環境ではスタックトレースを含めない
        var isDev = app.Environment.IsDevelopment();
        ctx.Response.StatusCode = 500;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsJsonAsync(new {
            message = "サーバーエラーが発生しました",
            detail  = isDev ? ex?.ToString() : null
        });
    });
});
```

### 6.3 フロントエンドのエラー処理

| エラー種別 | 表示方法 | ユーザーへのメッセージ |
|---|---|---|
| 400 バリデーションエラー | 各フィールド下にインライン表示 | APIが返したエラーメッセージをそのまま表示 |
| 401 認証エラー | /login にリダイレクト | 「ログインが必要です」 |
| 403 権限エラー | アクセス禁止ページに遷移 | 「この操作には権限がありません」 |
| 403 アカウント無効 | ログイン画面にメッセージ表示 | 「このアカウントは無効化されています。管理者にお問い合わせください」 |
| 404 見つからない | 404ページコンポーネントを表示 | 「ページが見つかりません」 |
| 504 AI タイムアウト | 画面上部にトースト通知 | 「AI回答の生成がタイムアウトしました。しばらく経ってから再試行してください」 |
| 500 サーバーエラー | 画面上部にトースト通知 | 「サーバーエラーが発生しました。しばらく経ってから再試行してください」 |
| ネットワークエラー | 画面上部にトースト通知 | 「通信エラーが発生しました。接続を確認してください」 |

---

## 7. AI連携設計【フェーズ3〜】

### 7.1 RAGパイプライン

```
ユーザー質問
    ↓
① キーワード抽出
   質問文を形態素解析または正規化してキーワードを抽出
    ↓
② FAQ検索（FTS）
   抽出キーワードでFaqs.Title・Body をフルテキスト検索
   スコア順で上位3〜5件を取得
    ↓
③ コンテキスト生成
   取得したFAQのタイトル・本文をプロンプトに埋め込む
    ↓
④ AI API 呼び出し
   システムプロンプト（ガードレール）＋コンテキスト＋質問文を送信
   タイムアウト：30秒
    ↓
⑤ AI回答受信
    ↓
⑥ AiHistories・AiHistorySources に保存
    ↓
⑦ レスポンス返却（回答＋参照元FAQ＋免責注意文）
```

### 7.2 AIサービス切替設計

| 設定値 | 呼び出し先 | 備考 |
|---|---|---|
| `AiSettings:Provider = "AzureOpenAI"` | Azure OpenAI Service（GPT-4o） | 本番推奨 |
| `AiSettings:Provider = "Claude"` | Anthropic Claude API | 切替可能 |

```csharp
// FaqApp.Infrastructure/ExternalServices/AiApiClient.cs
public class AiApiClient : IAiApiClient
{
    private readonly AiSettings _settings;

    public async Task<string> GenerateSummaryAsync(
        string question, IEnumerable<string> faqContents,
        CancellationToken ct = default)
    {
        var systemPrompt = BuildSystemPrompt();
        var userMessage  = BuildUserMessage(question, faqContents);

        return _settings.Provider switch {
            "AzureOpenAI" => await CallAzureOpenAIAsync(systemPrompt, userMessage, ct),
            "Claude"      => await CallClaudeAsync(systemPrompt, userMessage, ct),
            _ => throw new InvalidOperationException("不明なAIプロバイダーです")
        };
    }
}
```

### 7.3 ガードレールプロンプト

```
【システムプロンプト】
あなたは社内FAQ検索システムのアシスタントです。
以下のルールを厳守してください。

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

【FAQコンテキスト】
{context}

【ユーザーの質問】
{question}
```

---

## 8. 非機能設計

### 8.1 ログ設計

| 項目 | 内容 |
|---|---|
| ログライブラリ | Microsoft.Extensions.Logging + Serilog（JSONシンク） |
| ログレベル | 開発環境：Debug以上 / 本番環境：Warning以上 |
| 出力先 | 開発：コンソール / 本番：Azure Application Insights（フェーズ5で必須化） |
| ログ形式 | `{ "timestamp": "", "level": "", "message": "", "requestId": "", "userId": "", "path": "", "statusCode": "", "durationMs": "" }` |
| ログ対象 | APIリクエスト/レスポンス（パス・メソッド・ステータス・処理時間）・認証成功/失敗・AI API呼び出し（入力トークン数・出力トークン数・処理時間）・例外スタックトレース |
| ログ除外 | パスワード・JWTトークン・AI APIキーはマスキングして記録 |

### 8.2 セキュリティ設計

| 項目 | 内容 |
|---|---|
| HTTPS | 全通信をHTTPS必須（Azure App ServiceのHTTPS強制を有効化） |
| CORS | 許可オリジンをフロントエンドドメインのみに制限 |
| JWT保持 | httpOnlyクッキー（XSS対策） |
| SQLインジェクション | EF Core（パラメータ化クエリ）を使用。直接文字列連結は禁止 |
| XSS対策 | react-markdown の rehype-sanitize でMarkdownをサニタイズ |
| パスワード | ASP.NET Core IdentityのPasswordHasher（PBKDF2）でハッシュ保存 |
| アカウントロック | IdentityのLockoutEnabled（5回失敗で15分ロック）を有効化 |
| プロンプトインジェクション | AIへの入力はシステムプロンプトで役割を明示。ユーザー入力は質問文のみに限定 |
| レート制限 | フェーズ3以降、AI API呼び出しに1ユーザーあたり10回/分のレート制限を設ける |

### 8.3 パフォーマンス設計

| 項目 | 内容 |
|---|---|
| 目標レスポンスタイム | FAQ一覧・検索：2秒以内（同時10ユーザー想定）。AI要約：30秒以内 |
| キャッシュ | カテゴリ・タグ一覧はIMemoryCacheで5分キャッシュ |
| ページネーション | 一覧APIはページネーション必須（最大100件/ページ）。全件取得エンドポイントは設けない |
| N+1対策 | EF CoreのInclude（Eager Loading）でFaqs→Tags・Categoryを一括取得 |
| フルテキスト検索 | SQL Server FTSを使用。LIKEによる全件スキャンを避ける（フェーズ2〜） |
| AI APIコスト管理 | 1回の呼び出しでFAQ最大5件・トークン数を制限（コンテキスト上限4,000トークン） |

---

## 9. 開発・テスト方針

### 9.1 技術スタック一覧

#### フロントエンド

| パッケージ | バージョン | 用途 |
|---|---|---|
| Next.js | 16.x | App Router / RSC / Server Actions |
| React | 19.x | UIフレームワーク |
| TypeScript | 5.x | 型安全な開発 |
| Tailwind CSS | 4.x | ユーティリティCSS |
| react-markdown | 最新 | Markdownレンダリング |
| rehype-sanitize | 最新 | XSS対策サニタイズ |
| react-md-editor | 最新 | Markdownエディタ（FAQ登録・編集） |
| SWR | 最新 | クライアントサイドのデータフェッチ・キャッシュ |
| zod | 最新 | フォームバリデーションスキーマ |
| react-hook-form | 最新 | フォーム状態管理 |
| jose | 最新 | JWTのデコード・検証（Edge Runtime対応） |

#### バックエンド

| パッケージ | バージョン | 用途 |
|---|---|---|
| .NET | 8.x | ランタイム |
| ASP.NET Core | 8.x | Web API フレームワーク |
| EF Core | 8.x | ORM・マイグレーション |
| ASP.NET Core Identity | 8.x | ユーザー管理・認証（ApplicationUser拡張） |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.x | JWT Bearer 認証 |
| FluentValidation | 最新 | リクエストバリデーション |
| Serilog | 最新 | 構造化ログ出力 |
| xUnit | 最新 | 単体テスト |
| Moq | 最新 | モックライブラリ |
| Azure.AI.OpenAI | 最新 | Azure OpenAI Service クライアント【フェーズ3〜】 |
| Anthropic SDK | 最新 | Anthropic Claude API クライアント【フェーズ3〜、切替時】 |

### 9.2 データフェッチ設計（フロントエンド）

| 方式 | 内容 |
|---|---|
| RSC（サーバーコンポーネント） | 一覧・詳細などの初回描画データはRSCでfetchする。Next.js 16のfetchキャッシュ（revalidate設定）を利用 |
| Client Component + SWR | 検索バーからの絞り込み・ページネーションなどインタラクション後の再フェッチはSWRを利用 |
| Server Actions | FAQ登録・編集・削除などのミューテーション処理 |
| APIクライアント（lib/api.ts） | 全APIコールのベースURL・認証クッキー・エラーハンドリングを集約。v1/v2の切替もここで管理 |
| 環境変数 | `NEXT_PUBLIC_API_BASE_URL`（バックエンドAPIのベースURL）、`JWT_SECRET`（ミドルウェア用・サーバーサイドのみ） |

### 9.3 状態管理設計

| 状態の種類 | 管理方法 | 具体例 |
|---|---|---|
| サーバーデータキャッシュ | SWR / Next.js fetchキャッシュ | FAQ一覧・カテゴリ・タグ一覧 |
| URL状態 | URLクエリパラメータ（useSearchParams） | 検索キーワード・カテゴリ・タグ・ページ番号・ソート |
| フォーム状態 | react-hook-form | FAQ登録・編集フォームの入力値・バリデーション状態 |
| 認証状態 | httpOnlyクッキー + React Context | JWTトークン・ログイン中ユーザー情報（displayName・role） |
| トースト通知 | React Context（ToastContext） | 保存成功・削除完了・エラーメッセージ |
| 検索履歴（表示用キャッシュ） | sessionStorage | 検索バードロップダウン用の直近10件（DB保存と同期） |

### 9.4 テスト設計

| テスト種別 | 対象・ツール | 方針 |
|---|---|---|
| 単体テスト（バックエンド） | xUnit / Moq。FaqService・AiService・AuthService のビジネスロジック・バリデーション | カバレッジ70%以上。正常系・異常系・境界値を網羅 |
| 統合テスト（APIテスト） | ASP.NET Core WebApplicationFactory + xUnit + TestContainers（SQL Server） | 主要エンドポイントのリクエスト/レスポンスを検証 |
| AI動作テスト | xUnit + AIサービスのモック | 参照元FAQ0件時にAI呼び出しをスキップすることを確認。ガードレール（禁止ワード出力なし）を確認 |
| E2Eテスト | Playwright（TypeScript） | FAQ検索→詳細、FAQ登録→一覧反映、AI質問→回答表示、未認証→リダイレクト、ロール別アクセス制御 |
| 単体テスト（フロントエンド） | Jest / React Testing Library | コンポーネントの表示ロジック・フォームバリデーション |

#### 主要テストケース（バックエンド）

| 対象 | テストケース | 種別 |
|---|---|---|
| FaqService | 公開FAQのみ返す（未認証時） | 正常系 |
| FaqService | スコア順・AND検索が機能する | 正常系 |
| FaqService | 存在しないIDはnullを返す | 異常系 |
| FaqService | ViewCountが取得時にインクリメントされる | 正常系 |
| FaqService | 論理削除でDeletedAtが設定される | 正常系 |
| AiService | FAQ0件時にAI呼び出しをスキップしmessageを返す | 正常系 |
| AiService | AI APIタイムアウト時に504相当の例外をスローする | 異常系 |
| AuthService | IsActive=falseのユーザーは403を返す | 異常系 |
| UserService | 自己ロール変更・自己無効化は400を返す | 異常系 |

### 9.5 コーディング規約

| 対象 | 規約 |
|---|---|
| C#（バックエンド） | Microsoft C# コーディング規約に準拠。Nullable参照型を有効化。async/awaitを徹底 |
| TypeScript（フロント） | ESLint + Prettier。型推論が難しい箇所は明示的に型を記述。any禁止 |
| API命名 | リソースは複数形・小文字（/faqs, /categories）。動詞はHTTPメソッドで表現 |
| コミット | Conventional Commits（feat:, fix:, docs:, test:, refactor:） |
| ブランチ戦略 | main（本番）/ develop（開発）/ feature/xxx（機能単位） |

---

## 10. インフラ・デプロイ設計

### 10.1 環境一覧

| 環境 | 用途 | URL（例） |
|---|---|---|
| 開発（Dev） | 開発・デバッグ | localhost:3000（フロント）/ localhost:5000（API） |
| ステージング（Stg） | 検証・テスト | https://faq-app-stg.azurewebsites.net |
| 本番（Prod） | 運用 | https://faq-app.azurewebsites.net |

### 10.2 環境変数一覧

#### バックエンド（appsettings.json / Azure 環境変数）

| キー | 説明 | 例 |
|---|---|---|
| ConnectionStrings__DefaultConnection | DB接続文字列 | `Server=(localdb)\mssqllocaldb;Database=FaqAppDb;...` |
| JwtSettings__Secret | JWT署名シークレット（32文字以上） | your-super-secret-key-32chars |
| JwtSettings__Issuer | JWTの発行者 | https://faq-app-api.azurewebsites.net |
| JwtSettings__Audience | JWTの受信者 | https://faq-app.vercel.app |
| JwtSettings__ExpiresInMinutes | JWTの有効期間（分） | 60 |
| AiSettings__Provider | AIプロバイダー種別 | `AzureOpenAI` または `Claude` |
| AiSettings__AzureOpenAI__Endpoint | Azure OpenAI エンドポイント | https://xxx.openai.azure.com/ |
| AiSettings__AzureOpenAI__ApiKey | Azure OpenAI APIキー | （シークレット管理） |
| AiSettings__AzureOpenAI__DeploymentName | デプロイ名 | gpt-4o |
| AiSettings__Claude__ApiKey | Claude APIキー | （シークレット管理） |
| AiSettings__TimeoutSeconds | AI APIタイムアウト（秒） | 30 |
| AdminSeed__Email | 初期管理者メールアドレス | admin@faq-app.local |
| AdminSeed__Password | 初期管理者パスワード | （シークレット管理） |
| AdminSeed__DisplayName | 初期管理者表示名 | システム管理者 |
| AllowedOrigins | CORS許可オリジン | https://faq-app.vercel.app |

#### フロントエンド（.env.local）

| キー | 説明 | 例 |
|---|---|---|
| NEXT_PUBLIC_API_BASE_URL | バックエンドAPIのベースURL | https://faq-app-api.azurewebsites.net |
| JWT_SECRET | ミドルウェアのJWT検証用シークレット（サーバーサイドのみ） | your-super-secret-key-32chars |

### 10.3 GitHub Actions CI/CDワークフロー（概略）

```yaml
# .github/workflows/deploy.yml
name: CI/CD
on:
  push:
    branches: [main]

jobs:
  test-backend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '8.x' }
      - run: dotnet test FaqApp.Tests/ --logger trx

  test-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with: { node-version: '22' }
      - run: npm ci && npm run test && npm run build
        working-directory: frontend/

  deploy-api:
    needs: [test-backend, test-frontend]
    runs-on: ubuntu-latest
    steps:
      - run: dotnet publish -c Release -o ./publish
      - uses: azure/webapps-deploy@v3
        with:
          app-name: faq-app-api-stg
          publish-profile: ${{ secrets.AZURE_PUBLISH_PROFILE }}
          package: ./publish

  deploy-frontend:
    needs: [test-backend, test-frontend]
    runs-on: ubuntu-latest
    steps:
      - uses: amondnet/vercel-action@v25
        with:
          vercel-token: ${{ secrets.VERCEL_TOKEN }}
          vercel-org-id: ${{ secrets.VERCEL_ORG_ID }}
          vercel-project-id: ${{ secrets.VERCEL_PROJECT_ID }}
```

### 10.4 ローカル開発起動手順

| 手順 | コマンド | 備考 |
|---|---|---|
| 1. リポジトリクローン | `git clone {repo-url}` | |
| 2. DBセットアップ | `dotnet ef database update --project FaqApp.Infrastructure --startup-project FaqApp.Api` | LocalDBが自動作成・Seeder実行 |
| 3. バックエンド起動 | `dotnet run --project FaqApp.Api` | localhost:5000 で起動 |
| 4. フロントエンド起動 | `npm install && npm run dev`（frontendディレクトリ） | localhost:3000 で起動 |
| 5. 管理者ログイン | admin@faq-app.local / appsettings の初期パスワード | Seederで自動作成済み |

---

## 11. 付録

### 11.1 主要処理フロー（シーケンス）

#### FAQ検索フロー（フェーズ2）

```
ユーザー → Next.js(RSC) → GET /api/v2/faqs → FaqsController → FaqService → FaqRepository → SQL Server

1. ユーザーが /faqs?keyword=CSV+取込+エラー&sort=score にアクセス
2. Next.js RSC が GET /api/v2/faqs?keyword=...&sort=score を fetch
3. FaqService.GetListAsync() が FaqSearchQuery（keyword・sort・highlight）を受け取る
4. FaqRepository が EF Core + FTS で SQL Server をクエリ
   - キーワードを '"CSV*" AND "取込*" AND "エラー*"' に変換
   - タイトル・本文・カテゴリ名・タグ名を横断検索
   - スコア計算（フィールドごとの重み付け合計）
   - スコア降順でソート・ページネーション
5. PagedResult<FaqSummaryDto>（score・titleHighlighted付き）をレスポンス
6. RSC がHTMLをレンダリングしてブラウザに返す

（クライアントサイド再検索）
7. ユーザーが検索バーにキーワードを入力
8. 500ms デバウンス後に useRouter.push() でURLクエリパラメータを更新
9. Next.js がページを再レンダリング（RSC → API再コール）
```

#### AI要約回答フロー（フェーズ3〜4）

```
ユーザー → Next.js(CC) → POST /api/v1/ai/search → AiController → AiService → [FaqRepository + AiApiClient] → AiHistoryRepository

1. ユーザーが AI質問画面で質問を入力・送信
2. POST /api/v1/ai/search { question: "CSV取込エラーの対処は？" } を fetch
3. AiController がUserロールのJWTを確認
4. AiService.SearchAsync() を実行
   a. 質問文からキーワードを抽出
   b. FaqRepository.SearchAsync() で関連FAQ上位3〜5件を取得
   c. FAQ0件の場合 → AI呼び出しをスキップ・messageを返す
   d. FAQをコンテキストとしてAiApiClient.GenerateSummaryAsync() を呼び出し
   e. AI回答を受信
   f. AiHistories・AiHistorySources に保存
5. レスポンス返却（answer・disclaimer・sources・aiHistoryId）
6. フロントエンドがAiAnswerPanel・AiSourceList をレンダリング
7. ユーザーがフィードバックボタン（👍👎）をクリック
8. POST /api/v1/ai/histories/{aiHistoryId}/feedback を fetch
9. AiHistories.IsHelpful を更新
```

#### JWT認証フロー

```
ユーザー → Next.js(/login) → POST /api/v1/auth/login → AuthService → DB

1. ユーザーがEmail・パスワードを入力して送信
2. POST /api/v1/auth/login にリクエスト
3. AuthService が ApplicationUser を Email で取得
4. IsActive=false の場合 → 403 を返す
5. Identity の PasswordHasher でパスワード照合
6. 照合成功 → JWTを生成（sub・email・role・displayName を Claims に含める）
7. { accessToken, expiresIn: 3600, role, displayName } をレスポンス
8. フロントエンドが accessToken を httpOnlyクッキーにセット
9. middleware.ts が以降のリクエストでクッキーからJWTを取得・ロール検証
```

### 11.2 フェーズ別テーブル追加サマリ

| フェーズ | 追加・変更テーブル | 主な変更内容 |
|---|---|---|
| フェーズ1 | AspNetUsers（拡張）・AspNetRoles・AspNetUserRoles・Faqs・Categories・Tags・FaqTags・SearchHistories | 全基本テーブルの作成。ApplicationUser に DisplayName・IsActive 追加 |
| フェーズ2 | Faqs | FTSインデックス追加（Migration で生SQL実行） |
| フェーズ5 | AiHistories・AiHistorySources | AI実行履歴・参照元テーブルの追加 |
| フェーズ6 | AspNetRoles | Editorロールを追加（Migration + Seeder） |
| 余力 | Faqs（HelpfulCount・NotHelpfulCount追加）・FaqRatings・CategoryStats | 評価・統計機能用テーブルの追加 |