# 基本設計書

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

## 1. システム構成・アーキテクチャ

### 1.1 全体アーキテクチャ

本システムはフロントエンド・バックエンドAPI・データベースの3層構成とし、フロントエンドとバックエンドは独立してデプロイ可能な構成とする。

| 項目 | 内容 |
|---|---|
| フロントエンド | Next.js 16（App Router）/ React / TypeScript |
| バックエンドAPI | ASP.NET Core 8 Web API / C# |
| データベース | SQL Server（本番：Azure SQL Database、開発：SQL Server LocalDB） |
| 認証 | JWT（JSON Web Token）/ ASP.NET Core Identity |
| ホスティング | フロント：Vercel または Azure Static Web Apps / API：Azure App Service |
| CI/CD | GitHub Actions（mainブランチマージ → Staging自動デプロイ） |

### 1.2 レイヤー構成（バックエンド）

バックエンドはClean Architectureに近い4層構成とする。

| レイヤー | 役割 | 主なクラス・ファイル |
|---|---|---|
| API層（Controllers） | HTTPリクエストの受付・レスポンス返却・バリデーション | FaqsController, CategoriesController, TagsController, AuthController |
| アプリケーション層（Services） | ビジネスロジック・ユースケースの実装 | FaqService, CategoryService, TagService, SearchHistoryService, AuthService |
| ドメイン層（Models） | エンティティ・ドメインルールの定義 | Faq, Category, Tag, FaqTag, SearchHistory, ApplicationUser |
| インフラ層（Repositories） | DBアクセス・外部サービス連携 | FaqRepository, CategoryRepository, TagRepository, SearchHistoryRepository |

### 1.3 フロントエンドディレクトリ構成

```
src/
  app/
    faqs/
      page.tsx               # FAQ一覧（S-001）
      [id]/
        page.tsx             # FAQ詳細（S-002）
    admin/
      faqs/
        new/page.tsx         # FAQ登録（S-003）
        [id]/edit/page.tsx   # FAQ編集（S-004）
      categories/page.tsx    # カテゴリ管理（S-005）
      tags/page.tsx          # タグ管理（S-006）
    login/page.tsx           # ログイン画面
  components/
    faq/                     # FAQ関連コンポーネント
    layout/                  # ヘッダー・サイドバー等
    ui/                      # 汎用UIコンポーネント
  lib/
    api.ts                   # APIクライアント（fetch wrapper）
    auth.ts                  # JWT取得・保持ユーティリティ
  types/
    index.ts                 # 型定義（Faq, Category, Tag等）
```

### 1.4 バックエンドディレクトリ構成

```
FaqApp.Api/
  Controllers/
    FaqsController.cs
    CategoriesController.cs
    TagsController.cs
    AuthController.cs
  Services/
    FaqService.cs
    CategoryService.cs
    TagService.cs
    SearchHistoryService.cs
    AuthService.cs
  Models/
    Entities/    Faq.cs, Category.cs, Tag.cs, FaqTag.cs, SearchHistory.cs
    DTOs/        FaqDto.cs, FaqCreateRequest.cs, FaqUpdateRequest.cs ...
  Repositories/
    IFaqRepository.cs / FaqRepository.cs
    ICategoryRepository.cs / CategoryRepository.cs
  Data/
    AppDbContext.cs          # EF Core DbContext
    Migrations/              # EF Core マイグレーション
  Program.cs
  appsettings.json / appsettings.Development.json
```

---

## 2. 認証・認可設計

### 2.1 認証方式

| 項目 | 内容 |
|---|---|
| 認証方式 | JWT（JSON Web Token）Bearer認証 |
| ライブラリ | ASP.NET Core Identity + Microsoft.AspNetCore.Authentication.JwtBearer |
| トークン有効期限 | アクセストークン：60分 / リフレッシュトークン：フェーズ1は未実装 |
| トークン保持 | フロントエンドはlocalStorageまたはhttpOnlyクッキーに保持 |
| ロール | 一般利用者（User）・管理者（Admin）の2種類 |

### 2.2 認証フロー

| ステップ | 処理内容 | エンドポイント |
|---|---|---|
| 1. ログイン要求 | ユーザーがメールアドレスとパスワードを送信 | POST /api/v1/auth/login |
| 2. 認証処理 | ASP.NET Core IdentityでユーザーとパスワードHash照合 | （サーバー内部） |
| 3. JWT発行 | 認証成功時にJWTを生成（ロール情報をClaimsに含める） | （サーバー内部） |
| 4. トークン返却 | `{ accessToken, expiresIn, role }` をレスポンスで返す | （レスポンス） |
| 5. APIアクセス | 以降のリクエストに `Authorization: Bearer {token}` を付与 | 各APIエンドポイント |
| 6. ロール制御 | `[Authorize(Roles = "Admin")]` でController/Actionを保護 | （サーバー内部） |

### 2.3 ロール別アクセス制御

| 操作 | 一般利用者（User） | 管理者（Admin） |
|---|---|---|
| FAQ一覧・詳細閲覧（公開中） | ○ | ○ |
| FAQ一覧・詳細閲覧（非公開） | × | ○ |
| FAQ登録・編集・削除 | × | ○ |
| カテゴリ管理 | × | ○ |
| タグ管理 | × | ○ |
| 検索履歴閲覧（自分） | ○ | ○ |
| 検索履歴閲覧（全体） | × | ○ |

### 2.4 JWT ペイロード設計

```json
{
  "sub":   "user-uuid",
  "email": "user@example.com",
  "role":  "Admin",
  "iat":   1700000000,
  "exp":   1700003600
}
```

---

## 3. 画面設計詳細

### 3.1 共通レイアウト

| 項目 | 内容 |
|---|---|
| ヘッダー | サービス名ロゴ・検索バー・ログイン状態表示（ユーザー名 / ログアウト） |
| サイドバー | 管理者のみ表示。カテゴリ管理・タグ管理・FAQ管理へのリンク |
| フッター | バージョン・著作権表示 |
| レスポンシブ | デスクトップ（1024px以上）を主対象。モバイルはフェーズ2以降 |

### 3.2 画面別詳細

#### S-001: FAQ一覧

| 項目 | 内容 |
|---|---|
| URL | /faqs |
| 主要コンポーネント | SearchBar（キーワード入力・履歴ドロップダウン）、CategoryFilter（セレクト）、TagFilter（複数選択チップ）、FaqCardList（カード一覧）、Pagination |
| 表示データ | FAQカード：タイトル・カテゴリ名・タグチップ・更新日時・閲覧数 |
| 初期表示 | 公開中FAQ全件を更新日時降順で表示。ページサイズ20件 |
| 検索動作 | キーワード入力は500msのデバウンス後にAPIコール。カテゴリ・タグ変更は即時 |
| 権限差異 | 管理者ログイン時は非公開FAQも表示（バッジで「非公開」を明示） |

#### S-002: FAQ詳細

| 項目 | 内容 |
|---|---|
| URL | /faqs/[id] |
| 主要コンポーネント | FaqHeader（タイトル・カテゴリ・タグ・更新日時）、MarkdownRenderer（本文）、RelatedFaqList（同カテゴリの関連FAQ）、編集ボタン（管理者のみ） |
| 本文表示 | react-markdownでMarkdownをHTMLにレンダリング。コードブロックはsyntax-highlighter適用 |
| 閲覧数 | 画面表示時にVIEW_COUNTをインクリメント（APIコール） |
| エラー | 存在しないIDは404ページにリダイレクト |

#### S-003 / S-004: FAQ登録・編集

| 項目 | 内容 |
|---|---|
| URL | /admin/faqs/new（登録） / /admin/faqs/[id]/edit（編集） |
| アクセス制御 | 管理者（Admin）ロールのみアクセス可。未認証または一般利用者はリダイレクト |
| 主要コンポーネント | TitleInput、MarkdownEditor（react-md-editor等）、CategorySelect、TagMultiSelect、PublishedToggle、SaveButton / CancelButton |
| バリデーション | タイトル必須・100文字以内。本文必須。カテゴリ必須。フォーム送信前にクライアント側でチェック |
| 保存動作 | 登録：POST /api/v1/faqs → 成功時は一覧に遷移。編集：PUT /api/v1/faqs/{id} → 成功時は詳細に遷移 |
| エラー表示 | APIエラー時はフォーム上部にエラーメッセージを表示。フィールド単位のエラーは各入力欄の下に表示 |

#### S-005: カテゴリ管理

| 項目 | 内容 |
|---|---|
| URL | /admin/categories |
| アクセス制御 | 管理者（Admin）ロールのみ |
| 主要コンポーネント | CategoryList（名称・表示順・操作ボタン）、CategoryFormModal（追加・編集用インラインダイアログ） |
| 操作 | 追加・編集・削除（削除は確認ダイアログを表示） |
| 削除制約 | FAQが1件以上紐づくカテゴリは削除不可とし、エラーメッセージを表示 |

#### S-006: タグ管理

| 項目 | 内容 |
|---|---|
| URL | /admin/tags |
| アクセス制御 | 管理者（Admin）ロールのみ |
| 主要コンポーネント | TagList（タグ名・FAQ件数・操作ボタン）、TagFormModal |
| 操作 | 追加・編集・削除（確認ダイアログあり） |
| 削除動作 | タグ削除時はFaqTagsの紐づきも合わせて削除（カスケード） |

#### ログイン画面

| 項目 | 内容 |
|---|---|
| URL | /login |
| 主要コンポーネント | EmailInput、PasswordInput、LoginButton |
| 成功時 | JWTをlocalStorageに保存 → ロールに応じてリダイレクト（Admin → /admin/faqs、User → /faqs） |
| 失敗時 | 「メールアドレスまたはパスワードが正しくありません」を画面上部に表示 |

---

## 4. API詳細設計

### 4.1 共通仕様

| 項目 | 内容 |
|---|---|
| ベースURL | /api/v1 |
| Content-Type | application/json |
| 認証ヘッダー | `Authorization: Bearer {JWT}`（管理者操作・任意JWT操作に必要） |
| レスポンス形式 | JSON。成功時は200/201、バリデーションエラーは400、認証エラーは401、未認可は403、見つからない場合は404 |
| ページネーション | クエリパラメータ page（1始まり）・pageSize（最大100）。レスポンスに total・page・pageSize・items を含む |
| 日時形式 | ISO 8601形式（例：2026-05-07T09:00:00Z） |
| エラー形式 | `{ "errors": { "fieldName": ["エラーメッセージ"] } }`（400時） |

### 4.2 認証API

#### POST /api/v1/auth/login

| 項目 | 内容 |
|---|---|
| 概要 | ログイン認証・JWTトークン発行 |
| 認証 | 不要 |
| リクエストボディ | `{ "email": "string", "password": "string" }` |
| レスポンス（200） | `{ "accessToken": "string", "expiresIn": 3600, "role": "Admin" \| "User" }` |
| レスポンス（401） | `{ "message": "メールアドレスまたはパスワードが正しくありません" }` |
| 備考 | パスワードはASP.NET Core Identityのハッシュで検証 |

### 4.3 FAQ API

#### GET /api/v1/faqs

| 項目 | 内容 |
|---|---|
| 概要 | FAQ一覧取得（キーワード・カテゴリ・タグ絞り込み、ページネーション） |
| 認証 | 任意（JWTなし→公開のみ、管理者JWT→全件） |
| クエリパラメータ | keyword: string / categoryId: int / tagId: int / page: int（default 1）/ pageSize: int（default 20） |
| レスポンス（200） | `{ "total": int, "page": int, "pageSize": int, "items": [ FaqSummary ] }` |
| FaqSummary形式 | `{ "id": int, "title": string, "categoryId": int, "categoryName": string, "tags": [{id, name}], "isPublished": bool, "viewCount": int, "updatedAt": datetime }` |
| ソート | 更新日時降順（固定） |

#### GET /api/v1/faqs/{id}

| 項目 | 内容 |
|---|---|
| 概要 | FAQ詳細取得 |
| 認証 | 任意（非公開FAQは管理者JWTが必要、未認証または一般利用者は404を返す） |
| レスポンス（200） | `{ "id": int, "title": string, "body": string, "categoryId": int, "categoryName": string, "tags": [{id, name}], "isPublished": bool, "viewCount": int, "createdAt": datetime, "updatedAt": datetime }` |
| 副作用 | 取得成功時にViewCountを+1インクリメント |
| レスポンス（404） | `{ "message": "指定されたFAQは存在しません" }` |

#### POST /api/v1/faqs

| 項目 | 内容 |
|---|---|
| 概要 | FAQ新規登録 |
| 認証 | 要（JWT・Adminロール） |
| リクエストボディ | `{ "title": string（必須・最大100文字）, "body": string（必須）, "categoryId": int（必須）, "tagIds": int[]（任意）, "isPublished": bool（default: false）}` |
| レスポンス（201） | 登録されたFAQ詳細オブジェクト |
| バリデーション | title必須・100文字以内。body必須。categoryIdは存在するカテゴリIDであること。tagIdsは存在するタグIDのみ指定可 |

#### PUT /api/v1/faqs/{id}

| 項目 | 内容 |
|---|---|
| 概要 | FAQ更新 |
| 認証 | 要（JWT・Adminロール） |
| リクエストボディ | POST /api/v1/faqs と同形式 |
| レスポンス（200） | 更新後のFAQ詳細オブジェクト |
| レスポンス（404） | 対象FAQが存在しない場合 |

#### DELETE /api/v1/faqs/{id}

| 項目 | 内容 |
|---|---|
| 概要 | FAQ削除（論理削除） |
| 認証 | 要（JWT・Adminロール） |
| レスポンス（204） | No Content |
| 削除仕様 | DeletedAtに現在日時を設定。物理削除は行わない。以降のGET APIには含まれない |

### 4.4 カテゴリ・タグ API

| エンドポイント | 概要 | 認証 | レスポンス形式 |
|---|---|---|---|
| GET /api/v1/categories | カテゴリ一覧取得（DisplayOrder昇順） | 任意 | `[{ "id": int, "name": string, "displayOrder": int }]` |
| POST /api/v1/categories | カテゴリ登録 | 要（Admin） | `{ "id": int, "name": string, "displayOrder": int }` |
| PUT /api/v1/categories/{id} | カテゴリ更新 | 要（Admin） | 更新後のカテゴリオブジェクト |
| DELETE /api/v1/categories/{id} | カテゴリ削除 | 要（Admin） | 204 No Content / 400（FAQ紐づきあり） |
| GET /api/v1/tags | タグ一覧取得（名前昇順） | 任意 | `[{ "id": int, "name": string }]` |
| POST /api/v1/tags | タグ登録 | 要（Admin） | `{ "id": int, "name": string }` |
| PUT /api/v1/tags/{id} | タグ更新 | 要（Admin） | 更新後のタグオブジェクト |
| DELETE /api/v1/tags/{id} | タグ削除（FaqTagsもカスケード削除） | 要（Admin） | 204 No Content |

### 4.5 検索履歴 API

#### POST /api/v1/search-histories

| 項目 | 内容 |
|---|---|
| 概要 | 検索キーワードをDB保存 |
| 認証 | 不要（セッションIDで識別） |
| リクエストボディ | `{ "keyword": string, "sessionId": string（任意）}` |
| レスポンス（201） | `{ "id": int, "keyword": string, "searchedAt": datetime }` |
| 重複排除 | 同一sessionId・同一keywordが存在する場合はSearchedAtを更新（Upsert） |

#### GET /api/v1/search-histories

| 項目 | 内容 |
|---|---|
| 概要 | 検索履歴一覧取得（直近10件、SearchedAt降順） |
| 認証 | 不要（sessionIdをクエリパラメータで指定） |
| クエリパラメータ | sessionId: string |
| レスポンス（200） | `[{ "id": int, "keyword": string, "searchedAt": datetime }]` |

---

## 5. データベース詳細設計

### 5.1 ER図（テーブル関連）

```
Categories  ──< Faqs >──< FaqTags >── Tags
                          （多対多：FaqTagsが中間テーブル）

Faqs の CategoryId は Categories.Id を参照（FK）
FaqTags の FaqId は Faqs.Id を参照（FK）
FaqTags の TagId は Tags.Id を参照（FK）

SearchHistories は独立テーブル（FK制約なし）
AspNetUsers（ASP.NET Core Identity）は認証用テーブル
```

### 5.2 テーブル定義

#### Faqs（FAQテーブル）

| No | カラム名 | データ型 | NULL | キー | デフォルト | 備考 |
|---|---|---|---|---|---|---|
| 1 | Id | int | NOT NULL | PK | AUTO | 自動採番（IDENTITY） |
| 2 | Title | nvarchar(100) | NOT NULL | | | FAQタイトル |
| 3 | Body | nvarchar(max) | NOT NULL | | | FAQ本文（Markdown可） |
| 4 | CategoryId | int | NOT NULL | FK | | Categories.Id参照 |
| 5 | IsPublished | bit | NOT NULL | | 0 | 公開=1 / 非公開=0 |
| 6 | ViewCount | int | NOT NULL | | 0 | 閲覧数カウント |
| 7 | CreatedAt | datetime2 | NOT NULL | | GETUTC | 作成日時（UTC） |
| 8 | UpdatedAt | datetime2 | NULL | | | 更新日時（UTC） |
| 9 | DeletedAt | datetime2 | NULL | | | 論理削除日時。NULLで有効 |

#### Categories（カテゴリテーブル）

| No | カラム名 | データ型 | NULL | キー | デフォルト | 備考 |
|---|---|---|---|---|---|---|
| 1 | Id | int | NOT NULL | PK | AUTO | 自動採番 |
| 2 | Name | nvarchar(100) | NOT NULL | UQ | | カテゴリ名（一意） |
| 3 | DisplayOrder | int | NOT NULL | | 0 | 表示順 |
| 4 | CreatedAt | datetime2 | NOT NULL | | GETUTC | 作成日時（UTC） |
| 5 | UpdatedAt | datetime2 | NULL | | | 更新日時（UTC） |

#### Tags（タグテーブル）

| No | カラム名 | データ型 | NULL | キー | デフォルト | 備考 |
|---|---|---|---|---|---|---|
| 1 | Id | int | NOT NULL | PK | AUTO | 自動採番 |
| 2 | Name | nvarchar(50) | NOT NULL | UQ | | タグ名（一意） |
| 3 | CreatedAt | datetime2 | NOT NULL | | GETUTC | 作成日時（UTC） |
| 4 | UpdatedAt | datetime2 | NULL | | | 更新日時（UTC） |

#### FaqTags（FAQ・タグ中間テーブル）

| No | カラム名 | データ型 | NULL | キー | 備考 |
|---|---|---|---|---|---|
| 1 | FaqId | int | NOT NULL | PK / FK | Faqs.Id参照。カスケード削除 |
| 2 | TagId | int | NOT NULL | PK / FK | Tags.Id参照。カスケード削除 |

#### SearchHistories（検索履歴テーブル）

| No | カラム名 | データ型 | NULL | キー | デフォルト | 備考 |
|---|---|---|---|---|---|---|
| 1 | Id | int | NOT NULL | PK | AUTO | 自動採番 |
| 2 | Keyword | nvarchar(200) | NOT NULL | | | 検索キーワード |
| 3 | SessionId | nvarchar(100) | NULL | | | セッション識別子 |
| 4 | SearchedAt | datetime2 | NOT NULL | | GETUTC | 検索実行日時（UTC） |

> ※ AspNetUsers・AspNetRoles 等のASP.NET Core Identityテーブルは自動生成のため省略。

### 5.3 インデックス設計

| テーブル | カラム | 種別 | 目的 |
|---|---|---|---|
| Faqs | CategoryId | 非クラスター化 | カテゴリ絞り込み検索の高速化 |
| Faqs | IsPublished, DeletedAt | 非クラスター化 | 公開中・有効FAQの絞り込み |
| Faqs | Title（全文） | フルテキスト | キーワード検索（SQL Server FTS） |
| Faqs | Body（全文） | フルテキスト | 本文キーワード検索 |
| FaqTags | TagId | 非クラスター化 | タグ絞り込み検索の高速化 |
| SearchHistories | SessionId, SearchedAt | 非クラスター化 | セッション別履歴取得の高速化 |

### 5.4 初期データ（Seeder）

アプリケーション起動時にEF Core Data Seedingで以下のデータを投入する。

**カテゴリ初期データ**

| Id | Name | DisplayOrder |
|---|---|---|
| 1 | 請求処理 | 1 |
| 2 | CSV取込 | 2 |
| 3 | ログイン障害 | 3 |
| 4 | API障害 | 4 |
| 5 | PDF出力 | 5 |
| 6 | 月次締め | 6 |
| 7 | ユーザー権限 | 7 |

**管理者ユーザー初期データ**

| 項目 | 値 |
|---|---|
| Email | admin@faq-app.local |
| Password | （appsettings.json の AdminSeed:Password から取得） |
| Role | Admin |

---

## 6. エラーハンドリング設計

### 6.1 HTTPステータスコード定義

| ステータス | 発生条件 | レスポンスボディ例 |
|---|---|---|
| 200 OK | 正常取得・正常更新 | 各リソースのJSONオブジェクト |
| 201 Created | リソース新規作成成功 | 作成されたリソースのJSONオブジェクト |
| 204 No Content | 削除成功 | 空 |
| 400 Bad Request | バリデーションエラー・不正パラメータ | `{ "errors": { "title": ["タイトルは必須です"] } }` |
| 401 Unauthorized | JWTなし・トークン期限切れ・不正 | `{ "message": "認証が必要です" }` |
| 403 Forbidden | 認証済みだがロール不足 | `{ "message": "この操作には管理者権限が必要です" }` |
| 404 Not Found | 対象リソースが存在しない | `{ "message": "指定されたFAQは存在しません" }` |
| 409 Conflict | 一意制約違反（カテゴリ名・タグ名の重複等） | `{ "message": "同名のカテゴリが既に存在します" }` |
| 500 Internal Server Error | 予期しないサーバーエラー | `{ "message": "サーバーエラーが発生しました" }` |

### 6.2 グローバル例外ハンドラー

ASP.NET Core の `UseExceptionHandler` ミドルウェアを使用し、未処理の例外をすべてキャッチして500レスポンスに変換する。本番環境ではスタックトレースをレスポンスに含めない。

```csharp
// Program.cs
app.UseExceptionHandler(errApp => {
    errApp.Run(async ctx => {
        ctx.Response.StatusCode = 500;
        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsJsonAsync(
            new { message = "サーバーエラーが発生しました" });
    });
});
```

### 6.3 フロントエンドのエラー処理

| エラー種別 | 表示方法 | ユーザーへのメッセージ |
|---|---|---|
| 400 バリデーションエラー | 各フィールド下にインラインエラー表示 | APIが返したエラーメッセージをそのまま表示 |
| 401 認証エラー | ログイン画面にリダイレクト | 「ログインが必要です」 |
| 403 権限エラー | アクセス禁止ページに遷移 | 「この操作には権限がありません」 |
| 404 見つからない | 404ページコンポーネントを表示 | 「ページが見つかりません」 |
| 500 サーバーエラー | 画面上部にトーストまたはバナー通知 | 「サーバーエラーが発生しました。しばらく経ってから再試行してください」 |
| ネットワークエラー | 画面上部にトースト通知 | 「通信エラーが発生しました。接続を確認してください」 |

---

## 7. 非機能設計

### 7.1 ログ設計

| 項目 | 内容 |
|---|---|
| ログライブラリ | Microsoft.Extensions.Logging（構造化ログ）+ Serilog（JSONシンク） |
| ログレベル | 開発環境：Debug以上 / 本番環境：Warning以上 |
| 出力先 | 開発：コンソール / 本番：Azure Application Insights（フェーズ1はオプション） |
| ログ形式 | `{ "timestamp": "", "level": "", "message": "", "requestId": "", "userId": "" }` |
| ログ対象 | APIリクエスト/レスポンス（パス・メソッド・ステータス・処理時間）、認証成功/失敗、例外スタックトレース |
| ログ除外 | パスワード・JWTトークン等の機密情報はマスキングして記録 |

### 7.2 セキュリティ設計

| 項目 | 内容 |
|---|---|
| HTTPS | 全通信をHTTPS必須（Azure App ServiceのHTTPS強制を有効化） |
| CORS | 許可オリジンをフロントエンドドメインのみに制限 |
| SQLインジェクション | EF Core（パラメータ化クエリ）を使用し直接文字列連結は行わない |
| XSS対策 | フロントエンドはreact-markdownのsanitize機能を使用。ユーザー入力をそのままdangerouslySetInnerHTMLに渡さない |
| パスワード | ASP.NET Core IdentityのPasswordHasher（PBKDF2）でハッシュ保存 |
| レート制限 | フェーズ1は未実装。フェーズ2で検討 |

### 7.3 パフォーマンス設計

| 項目 | 内容 |
|---|---|
| 目標レスポンスタイム | FAQ一覧・検索：2秒以内（同時10ユーザー想定） |
| キャッシュ | カテゴリ・タグ一覧はメモリキャッシュ（IMemoryCache）で5分キャッシュ |
| ページネーション | 一覧APIはページネーション必須（最大100件/ページ）。全件取得エンドポイントは設けない |
| N+1対策 | EF CoreのInclude（Eager Loading）でFaqs→Tags・Categoryを一括取得 |
| フルテキスト検索 | SQL Server Full-Text Search（FTS）を使用。LIKEによる全件スキャンを避ける |

---

## 8. 開発・テスト方針

### 8.1 開発環境セットアップ

| 項目 | 内容 |
|---|---|
| フロントエンド | Node.js 22 / npm / Next.js 16。`npm run dev` でローカル起動（localhost:3000） |
| バックエンド | .NET 8 SDK / Visual Studio 2022 または VS Code + C# Dev Kit。`dotnet run` でローカル起動（localhost:5000） |
| データベース | SQL Server LocalDB（Visual Studio付属）または Docker（mcr.microsoft.com/mssql/server:2022-latest） |
| マイグレーション | `dotnet ef migrations add InitialCreate` → `dotnet ef database update` |
| 環境変数 | appsettings.Development.json にConnectionStrings・JwtSettings・AdminSeedを定義 |

### 8.2 テスト設計

| テスト種別 | 対象・ツール | 方針・カバレッジ目標 |
|---|---|---|
| 単体テスト（バックエンド） | xUnit / Moq。Serviceクラスのビジネスロジック・バリデーション | カバレッジ70%以上。正常系・異常系・境界値を網羅 |
| 統合テスト（APIテスト） | ASP.NET Core WebApplicationFactory + xUnit | 主要エンドポイントのリクエスト/レスポンスを検証。TestContainersでSQL Server起動 |
| E2Eテスト | Playwright（TypeScript） | 主要シナリオ：FAQ検索→詳細、FAQ登録→一覧確認、カテゴリ追加。CI上で実行 |
| 単体テスト（フロントエンド） | Jest / React Testing Library | コンポーネントの表示ロジック・フォームバリデーション |

### 8.3 コーディング規約

| 対象 | 規約 |
|---|---|
| C#（バックエンド） | Microsoft C# コーディング規約に準拠。Nullable参照型を有効化。async/awaitを徹底 |
| TypeScript（フロント） | ESLint + Prettier。型推論が難しい箇所は明示的に型を記述。any禁止 |
| API命名 | リソースは複数形・小文字（/faqs, /categories）。動詞はHTTPメソッドで表現 |
| コミット | Conventional Commits（feat:, fix:, docs:, test:, refactor:） |
| ブランチ戦略 | main（本番）/ develop（開発）/ feature/xxx（機能単位） |