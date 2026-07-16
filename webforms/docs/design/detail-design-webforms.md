# 詳細設計書

**社内FAQ・業務ナレッジ検索アプリ — ASP.NET Web Forms版**  
**FAQ Knowledge Search - ASP.NET Web Forms**

| 項目 | 内容 |
|---|---|
| プロジェクト名 | 社内FAQ・業務ナレッジ検索アプリ |
| 対象システム | ASP.NET Web Forms版 |
| ドキュメント種別 | 詳細設計書 |
| バージョン | 1.0 |
| 作成日 | 2026/07/16 |
| 更新日 | 2026/07/16 |
| 作成者 | — |
| 承認者 | — |

---

## 改訂履歴

| バージョン | 日付 | 変更内容 | 作成者 |
|---|---|---|---|
| 1.0 | 2026/07/16 | Web Forms版の要件定義書、基本設計書、画面CodeBehind、Service、DTO、DbContext、Identity、OWIN認証およびAI連携実装を基に初版作成 | — |

---

## 1. 本書の目的

本書は、社内FAQ・業務ナレッジ検索アプリのASP.NET Web Forms版について、クラス、メソッド、画面イベント、DTO、データアクセス、認証・認可、OpenAI API連携、入力検証、例外処理およびデータ更新方法を実装単位で定義することを目的とする。

本書は現行ソースコードを基にしたAs-Built設計書である。画面レイアウト、サーバーコントロールの配置、Validatorの種類など、ASPXマークアップに依存する項目については、確認できたCodeBehind上のコントロール名と処理を記載し、未確認の表示属性は断定しない。

---

## 2. 対象範囲・前提

### 2.1 対象機能

- 公開FAQ一覧・検索
- 公開FAQ詳細・閲覧数加算
- 管理者FAQ一覧・検索・ページング
- FAQ新規登録・編集・論理削除
- AI FAQ検索
- OpenAI API呼び出し
- AI検索成功・失敗履歴保存
- AI検索履歴一覧・詳細
- AI回答フィードバック
- 管理者ログイン・ログアウト
- 管理画面共通認可
- ユーザー一覧・有効／無効切り替え
- Entity Framework 6によるMySQLアクセス
- OWIN Cookie Authentication
- 設定ファイル・環境変数の読み込み

### 2.2 対象外

- ASPXのCSS・HTMLを含むピクセル単位の画面レイアウト
- SQLスクリプト全文
- ASP.NET Identity標準テーブルの全カラム定義
- `ApplicationUserManager`のパスワード・ロックアウト設定値
- 自動E2Eテストの実装
- IIS本番配置手順
- Next.js + ASP.NET Core版の詳細設計

### 2.3 設計上の基準資料

- `requirements-webforms.md`
- `basic-design-webforms.md`
- ER図
- 一般利用者・管理者向け画面遷移図
- 現行のCodeBehind、Service、DTO、DbContext、Identity、OWIN設定
- `database/*.sql`

---

## 3. 内部アーキテクチャ

### 3.1 処理構成

```text
ASPX / UserControl
      ↓ PostBack・ページイベント
CodeBehind
      ↓ メソッド呼び出し
Service / AI Client
      ↓ DTO・Entity
DbContext / UserManager
      ↓ EF6 / ASP.NET Identity 2
MySQL

AI検索時：
CodeBehind → AiService → FaqService → AiApiClient → OpenAI API
                         ↓
                  AiSearchHistoryService → MySQL
```

### 3.2 責務

| 要素 | 責務 |
|---|---|
| ASPX | サーバーコントロール、Validator、画面レイアウトを定義する |
| CodeBehind | ページライフサイクル、入力取得、画面状態、Service呼び出し、リダイレクトを制御する |
| Service | 検索条件、入力検証、業務ルール、DB登録・更新を実行する |
| DTO | CodeBehindとService間のデータを保持する |
| Entity | MySQLへ永続化する業務データを表す |
| DbContext | EF6による検索、関連マッピング、保存を管理する |
| Identity | ユーザー、ロール、パスワード、ロックアウト、SecurityStampを管理する |
| OWIN | Cookie認証ミドルウェアとSecurityStamp検証を構成する |
| AI Client | OpenAI APIへのHTTP通信とレスポンス解析を行う |

### 3.3 依存関係の生成

現行実装はDIコンテナを使用せず、CodeBehindのコンストラクター、フィールド初期化、または`Page_Init`で具象クラスを生成する。

```text
Faqs.Index / Faqs.Detail
  └─ new FaqService()

Admin.Faqs.Index / Admin.Faqs.Edit
  └─ new FaqService()

Admin.Users.Index
  └─ new AdminUserService()

Admin.AiHistories.Index
  └─ new AiSearchHistoryQueryService()

AiSearch.Index
  ├─ AiSettings.Load()
  ├─ new FaqService()
  ├─ new AiApiClient(settings)
  ├─ new AiSearchHistoryService()
  ├─ new AiSearchFeedbackService()
  └─ new AiService(...)
```

Serviceにはインターフェースを用意しているため、テスト時には代替実装へ差し替えられる。ただし画面側の生成方法は手動生成である。

---

## 4. 名前空間・主要クラス一覧

### 4.1 画面・共通UI

| 名前空間／クラス | 種別 | 役割 |
|---|---|---|
| `FaqKnowledgeSearch.WebForms.Faqs.Index` | Page | 公開FAQ一覧・検索 |
| `FaqKnowledgeSearch.WebForms.Faqs.Detail` | Page | 公開FAQ詳細 |
| `FaqKnowledgeSearch.WebForms.AiSearch.Index` | Page | AI FAQ検索・フィードバック |
| `FaqKnowledgeSearch.WebForms.Account.Login` | Page | 管理者ログイン |
| `FaqKnowledgeSearch.WebForms.Account.Logout` | Page | ログアウト確認・実行 |
| `FaqKnowledgeSearch.WebForms.Admin.AdminPageBase` | Page基底クラス | 管理画面の認証・Admin認可 |
| `FaqKnowledgeSearch.WebForms.Admin.Default` | Page | 管理トップ |
| `FaqKnowledgeSearch.WebForms.Admin.Faqs.Index` | Page | 管理者FAQ一覧 |
| `FaqKnowledgeSearch.WebForms.Admin.Faqs.Edit` | Page | FAQ新規登録・編集 |
| `FaqKnowledgeSearch.WebForms.Admin.AiHistories.Index` | Page | AI検索履歴一覧 |
| `FaqKnowledgeSearch.WebForms.Admin.AiHistories.Detail` | Page | AI検索履歴詳細 |
| `FaqKnowledgeSearch.WebForms.Admin.Users.Index` | Page | ユーザー管理 |
| `FaqKnowledgeSearch.WebForms.Admin.AdminHeader` | UserControl | 管理画面メニューの選択状態制御 |

### 4.2 Service

| インターフェース | 実装 | 役割 |
|---|---|---|
| `IFaqService` | `FaqService` | FAQ検索、詳細、登録、編集、論理削除、選択肢取得 |
| `IAdminUserService` | `AdminUserService` | ユーザー一覧、状態変更 |
| `IAiService` | `AiService` | AI検索のオーケストレーション |
| `IAiApiClient` | `AiApiClient` | OpenAI API通信 |
| `IAiSearchHistoryService` | `AiSearchHistoryService` | 成功・失敗履歴保存 |
| `IAiSearchHistoryQueryService` | `AiSearchHistoryQueryService` | 履歴一覧・詳細取得 |
| `IAiSearchFeedbackService` | `AiSearchFeedbackService` | フィードバック登録・更新 |

### 4.3 データ・認証

| クラス | 役割 |
|---|---|
| `FaqKnowledgeDbContext` | FAQ・カテゴリ・タグ・AI履歴・フィードバック用DbContext |
| `ApplicationIdentityDbContext` | ASP.NET Identity用DbContext |
| `ApplicationUser` | Identityユーザー拡張クラス |
| `Startup` | OWIN Cookie認証設定 |
| `AiSettings` | AI設定読み込み |

---

## 5. DbContext・データアクセス設計

### 5.1 `FaqKnowledgeDbContext`

| 項目 | 内容 |
|---|---|
| 継承元 | `System.Data.Entity.DbContext` |
| 接続文字列名 | `FaqKnowledgeDb` |
| Provider | MySQL Entity Framework Provider |
| DB初期化 | `Database.SetInitializer(null)`により無効 |
| スキーマ管理 | `database/*.sql`を正とする |

#### DbSet

| プロパティ | Entity |
|---|---|
| `Categories` | `Category` |
| `Faqs` | `Faq` |
| `Tags` | `Tag` |
| `AiSearchHistories` | `AiSearchHistory` |
| `AiSearchHistorySources` | `AiSearchHistorySource` |
| `AiSearchFeedbacks` | `AiSearchFeedback` |

#### Fluent APIマッピング

| 関係 | 設計 |
|---|---|
| Category 1 : N Faq | `Faq.CategoryId`を外部キーとし、削除カスケードを無効化する |
| Faq N : N Tag | 中間テーブル`faq_tags`、キー`faq_id`・`tag_id`を使用する |
| AiSearchHistory 1 : N Source | `AiSearchHistorySource.AiSearchHistoryId`を外部キーとし、履歴削除時は参照元をカスケード削除する |

### 5.2 `ApplicationIdentityDbContext`

| 項目 | 内容 |
|---|---|
| 継承元 | `IdentityDbContext<ApplicationUser>` |
| 接続文字列名 | `FaqKnowledgeDb` |
| スキーマ | ASP.NET Identity 2標準テーブル＋`ApplicationUser`追加項目 |
| Factory | `Create()`で新しいContextを返す |

業務データ用ContextとIdentity用Contextは論理的に分離するが、物理的には同一のMySQLデータベースへ接続する。

### 5.3 トランザクション単位

- 各Serviceメソッドは原則として1つのDbContextを生成し、1回の`SaveChanges`または`SaveChangesAsync`で更新する。
- EF6の単一`SaveChanges`内の更新はDBトランザクションとして扱われる。
- OpenAI API呼び出しとAI検索履歴保存は同一トランザクションではない。
- AI回答生成後に履歴保存が失敗しても、AI回答は利用者へ返却する。
- 複数Serviceをまたぐ分散トランザクションは実装しない。

### 5.4 ER図

![Web Forms版 ER図](../images/webforms/ERD.drawio.png)

---

## 6. Identityユーザー詳細設計

### 6.1 `ApplicationUser`

`IdentityUser`を継承し、以下の項目を追加する。

| プロパティ | 型 | 制約・初期値 | 内容 |
|---|---|---|---|
| `DisplayName` | `string` | 必須、最大100文字 | 管理画面表示名 |
| `IsActive` | `bool` | 生成時`true` | 有効状態 |
| `CreatedAt` | `DateTime` | 生成時`DateTime.UtcNow` | 作成日時 |

### 6.2 `GenerateUserIdentityAsync`

| 項目 | 内容 |
|---|---|
| 入力 | `UserManager<ApplicationUser>` |
| 出力 | `ClaimsIdentity` |
| 認証タイプ | `DefaultAuthenticationTypes.ApplicationCookie` |
| 処理 | `manager.CreateIdentityAsync`を呼び出す |

### 6.3 Identityテーブル

主に次のテーブルを使用する。

| テーブル | 用途 |
|---|---|
| `AspNetUsers` | ユーザー、認証情報、表示名、有効状態、作成日時 |
| `AspNetRoles` | ロール |
| `AspNetUserRoles` | ユーザーとロールの関連 |
| その他Identity標準テーブル | Claim、外部ログイン等。現行機能で未使用のものを含む |

---

## 7. DTO詳細設計

### 7.1 FAQ系DTO

#### `FaqListItemDto`

| プロパティ | 型 | 用途 |
|---|---|---|
| `Id` | `long` | FAQ ID |
| `CategoryName` | `string` | カテゴリ名 |
| `Question` | `string` | 質問・タイトル |
| `Answer` | `string` | 回答本文 |
| `IsPublished` | `bool` | 公開状態 |
| `IsDeleted` | `bool` | 論理削除状態 |
| `ViewCount` | `int` | 閲覧数 |
| `UpdatedAt` | `DateTime` | 更新日時 |
| `TagNames` | `string` | カンマ区切りタグ名 |

#### `FaqDetailDto`

| プロパティ | 型 | 用途 |
|---|---|---|
| `Id` | `long` | FAQ ID |
| `CategoryName` | `string` | カテゴリ名 |
| `Question` | `string` | 質問 |
| `Answer` | `string` | 回答 |
| `ViewCount` | `int` | 加算後閲覧数 |
| `UpdatedAt` | `DateTime` | 更新日時 |
| `TagNames` | `string` | カンマ区切りタグ名 |

#### `FaqEditDto`

| プロパティ | 型 | 用途 |
|---|---|---|
| `Id` | `long` | 編集対象ID |
| `CategoryId` | `long` | カテゴリID |
| `Question` | `string` | 質問・タイトル |
| `Answer` | `string` | 回答本文 |
| `IsPublished` | `bool` | 公開状態 |
| `SelectedTagIds` | `IList<int>` | 選択タグID一覧。コンストラクターで空Listを設定 |

#### 選択肢DTO

| DTO | プロパティ | 補足 |
|---|---|---|
| `CategoryOptionDto` | `Id`, `Name`, `IsActive`, `DisplayName` | 無効カテゴリは表示名に「（無効）」を付与する |
| `TagOptionDto` | `Id`, `Name`, `DisplayOrder` | タグチェックボックス表示用 |

### 7.2 AI検索DTO

#### `AiSearchRequest`

| プロパティ | 型 | 用途 |
|---|---|---|
| `Question` | `string` | AI検索質問文 |

#### `AiSearchResponse`

| プロパティ | 型 | 用途 |
|---|---|---|
| `Answer` | `string` | AI回答。未生成時は`null` |
| `Disclaimer` | `string` | 注意文 |
| `Sources` | `IList<AiSourceDto>` | 参照元FAQ。コンストラクターで空Listを設定 |
| `Message` | `string` | FAQ0件・API失敗等の画面メッセージ |
| `AiHistoryId` | `long` | 保存した履歴ID。保存失敗時は0 |

#### `AiSourceDto`

| プロパティ | 型 | 用途 |
|---|---|---|
| `FaqId` | `long` | 参照元FAQ ID |
| `Title` | `string` | FAQ質問 |
| `Url` | `string` | 公開FAQ詳細URL |

### 7.3 AI履歴DTO

#### `AiSearchHistoryListItemDto`

| プロパティ | 型 | 用途 |
|---|---|---|
| `Id` | `long` | 履歴ID |
| `Question` | `string` | 質問 |
| `AnswerPreview` | `string` | 回答プレビュー。回答なしは`null` |
| `IsSuccess` | `bool` | 成否 |
| `ErrorMessage` | `string` | 失敗内容 |
| `SourceCount` | `int` | 参照元件数 |
| `IsHelpful` | `bool?` | `true`役立った、`false`役立たなかった、`null`未評価 |
| `ExecutedAt` | `DateTime` | DB上はUTC |

#### `AiSearchHistoryDetailDto`

| プロパティ | 型 | 用途 |
|---|---|---|
| `Id` | `long` | 履歴ID |
| `Question` | `string` | 質問 |
| `SearchKeywords` | `string` | 検索キーワード。現行は質問文と同値 |
| `AiAnswer` | `string` | AI回答 |
| `IsSuccess` | `bool` | 成否 |
| `ErrorMessage` | `string` | 失敗内容 |
| `ExecutedAt` | `DateTime` | UTC |
| `Sources` | `IList<AiSearchHistorySourceDto>` | 参照元一覧 |
| `IsHelpful` | `bool?` | 評価 |

#### `AiSearchHistorySourceDto`

| プロパティ | 型 | 用途 |
|---|---|---|
| `FaqId` | `long` | FAQ ID |
| `FaqQuestion` | `string` | 保存時点の質問 |
| `FaqAnswer` | `string` | 保存時点の回答 |
| `CategoryName` | `string` | 保存時点のカテゴリ名 |
| `DisplayOrder` | `int` | 表示順 |
| `Url` | `string` | 現在の公開FAQ詳細URL。非公開・削除後はリンク先で未存在となり得る |

### 7.4 ユーザー管理DTO

#### `AdminUserListItemDto`

| プロパティ | 型 | 用途 |
|---|---|---|
| `Id` | `string` | IdentityユーザーID |
| `DisplayName` | `string` | 表示名 |
| `Email` | `string` | メールアドレス |
| `RoleName` | `string` | 代表ロール |
| `IsActive` | `bool` | 有効状態 |
| `CreatedAt` | `DateTime` | UTC作成日時 |
| `IsAdmin` | `bool` | `RoleName`がAdminかを判定する計算プロパティ |
| `CanChangeStatus` | `bool` | Admin以外の場合`true` |

#### `AdminUserPageDto`

| プロパティ | 型 | 用途 |
|---|---|---|
| `Items` | `IList<AdminUserListItemDto>` | 現在ページのユーザー |
| `TotalCount` | `int` | 総件数 |
| `PageNumber` | `int` | 1始まりのページ番号 |
| `PageSize` | `int` | 1ページ件数 |
| `TotalPages` | `int` | 算出プロパティ |
| `HasPreviousPage` | `bool` | 前ページ有無 |
| `HasNextPage` | `bool` | 次ページ有無 |

---

## 8. 管理画面共通認可詳細設計

### 8.1 対象クラス

`FaqKnowledgeSearch.WebForms.Admin.AdminPageBase`

### 8.2 `OnPreInit`

| 順序 | 条件・処理 |
|---:|---|
| 1 | `Context.User.Identity.IsAuthenticated`を確認する |
| 2 | 未認証の場合は`RedirectToLogin`を実行し、その後の処理を終了する |
| 3 | 認証済みの場合は`Context.User.IsInRole("Admin")`を確認する |
| 4 | Admin以外の場合はHTTP 403の`HttpException`を送出する |
| 5 | Adminの場合のみ`base.OnPreInit`を実行する |

### 8.3 `RedirectToLogin`

```text
Request.RawUrl
  ↓ URLエンコード
~/Account/Login.aspx?ReturnUrl={encoded URL}
  ↓
Response.Redirect(loginUrl, true)
```

未認証ユーザーが管理画面へ直接アクセスした場合、元画面を`ReturnUrl`に保持する。

### 8.4 管理画面継承関係

```text
System.Web.UI.Page
  └─ AdminPageBase
      ├─ Admin.Default
      ├─ Admin.Faqs.Index
      ├─ Admin.Faqs.Edit
      ├─ Admin.AiHistories.Index
      ├─ Admin.AiHistories.Detail
      └─ Admin.Users.Index
```

---

## 9. OWIN Cookie認証詳細設計

### 9.1 `Startup.Configuration`

| 設定 | 値 |
|---|---|
| Context | `ApplicationIdentityDbContext.Create`をOWIN単位で生成 |
| UserManager | `ApplicationUserManager.Create`をOWIN単位で生成 |
| AuthenticationType | `ApplicationCookie` |
| Cookie名 | `.FaqKnowledgeSearch.Auth` |
| LoginPath | `/Account/Login.aspx` |
| 有効期間 | 30分 |
| SlidingExpiration | `true` |
| HttpOnly | `true` |
| Secure | `SameAsRequest` |
| SecurityStamp検証間隔 | 30分 |

### 9.2 SecurityStamp再検証

30分間隔で`SecurityStampValidator.OnValidateIdentity`を実行し、Identityを再生成する。ユーザー無効化時には`AdminUserService`がSecurityStampを変更するため、既存Cookieは次回検証時に無効と判断され得る。

現行設定ではSecurityStamp検証間隔が30分のため、無効化済みユーザーの既存セッションが即時に切断されるとは限らない。

---

## 10. 管理者ログイン画面詳細設計

### 10.1 対象

| 項目 | 内容 |
|---|---|
| URL | `/Account/Login.aspx` |
| CodeBehind | `Account.Login` |
| 利用者 | 未認証の管理者 |
| 認証方式 | メールアドレス＋パスワード |

### 10.2 `Page_Load`

- 初期状態でログインエラーパネルを非表示にする。
- 初回表示かつ既に認証済みの場合は`RedirectAfterLogin`を実行する。

### 10.3 `LoginButton_Click`

| 順序 | 処理 |
|---:|---|
| 1 | `Page.IsValid`が`false`なら終了する |
| 2 | メールアドレスをTrimし、パスワードを取得する |
| 3 | OWIN Contextから`ApplicationUserManager`とAuthenticationを取得する |
| 4 | `FindByEmailAsync`でユーザーを取得する |
| 5 | ユーザーなしの場合は共通認証エラーを表示する |
| 6 | `IsLockedOutAsync`でロックアウト状態を確認する |
| 7 | `CheckPasswordAsync`でパスワードを検証する |
| 8 | パスワード不一致時は`AccessFailedAsync`で失敗回数を加算する |
| 9 | `IsInRoleAsync(user.Id, "Admin")`でAdminロールを確認する |
| 10 | 非Adminの場合は共通認証エラーを表示する |
| 11 | 成功時は失敗回数をリセットする |
| 12 | `GenerateUserIdentityAsync`でClaimsIdentityを生成する |
| 13 | 既存ApplicationCookieをSignOutする |
| 14 | RememberMeに応じた`IsPersistent`でSignInする |
| 15 | `RedirectAfterLogin`を実行する |

### 10.4 エラーメッセージ

| 条件 | 表示メッセージ |
|---|---|
| ユーザーなし | メールアドレスまたはパスワードが正しくありません。 |
| パスワード不一致 | メールアドレスまたはパスワードが正しくありません。 |
| Adminロールなし | メールアドレスまたはパスワードが正しくありません。 |
| ロックアウト | ログイン試行回数の上限に達しました。しばらくしてから再度お試しください。 |

ユーザーの存在やロールを推測されにくくするため、通常の認証失敗は同一メッセージを使用する。表示前に`HttpUtility.HtmlEncode`を適用する。

### 10.5 リダイレクト

`ReturnUrl`が次の形式を満たす場合のみ遷移先として採用する。

- `/`で始まり、2文字目が`/`または`\`ではない
- または`~/`で始まる

不正または未指定の場合は`~/Admin/Default.aspx`へ遷移する。これにより外部URLへのオープンリダイレクトを防止する。

### 10.6 `IsActive`に関する現行実装

`Login.aspx.cs`の確認範囲では、パスワード検証前後に`ApplicationUser.IsActive`を明示的に判定していない。無効ユーザーの新規ログインを禁止する要件を完全に満たすには、`ApplicationUserManager`側の追加処理またはログイン処理への判定追加が必要である。詳細は「実装整合確認事項」を参照する。

---

## 11. ログアウト画面詳細設計

### 11.1 `Page_Load`

初回表示かつ未認証の場合はトップ画面へ遷移する。認証済みの場合はログアウト確認画面を表示する。

### 11.2 `LogoutButton_Click`

| 順序 | 処理 |
|---:|---|
| 1 | OWIN Authenticationを取得する |
| 2 | ApplicationCookieをSignOutする |
| 3 | `Session.Clear()`を実行する |
| 4 | `Session.Abandon()`を実行する |
| 5 | レスポンスキャッシュを`NoCache`にする |
| 6 | `SetNoStore()`を実行する |
| 7 | `~/Default.aspx`へ遷移する |

---

## 12. 管理画面ヘッダー詳細設計

### 12.1 `AdminHeader`

`ActiveItem`に応じて各メニューリンクのCSSクラスを設定する。

| `ActiveItem` | 選択状態になるリンク |
|---|---|
| `faq` | FAQ一覧 |
| `new` | FAQ新規登録 |
| `users` | ユーザー一覧 |

`OnPreRender`で各リンクへ`admin-menu-button`を設定し、対象項目には`admin-menu-button--active`を追加する。

AI検索履歴メニューの選択状態は、確認済みの`AdminHeader.ascx.cs`では専用項目として定義されていない。

---

## 13. 公開FAQ一覧画面詳細設計

### 13.1 対象

| 項目 | 内容 |
|---|---|
| URL | `/Faqs/Index.aspx` |
| CodeBehind | `Faqs.Index` |
| Service | `IFaqService` / `FaqService` |
| ページサイズ | 5件 |
| ページ状態 | `ViewState["CurrentPageIndex"]`、0始まり |

### 13.2 イベント

| イベント | 処理 |
|---|---|
| `Page_Load` | 初回表示時にページ番号0を設定し、FAQをバインドする |
| `SearchButton_Click` | ページ番号を0に戻し、入力キーワードで再検索する |
| `ClearButton_Click` | キーワードを空にし、ページ番号0で再検索する |
| `PreviousButton_Click` | 現在ページが1ページ目より後なら1減算する |
| `NextButton_Click` | 現在ページを1加算し、`BindFaqs`内で範囲補正する |

### 13.3 `BindFaqs`

| 順序 | 処理 |
|---:|---|
| 1 | `KeywordTextBox.Text`をTrimする |
| 2 | `SearchPublishedFaqs`を実行する |
| 3 | 全件数と総ページ数を計算する |
| 4 | 0件または範囲外ページを補正する |
| 5 | `Skip`・`Take`により現在ページの5件をメモリ上で抽出する |
| 6 | `FaqRepeater`へDataBindする |
| 7 | 件数、空表示、ページャー、ページ番号を更新する |
| 8 | 前後ボタンのEnabledを更新する |

### 13.4 表示加工

#### 回答プレビュー

- 改行を空白へ変換してTrimする。
- 最大140文字とする。
- 超過時は末尾に`...`を付ける。
- 回答が空の場合は「回答内容は詳細画面で確認できます。」を返す。

#### タグ

- 区切り文字は`,`、`、`、CR、LFとする。
- 前後空白と先頭`#`を除去する。
- 大文字小文字を無視して重複を除外する。
- 一覧では最大5件を表示する。

### 13.5 例外時

- Repeaterを空にする。
- 件数を0件とする。
- 空表示パネルとページャーを非表示にする。
- `faq-message--error`でメッセージを表示する。
- 現行実装では基底例外のメッセージをHTMLエンコードして画面へ含める。
- 例外全文をDebug出力する。

---

## 14. 公開FAQ詳細画面詳細設計

### 14.1 入力

| 項目 | 内容 |
|---|---|
| クエリ文字列 | `id` |
| 型 | 正の`long` |
| 不正時 | HTTP 400、エラーパネル表示 |

### 14.2 `LoadFaq`

| 順序 | 処理 |
|---:|---|
| 1 | `id`を`long`へ変換する |
| 2 | 不正な場合は「FAQ IDが正しくありません。」を表示する |
| 3 | `GetPublishedFaqById`を実行する |
| 4 | `null`の場合はHTTP 404を設定する |
| 5 | 質問、回答、カテゴリ、タグ、閲覧数、更新日時を画面へ設定する |
| 6 | FAQパネルを表示し、エラーパネルを非表示にする |

### 14.3 表示形式

| 項目 | 形式 |
|---|---|
| 閲覧数 | `N0` |
| 更新日時 | `yyyy/MM/dd HH:mm` |
| タグ | カンマ・読点・改行で分割、先頭`#`除去、重複除外 |

### 14.4 HTTPステータス

| 条件 | ステータス | メッセージ |
|---|---:|---|
| ID不正 | 400 | FAQ IDが正しくありません。 |
| 対象なし | 404 | 指定されたFAQは見つかりませんでした。 |
| 取得例外 | 500 | FAQの取得に失敗しました。 |

`Response.TrySkipIisCustomErrors = true`を設定し、ページ内エラー表示を維持する。

---

## 15. 管理者FAQ一覧画面詳細設計

### 15.1 対象

| 項目 | 内容 |
|---|---|
| URL | `/Admin/Faqs/Index.aspx` |
| 継承 | `AdminPageBase` |
| Service | `IFaqService` / `FaqService` |
| ページサイズ | 5件 |
| ページ状態 | `FaqGridView.PageIndex` |

### 15.2 初期表示

1. PostBackの場合は終了する。
2. クエリ文字列`keyword`、`status`から検索条件を復元する。
3. GridViewのページサイズを5、PageIndexを0に設定する。
4. FAQ一覧をバインドする。
5. クエリ文字列`message`に応じて完了メッセージを表示する。

### 15.3 イベント

| イベント | 処理 |
|---|---|
| `SearchButton_Click` | PageIndexを0へ戻して検索 |
| `ClearButton_Click` | キーワード・公開状態を初期化して検索 |
| `PreviousPageButton_Click` | PageIndexを1減算して検索 |
| `NextPageButton_Click` | PageIndexを1加算して検索 |
| `FaqGridView_RowCommand` | `DeleteFaq`コマンド時に論理削除 |

### 15.4 検索条件

| DropDown値 | Serviceへ渡す値 |
|---|---|
| `published` | `true` |
| `unpublished` | `false` |
| その他 | `null` |

### 15.5 一覧表示加工

- 公開状態は「公開」「非公開」で表示する。
- カテゴリ名のキーワードに応じてBootstrapバッジクラスを切り替える。
- 回答プレビューは改行を空白にし、最大105文字とする。
- 登録件数と`現在ページ / 総ページ`を表示する。

### 15.6 削除処理

```text
RowCommand(DeleteFaq)
  ↓ FAQ ID検証
FaqService.DeleteFaq
  ├─ false：対象なしメッセージ、再バインド
  └─ true：/Admin/Faqs/Index.aspx?message=deleted へ遷移
```

例外時はTraceへ記録し、「FAQの削除中にエラーが発生しました。」を表示する。

### 15.7 完了メッセージ

| `message` | 表示内容 |
|---|---|
| `created` | FAQを登録しました。 |
| `updated` | FAQを更新しました。 |
| `deleted` | FAQを削除しました。 |

すべて`Server.HtmlEncode`を適用する。

---

## 16. FAQ新規登録・編集画面詳細設計

### 16.1 画面モード

| モード | 条件 | 動作 |
|---|---|---|
| 新規登録 | `id`なし | 空フォーム、非公開初期値、ボタン「登録する」 |
| 編集 | 正の`id`あり | 既存データを表示、ボタン「更新する」 |
| エラー | 不正IDまたは対象なし | 入力を無効化し、保存ボタンを非表示 |

### 16.2 `Page_Load`

1. エラーパネルを非表示にする。
2. `TryGetFaqId`でモードを決定する。
3. 不正IDならエラーモードにする。
4. 管理ヘッダーのActiveItemを、新規は`new`、編集は`faq`にする。
5. PostBackの場合はデータ再読込を行わない。
6. 新規または編集モードの初期表示を構成する。

### 16.3 `SaveButton_Click`

| 順序 | 処理 |
|---:|---|
| 1 | `Page.Validate("FaqEdit")`を実行する |
| 2 | `Page.IsValid == false`なら終了する |
| 3 | FAQ IDとカテゴリIDを検証する |
| 4 | 質問・回答をTrimする |
| 5 | 質問500文字、回答4000文字を超える場合は画面エラーとする |
| 6 | 選択タグIDを収集し、重複を除外する |
| 7 | `FaqEditDto`を生成する |
| 8 | 編集は`UpdateFaq`、新規は`CreateFaq`を呼ぶ |
| 9 | 成功時は一覧画面へリダイレクトする |
| 10 | `ArgumentException`・`InvalidOperationException`はメッセージを画面表示する |
| 11 | その他例外はTraceへ記録し、固定メッセージを表示する |

### 16.4 入力制約

| 項目 | 制約 | 検証場所 |
|---|---|---|
| カテゴリ | 必須、正のID | CodeBehind・Service |
| 質問／タイトル | 必須、最大500文字 | Validator・CodeBehind・Service |
| 回答本文 | 必須、画面上最大4000文字 | Validator・CodeBehind・Service（必須） |
| 公開状態 | bool | CodeBehind |
| タグ | 0件以上、存在するIDのみ | CodeBehind・Service |

### 16.5 カテゴリ選択肢

- 新規登録時は有効カテゴリのみ取得する。
- 編集時は、現在選択されているカテゴリが無効でも選択肢に含める。
- 無効カテゴリの表示名には「（無効）」を付ける。
- 先頭に「選択してください」を挿入する。

### 16.6 タグ選択肢

- `DisplayOrder`、IDの順で取得する。
- 編集時は既存選択タグをチェック状態にする。
- タグ0件時はチェックボックスを非表示にし、空表示パネルを表示する。

### 16.7 遷移

| 処理 | 遷移先 |
|---|---|
| 新規登録成功 | `/Admin/Faqs/Index.aspx?message=created` |
| 更新成功 | `/Admin/Faqs/Index.aspx?message=updated` |

---

## 17. `FaqService`詳細設計

### 17.1 メソッド一覧

| メソッド | 戻り値 | 概要 |
|---|---|---|
| `SearchPublishedFaqs(string)` | `IReadOnlyList<FaqListItemDto>` | 公開FAQ検索 |
| `GetPublishedFaqById(long)` | `FaqDetailDto` | 公開FAQ詳細・閲覧数加算 |
| `SearchFaqsForAdmin(string, bool?)` | `IReadOnlyList<FaqListItemDto>` | 管理者FAQ検索 |
| `GetCategoryOptions(long?)` | `IReadOnlyList<CategoryOptionDto>` | カテゴリ選択肢 |
| `GetTagOptions()` | `IReadOnlyList<TagOptionDto>` | タグ選択肢 |
| `GetFaqForAdmin(long)` | `FaqEditDto` | 編集用FAQ取得 |
| `CreateFaq(FaqEditDto)` | `long` | FAQ登録 |
| `UpdateFaq(long, FaqEditDto)` | `bool` | FAQ更新 |
| `DeleteFaq(long)` | `bool` | FAQ論理削除 |

### 17.2 `SearchPublishedFaqs`

#### 検索条件

```text
IsPublished = true
AND IsDeleted = false
AND Category.IsActive = true
AND（キーワードなし、または次のいずれかをContains）
  - Question
  - Answer
  - Category.Name
  - Tags.Name
```

#### 取得方法

- `AsNoTracking`を使用する。
- CategoryとTagsをIncludeする。
- キーワードをTrimする。
- 更新日時降順、ID降順で並べる。
- DTO変換はDB取得後に行い、タグ名を表示順・ID順でカンマ結合する。

現行の検索は入力文字列全体に対する部分一致であり、単語分割、全文検索、関連度スコアリングは行わない。

### 17.3 `GetPublishedFaqById`

- IDが0以下の場合は`null`を返す。
- 公開済み、未削除、有効カテゴリのFAQを1件取得する。
- 取得できた場合は`ViewCount`を1加算し、`SaveChanges`する。
- 加算後の値をDTOへ設定する。

閲覧数加算は楽観的同時実行制御を使用していないため、同時アクセス時に更新競合で加算が失われる可能性がある。

### 17.4 `SearchFaqsForAdmin`

- 未削除FAQのみ対象とする。
- キーワードは質問、回答、カテゴリ、タグを検索する。
- `isPublished`指定時のみ公開状態を絞り込む。
- 更新日時降順、ID降順で返す。
- 画面側でメモリページングするため、Serviceは該当全件を返す。

### 17.5 `GetCategoryOptions`

- 有効カテゴリを表示順、ID順で返す。
- `includeCategoryId`指定時は、そのカテゴリが無効でも取得対象に含める。

### 17.6 `GetTagOptions`

全タグを表示順、ID順で返す。

### 17.7 `GetFaqForAdmin`

- IDが0以下の場合は`null`を返す。
- 未削除FAQを対象とする。
- TagsをIncludeし、選択タグIDを表示順・ID順で返す。

### 17.8 `ValidateFaqInput`

| 条件 | 例外メッセージ |
|---|---|
| DTOが`null` | `ArgumentNullException("input")` |
| カテゴリIDが0以下 | カテゴリを選択してください。 |
| 質問が空 | 質問を入力してください。 |
| 質問が500文字超 | 質問は500文字以内で入力してください。 |
| 回答が空 | 回答を入力してください。 |

質問・回答はTrimし、タグIDは正数のみ、重複なしへ正規化する。

### 17.9 `CreateFaq`

1. 入力を検証する。
2. 指定カテゴリが有効であることを確認する。
3. 指定タグを取得し、存在件数を検証する。
4. 作成・更新日時に`DateTime.Now`を設定する。
5. `IsDeleted=false`、`ViewCount=0`でEntityを生成する。
6. タグ関連を追加する。
7. `SaveChanges`し、採番IDを返す。

### 17.10 `UpdateFaq`

1. IDが0以下なら`false`を返す。
2. 入力を検証する。
3. 未削除FAQとタグを取得する。
4. 対象なしなら`false`を返す。
5. カテゴリが有効、または現在のカテゴリと同じことを確認する。
6. 指定タグの存在を確認する。
7. 質問、回答、カテゴリ、公開状態、更新日時を更新する。
8. 現在のタグ関連をClearし、選択タグを再追加する。
9. `SaveChanges`し、`true`を返す。

### 17.11 `DeleteFaq`

- IDが0以下、または未削除対象がない場合は`false`を返す。
- `IsDeleted=true`、`IsPublished=false`、`UpdatedAt=DateTime.Now`へ更新する。
- 物理削除は行わない。

---

## 18. AI FAQ検索画面詳細設計

### 18.1 対象

| 項目 | 内容 |
|---|---|
| URL | `/AiSearch/Index.aspx` |
| CodeBehind | `AiSearch.Index` |
| 主Service | `IAiService` |
| フィードバック | `IAiSearchFeedbackService` |
| 認証 | 不要 |

### 18.2 `Page_Init`

1. `AiSettings.Load()`で設定を取得する。
2. `FaqService`を生成する。
3. `AiApiClient`を設定付きで生成する。
4. `AiSearchHistoryService`を生成する。
5. `AiSearchFeedbackService`を生成する。
6. 上記を`AiService`へ渡す。

### 18.3 `SearchButton_Click`

#### 検索前初期化

- メッセージパネルを非表示にする。
- 結果パネルを非表示にする。
- 履歴ID HiddenFieldを空にする。
- フィードバックパネルと完了パネルを非表示にする。
- 完了メッセージを空にする。

#### 処理フロー

```text
Page.IsValid確認
  ↓
質問文Trim
  ↓
IP単位レート制限確認
  ├─ 超過：待機秒数を表示して終了
  └─ 許可
       ↓
検索ボタン無効化
       ↓
AiService.SearchAsync
       ↓
結果に応じて回答・参照元・メッセージ・フィードバックを表示
       ↓
finallyで検索ボタン再有効化
```

### 18.4 結果別画面制御

| 条件 | 回答パネル | 参照元 | フィードバック |
|---|---|---|---|
| 回答あり、履歴ID>0 | 表示 | 表示 | 表示 |
| 回答あり、履歴ID=0 | 表示 | 表示 | 非表示 |
| 回答なし、参照元あり | 回答欄空で結果パネル表示 | 表示 | 非表示 |
| FAQなし | Serviceメッセージのみ | 非表示 | 非表示 |
| 画面処理例外 | 固定エラーメッセージ | 非表示 | 非表示 |

### 18.5 レート制限

| 項目 | 内容 |
|---|---|
| 上限 | 1分間に5回 |
| 単位 | `Request.UserHostAddress`で取得したクライアントIP |
| 保存先 | `HttpRuntime.Cache` |
| キー | `ai-search-rate-limit:{IP}` |
| 期間 | 初回許可時刻から1分固定 |
| 排他 | static `RateLimitSync`でlock |
| 時刻 | UTC |
| 超過時 | 期限までの残秒を切り上げて表示 |
| Sliding | なし |

IPが取得できない場合は`unknown`を使用する。信頼済みプロキシ設定がないため、`X-Forwarded-For`は使用しない。

#### 制約

- アプリケーションプロセス再起動でカウンターが失われる。
- 複数IISインスタンス間で共有されない。
- 同一グローバルIP利用者は上限を共有する。
- プロキシ配下ではプロキシIP単位になる可能性がある。

### 18.6 出力エンコード

AI回答は`HttpUtility.HtmlEncode`後に改行を`<br />`へ変換する。注意文と画面メッセージもHTMLエンコードする。

### 18.7 フィードバック処理

| イベント | `isHelpful` |
|---|---:|
| `HelpfulButton_Click` | `true` |
| `NotHelpfulButton_Click` | `false` |

`SaveFeedbackAsync`の処理：

1. HiddenFieldから履歴IDを取得する。
2. 正の`long`でなければエラー表示する。
3. 両評価ボタンを無効化する。
4. `SaveAsync(historyId, isHelpful, null)`を呼ぶ。
5. 成功メッセージを表示する。
6. 例外時はTraceへ記録し、固定メッセージを表示する。
7. `finally`で両ボタンを再有効化する。

現行画面はコメント入力を持たず、Serviceへ`null`を渡す。

---

## 19. `AiService`詳細設計

### 19.1 依存関係

- `IFaqService`
- `IAiApiClient`
- `IAiSearchHistoryService`
- `AiSettings`

コンストラクター引数が`null`の場合は`ArgumentNullException`を送出する。

### 19.2 `SearchAsync`

#### 入力検証

| 条件 | 応答 |
|---|---|
| 空白 | `Message="質問文を入力してください。"`、履歴保存なし |
| 500文字超 | `Message="質問文は500文字以内で入力してください。"`、履歴保存なし |

#### 参照FAQ件数

`AiSettings.MaxContextFaqCount`が1以上なら設定値を使用し、それ以外は5件とする。

#### 関連FAQ取得

```csharp
_faqService.SearchPublishedFaqs(normalizedQuestion)
    .Take(maximumCount)
```

現行実装は質問全文をFAQの部分一致キーワードとして使用し、`FaqService`の更新日時降順結果から先頭N件を取得する。意味ベース検索や関連度順ソートは行わない。

#### FAQ0件

- OpenAI APIを呼び出さない。
- 失敗履歴の保存を試行する。
- 「該当するFAQが見つかりませんでした。キーワードを変えて検索してください。」を返す。

#### FAQコンテキスト

各FAQを次の形式で連結する。

```text
[FAQ1]
ID: {Id}
カテゴリ: {CategoryName or 未分類}
タグ: {TagNames or なし}
質問:
{Question or 質問が登録されていません。}
回答:
{Answer or 回答が登録されていません。}
```

FAQ間は空行で区切る。

#### API成功

1. AI回答を取得する。
2. 成功履歴保存を試行する。
3. 回答、注意文、参照元、履歴IDを返す。

注意文：

```text
この回答はFAQをもとに生成されています。必ず参照元を確認してください。
```

#### API失敗

- 例外全文をTraceへ記録する。
- DBへは固定エラーメッセージのみ保存する。
- 失敗履歴保存を試行する。
- 回答は`null`、参照元FAQは返し、画面メッセージを設定する。

#### 履歴保存失敗

`TrySaveSuccessHistoryAsync`および`TrySaveFailureHistoryAsync`は例外を捕捉し、Traceへ記録後に0を返す。AI検索結果の返却は継続する。

---

## 20. `AiApiClient`詳細設計

### 20.1 HTTPクライアント

| 項目 | 内容 |
|---|---|
| インスタンス | static readonly `HttpClient` |
| タイムアウト | 30秒 |
| メソッド | POST |
| Content-Type | `application/json; charset=utf-8` |
| 認証 | `Authorization: Bearer {ApiKey}` |
| JSON | Newtonsoft.Json |

### 20.2 設定検証

| 不足項目 | 例外メッセージ |
|---|---|
| APIキー | AI APIキーが設定されていません。 |
| エンドポイント | AI APIエンドポイントが設定されていません。 |
| モデル | AIモデルが設定されていません。 |

質問またはFAQコンテキストが空の場合は`ArgumentException`を送出する。

### 20.3 リクエスト

```json
{
  "model": "設定値",
  "instructions": "システムプロンプト",
  "input": "FAQコンテキストと質問",
  "max_output_tokens": 1000
}
```

### 20.4 システムプロンプト方針

- FAQコンテキストのみを根拠にする。
- FAQにない内容を断言しない。
- 手順、原因、担当部署、問い合わせ先を推測しない。
- 機密情報、個人情報、認証情報を出力しない。
- 簡潔で分かりやすい日本語とする。
- 最後に「詳細は参照元FAQをご確認ください。」を付ける。

### 20.5 ユーザープロンプト

- FAQコンテキスト
- 利用者の質問
- 回答形式指示
- 根拠がない場合の固定表現

を連結する。

### 20.6 レスポンス解析

- JSONルートの`output`配列を走査する。
- 各要素の`content`配列を走査する。
- `type == "output_text"`の`text`を収集する。
- 複数テキストは改行で連結する。
- 取得できない場合は「AI回答が空でした。」として例外にする。

HTTP失敗時はステータスコードをTrace Warningへ記録し、利用者向けには固定例外を送出する。APIレスポンス全文はログや履歴へ保存しない。

---

## 21. AI検索履歴保存詳細設計

### 21.1 `AiSearchHistoryService`

#### 公開メソッド

| メソッド | 成否 | 保存内容 |
|---|---|---|
| `SaveSuccessAsync` | 成功 | 質問、回答、参照元 |
| `SaveFailureAsync` | 失敗 | 質問、エラー、参照元 |

両メソッドは共通の`SaveAsync`を呼び出す。

### 21.2 入力正規化

| 項目 | 処理 |
|---|---|
| 質問 | Trim、空なら例外、500文字超は切り詰め |
| 成功回答 | Trim、成功なのに空なら例外 |
| 失敗エラー | 空なら「AI検索に失敗しました。」、最大1000文字 |
| 検索キーワード | 現行は質問文をそのまま保存 |
| 実行日時 | `DateTime.UtcNow` |

### 21.3 参照元スナップショット

- `null`要素、ID 0以下を除外する。
- FAQ ID単位で重複を除外する。
- 表示順は入力順＋1とする。
- 質問は最大500文字、未設定時は「質問未設定」。
- カテゴリ名は最大100文字。
- 回答はTrimして保存する。

履歴保存後にFAQが編集・非公開・削除されても、当時の回答根拠を確認できるようにスナップショットを保持する。

---

## 22. AI検索履歴検索・詳細取得Service設計

### 22.1 `AiSearchHistoryQueryService.SearchHistories`

#### 検索条件

キーワードがある場合、次のいずれかを`Contains`検索する。

- `Question`
- `SearchKeywords`
- `AiAnswer`
- `ErrorMessage`

`isSuccess`指定時は成否を絞り込む。

#### 並び順・加工

- 実行日時降順、ID降順。
- 履歴と参照元を取得する。
- 対象履歴IDのフィードバックを1回のクエリでDictionary化する。
- 回答プレビューは改行を空白へ変換し、最大120文字とする。

### 22.2 `GetHistoryById`

- IDが0以下なら`null`を返す。
- 履歴と参照元を取得する。
- フィードバックを`bool?`で取得する。
- 参照元を表示順、ID順でDTO化する。
- URLは`~/Faqs/Detail.aspx?id={FaqId}`とする。

---

## 23. AIフィードバックService詳細設計

### 23.1 `AiSearchFeedbackService.SaveAsync`

| 順序 | 処理 |
|---:|---|
| 1 | 履歴IDが0以下なら`ArgumentOutOfRangeException` |
| 2 | コメントをTrimし、空なら`null`、1000文字超は切り詰める |
| 3 | 対象履歴の存在を`AnyAsync`で確認する |
| 4 | 存在しない場合は`InvalidOperationException` |
| 5 | 履歴IDに紐づく既存フィードバックを取得する |
| 6 | 未登録なら新規Entityを追加する |
| 7 | 登録済みなら評価・コメント・更新日時を上書きする |
| 8 | `SaveChangesAsync`を実行する |

作成・更新日時はUTCとする。1履歴1件を前提とし、再評価はINSERTではなくUPDATEする。

---

## 24. AI検索履歴一覧画面詳細設計

### 24.1 対象

| 項目 | 内容 |
|---|---|
| URL | `/Admin/AiHistories/Index.aspx` |
| 継承 | `AdminPageBase` |
| Service | `IAiSearchHistoryQueryService` |
| Service生成 | `Page_Init` |
| ページング | `GridView`のPageIndex。ページサイズはASPX設定に依存 |

### 24.2 イベント

| イベント | 処理 |
|---|---|
| `Page_Load` | 初回表示で履歴読込 |
| `SearchButton_Click` | PageIndexを0へ戻して検索 |
| `ResetButton_Click` | キーワード・成否条件を空にして検索 |
| `HistoryGrid_PageIndexChanging` | 指定PageIndexへ変更して検索 |

### 24.3 表示変換

| 項目 | 表示 |
|---|---|
| 実行日時 | UTCをTokyo Standard Timeへ変換し`yyyy/MM/dd HH:mm:ss` |
| 成功 | 「成功」、成功バッジ |
| 失敗 | 「失敗」、失敗バッジ |
| 評価あり | 👍または👎 |
| 未評価 | `-` |
| 結果 | 回答プレビューを優先し、なければエラー、両方なければ`-` |

MySQLの`TINYINT(1)`等が0／1で渡されるケースを考慮し、`bool?`変換を行う。

### 24.4 例外時

- Trace Warningへ例外を記録する。
- GridViewを空にする。
- 件数を0件にする。
- 「AI検索履歴を取得できませんでした。」をHTMLエンコードして表示する。

---

## 25. AI検索履歴詳細画面詳細設計

### 25.1 対象

| 項目 | 内容 |
|---|---|
| URL | `/Admin/AiHistories/Detail.aspx?id={id}` |
| 継承 | `AdminPageBase` |
| データアクセス | CodeBehindから`FaqKnowledgeDbContext`を直接使用 |

現行実装では`AiSearchHistoryQueryService.GetHistoryById`ではなく、画面内でDbContextを生成して取得する。

### 25.2 `LoadHistoryDetail`

1. クエリ文字列`id`を正の`long`へ変換する。
2. 履歴とSourcesを`AsNoTracking`で取得する。
3. 対象なしの場合はHTTP 404とする。
4. フィードバックを`bool?`で取得する。
5. Sourcesを表示順で画面用内部ViewModelへ変換する。
6. `BindHistory`で画面へ設定する。

### 25.3 表示制御

- 成功／失敗でステータス文言とCSSを変更する。
- 質問文と検索キーワードが完全一致する場合、検索キーワード欄を非表示にする。
- 回答がある場合は回答パネル、ない場合は空パネルを表示する。
- エラーメッセージがある場合のみエラー欄を表示する。
- 参照元0件の場合は空表示パネルを表示する。
- フィードバックは「未評価」「役に立った 👍」「役に立たなかった 👎」で表示する。

### 25.4 参照元プレビュー

- 回答なしは「回答は保存されていません。」
- 改行を空白へ変換する。
- 最大160文字、超過時は`...`を付ける。

### 25.5 日時

MySQLから取得した`DateTime.Kind`が`Unspecified`でもUTCとして扱い、日本時間へ変換する。変換失敗時は元日時を同じ形式で表示する。

---

## 26. ユーザー管理Service詳細設計

### 26.1 `AdminUserService.GetPage`

#### 入力補正

| 項目 | 補正 |
|---|---|
| pageNumber | 1未満は1 |
| pageSize | 1未満は1、100超は100 |

#### 取得処理

1. ユーザー総件数を取得する。
2. 総ページ数を計算する。
3. 指定ページが最終ページを超える場合は最終ページへ補正する。
4. 作成日時降順、表示名、メールの順でページ取得する。
5. 対象ユーザーIDのロールを一括取得する。
6. DTOへ変換する。

表示名が空の場合は「未設定」、メールが空の場合はUserNameを使用する。ロールなしは「未割当」とする。

### 26.2 代表ロール

- AdminロールがあればAdminを優先する。
- Adminがなければロール名昇順の先頭を採用する。
- ロールなしは「未割当」とする。

### 26.3 `SetActiveAsync`

| 条件 | 結果 |
|---|---|
| userId空 | 失敗結果「ユーザーIDが指定されていません。」 |
| ユーザーなし | 失敗結果「対象のユーザーが見つかりません。」 |
| Admin | 失敗結果「Adminユーザーの有効・無効は変更できません。」 |
| 現状態と同じ | Success、DB更新なし |
| 一般ユーザー | `IsActive`を更新する |

無効化時のみSecurityStampを新しいGUIDへ更新し、既存認証Cookieが将来的に無効判定されるようにする。

---

## 27. ユーザー管理画面詳細設計

### 27.1 対象

| 項目 | 内容 |
|---|---|
| URL | `/Admin/Users/Index.aspx` |
| 継承 | `AdminPageBase` |
| ページサイズ | 5件 |
| ページ指定 | クエリ文字列`page`、1始まり |
| 一覧 | Repeater |

### 27.2 初期表示

初回表示で`LoadUsers`を実行する。Serviceから取得したItemsをRepeaterへバインドし、総件数、空表示、ページャーを更新する。

### 27.3 状態変更

`ToggleStatusButton_Command`でCommandNameを判定する。

| CommandName | 処理 |
|---|---|
| `Enable` | 有効化 |
| その他／`Disable` | 無効化 |

`RegisterAsyncTask`と`PageAsyncTask`を使用して非同期に`SetActiveAsync`を呼ぶ。

### 27.4 メッセージ

| 結果 | 表示 |
|---|---|
| 有効化成功 | ユーザーを有効にしました。 |
| 無効化成功 | ユーザーを無効にしました。 |
| 失敗 | IdentityResult.Errorsを空白結合 |

表示前に`Server.HtmlEncode`を適用する。

### 27.5 ページャー

- 総ページ数が2以上の場合のみ表示する。
- 前後リンクは`~/Admin/Users/Index.aspx?page={n}`を設定する。
- 存在しないページはService側で最終ページへ補正される。

### 27.6 状態変更確認

| 現在状態 | ボタン | JavaScript確認 |
|---|---|---|
| 有効 | 無効にする | このユーザーを無効にしますか？ |
| 無効 | 有効にする | このユーザーを有効にしますか？ |

AdminユーザーはDTOの`CanChangeStatus=false`として、画面上の状態変更対象外とする。

### 27.7 作成日時

UTCとして扱い、`Tokyo Standard Time`へ変換し、`yyyy/MM/dd HH:mm`で表示する。

---

## 28. 入力チェック・メッセージ設計

### 28.1 主な入力チェック

| 画面・機能 | 項目 | 条件 |
|---|---|---|
| ログイン | メール・パスワード | ASPX ValidatorおよびIdentity認証 |
| FAQ検索 | キーワード | 任意、Trim |
| FAQ詳細 | id | 正のlong |
| FAQ登録・編集 | カテゴリ | 必須、正のlong、利用可能カテゴリ |
| FAQ登録・編集 | 質問 | 必須、500文字以内 |
| FAQ登録・編集 | 回答 | 必須、画面上4000文字以内 |
| FAQ登録・編集 | タグ | 正のID、重複排除、DB存在確認 |
| AI検索 | 質問 | Validator、Serviceで必須・500文字以内 |
| AI検索 | レート | IP単位で1分5回 |
| フィードバック | 履歴ID | 正のlong、DB存在確認 |
| フィードバック | コメント | Service上1000文字以内。現行画面は未入力 |
| ユーザー一覧 | page | 1以上へ補正 |
| ユーザー状態 | userId | 必須、存在、非Admin |

### 28.2 主な画面メッセージ

| 分類 | メッセージ例 |
|---|---|
| FAQ登録 | FAQを登録しました。 |
| FAQ更新 | FAQを更新しました。 |
| FAQ削除 | FAQを削除しました。 |
| AI FAQなし | 該当するFAQが見つかりませんでした。キーワードを変えて検索してください。 |
| AI API失敗 | AI回答の生成に失敗しました。参照元FAQをご確認ください。 |
| レート超過 | AI検索は1分間に5回まで利用できます。{n}秒ほど待ってから再度お試しください。 |
| 評価成功 | 「役に立った」として送信しました。／「役に立たなかった」として送信しました。 |
| 認証失敗 | メールアドレスまたはパスワードが正しくありません。 |

---

## 29. 例外・ログ・HTTPステータス設計

### 29.1 ログ方式

| 箇所 | ログ方式 |
|---|---|
| `AiService` | `Trace.TraceError` |
| `AiApiClient` | `Trace.TraceInformation`、`Trace.TraceWarning` |
| AI検索画面 | `Trace.Warn` |
| AI履歴画面 | `Trace.Warn` |
| 管理FAQ | `System.Diagnostics.Trace.TraceError` |
| 公開FAQ | `Debug.WriteLine` |

画面には原則として固定メッセージを表示し、例外全文を表示しない方針とする。ただし公開FAQ一覧の現行実装は基底例外メッセージを画面表示しているため、改善確認対象とする。

### 29.2 HTTPステータス

| ケース | ステータス |
|---|---:|
| 公開FAQ ID不正 | 400 |
| 公開FAQなし | 404 |
| 公開FAQ取得例外 | 500 |
| 管理AI履歴なし | 404 |
| 認証済み非Adminの管理画面アクセス | 403例外 |

### 29.3 `Web.config`エラー設定

| 項目 | 内容 |
|---|---|
| `customErrors.mode` | `RemoteOnly` |
| 既定 | `~/Error/500.aspx` |
| 404 | `~/Error/404.aspx` |
| redirectMode | `ResponseRewrite` |

### 29.4 例外境界

- 入力・業務ルール違反は`ArgumentException`、`InvalidOperationException`等で表現する。
- CodeBehindで捕捉可能な業務例外は利用者向けメッセージとして表示する。
- AI APIの詳細レスポンス、APIキー、認証情報はDBや画面へ出さない。
- 履歴保存失敗はAI検索結果の返却を妨げない。

---

## 30. 日時・タイムゾーン設計

### 30.1 UTC保存

次の日時はUTC保存を前提とする。

- `ApplicationUser.CreatedAt`
- `AiSearchHistory.ExecutedAt`
- `AiSearchFeedback.CreatedAt`
- `AiSearchFeedback.UpdatedAt`

### 30.2 ローカル時刻保存

FAQの作成・更新は現行Serviceで`DateTime.Now`を使用する。MySQL・アプリケーションサーバーのタイムゾーンに依存する。

### 30.3 表示変換

管理ユーザー一覧とAI履歴画面は、UTCを`Tokyo Standard Time`へ変換して表示する。MySQLからの`DateTime.Kind=Unspecified`はUTCとして補正する。

### 30.4 設計上の注意

UTCとローカル時刻が混在しているため、将来は全テーブルの保存時刻をUTCへ統一することが望ましい。

---

## 31. 設定管理詳細設計

### 31.1 `Web.config`

| 項目 | 内容 |
|---|---|
| Target Framework | 4.8 |
| ASP.NET認証モード | `None`。認証はOWIN側で処理 |
| ConnectionStrings | `connectionStrings.config`へ外部化 |
| AppSettings | `appSettings.local.config`へ外部化 |
| EF Provider | SQL Server ProviderとMySQL Providerを登録 |
| MySQL Provider Version | 9.7.0.0 |
| OWIN | 4.2.3系Binding Redirect |
| Newtonsoft.Json | 13.0.0.0へBinding Redirect |

### 31.2 `AiSettings.Load`

| プロパティ | 読み込み元 | 既定・補正 |
|---|---|---|
| `ApiKey` | 環境変数`FAQ_AI_API_KEY`を優先し、未設定なら`AiApiKey` | 未設定時はClientで例外 |
| `Endpoint` | AppSettings `AiApiEndpoint` | 未設定時はClientで例外 |
| `Model` | AppSettings `AiModel` | 未設定時はClientで例外 |
| `MaxContextFaqCount` | AppSettings `AiMaxContextFaqCount` | 未設定・0以下は5 |

### 31.3 機密情報

- APIキーは環境変数を優先する。
- 接続文字列とローカルAppSettingsは外部ファイルへ分離する。
- 実値をREADME、設計書、ソース管理へ掲載しない。
- ログへAuthorizationヘッダー、リクエストJSON全文、APIレスポンス全文を出さない。

---

## 32. データベース物理概要

物理スキーマは`database/*.sql`およびER図を正とする。本節は主要項目を示す。

### 32.1 業務テーブル

| テーブル | 主な項目 | 主な制約・用途 |
|---|---|---|
| `categories` | id, name, display_order, is_active, created_at, updated_at | FAQカテゴリ |
| `faqs` | id, category_id, question, answer, is_published, is_deleted, view_count, created_at, updated_at | FAQ本体、論理削除 |
| `tags` | id, name, display_order | タグ |
| `faq_tags` | faq_id, tag_id | FAQ・タグ多対多の複合キー |
| `ai_search_histories` | id, question, search_keywords, ai_answer, is_success, error_message, executed_at | AI検索履歴 |
| `ai_search_history_sources` | id, ai_search_history_id, faq_id, faq_question, faq_answer, category_name, display_order | 参照元スナップショット |
| `ai_search_feedbacks` | id, ai_search_history_id, is_helpful, comment, created_at, updated_at | 履歴評価。1履歴1件 |

### 32.2 主な長さ

| 項目 | 最大長 |
|---|---:|
| FAQ質問 | 500 |
| FAQ回答 | 画面入力4000。DBは長文型 |
| 履歴質問 | 500 |
| 履歴エラー | 1000 |
| 参照元質問 | 500 |
| 参照元カテゴリ | 100 |
| フィードバックコメント | 1000 |
| ユーザー表示名 | 100 |

### 32.3 データ保持

- FAQは`is_deleted`による論理削除とする。
- 論理削除時に`is_published=false`へ変更する。
- AI検索履歴は成功・失敗とも保持する。
- 参照元は回答生成時点のスナップショットを保持する。
- ユーザーは物理削除せず、`IsActive`で管理する。

---

## 33. パフォーマンス・同時実行設計

### 33.1 読み取り

- 一覧参照には原則`AsNoTracking`を使用する。
- ユーザーのロールと履歴フィードバックは対象ID分をまとめて取得する。
- AIコンテキスト件数を設定値で制限する。

### 33.2 ページング

| 画面 | 方式 |
|---|---|
| 公開FAQ一覧 | Serviceで全件取得後、CodeBehindで`Skip/Take` |
| 管理FAQ一覧 | Serviceで全件取得後、GridViewへバインド |
| ユーザー一覧 | DBクエリで`Skip/Take` |
| AI履歴一覧 | Serviceで全件取得後、GridViewページング |

FAQ・履歴件数が増える場合は、Serviceにページ番号・件数を渡し、DB側でページングすることが望ましい。

### 33.3 同時実行

- FAQ閲覧数は単純な読取後加算であり、高並行時の厳密な加算保証はない。
- EntityにConcurrency Tokenは確認されていない。
- フィードバックは最後の更新を採用する。
- AI検索レート制限はstatic lockにより同一プロセス内の更新競合を防止する。

---

## 34. セキュリティ詳細設計

### 34.1 認証・認可

- OWIN CookieはHttpOnlyとする。
- HTTPS時はSecure属性を付与する。
- 管理画面は`AdminPageBase`で認証・ロール確認する。
- LoginのReturnUrlをローカルURLに制限する。
- パスワード不一致時に失敗回数を加算する。
- ロックアウト状態を確認する。

### 34.2 出力エンコード

明示的に確認できた箇所：

- ログインエラー
- AI回答・注意文・AI画面メッセージ
- 管理FAQ完了・エラーメッセージ
- ユーザー管理メッセージ
- AI履歴一覧エラーメッセージ

FAQ本文、質問、AI履歴詳細等をLiteralへ設定する箇所は、ASPX側の`Mode`や出力方式を含めてエンコード方針の確認が必要である。

### 34.3 外部入力

- EF6のLINQクエリを使用し、文字列連結SQLを使用しない。
- APIキーを環境変数・外部設定で管理する。
- OpenAI APIへ渡すFAQは公開中のFAQに限定する。
- AI回答にはFAQ外の推測を抑制するプロンプトを設定する。

### 34.4 レート制限

AI APIの乱用とコスト増加を抑えるため、IP単位で1分5回に制限する。公開環境を複数台構成にする場合は、Redis等の共有カウンターへ置換する。

---

## 35. テスト観点

### 35.1 FAQ

| ID | 観点 |
|---|---|
| T-FAQ-001 | キーワードなしで公開・未削除・有効カテゴリFAQのみ表示される |
| T-FAQ-002 | 質問、回答、カテゴリ、タグの部分一致で検索できる |
| T-FAQ-003 | 非公開・削除済み・無効カテゴリFAQが一般画面に出ない |
| T-FAQ-004 | 詳細表示で閲覧数が1増える |
| T-FAQ-005 | 不正IDは400、対象なしは404となる |
| T-FAQ-006 | 質問500文字、回答4000文字の境界値 |
| T-FAQ-007 | 無効カテゴリを既存編集で維持でき、新規登録では選べない |
| T-FAQ-008 | 削除時に論理削除かつ非公開になる |
| T-FAQ-009 | タグ重複・存在しないタグIDを適切に処理する |

### 35.2 AI検索

| ID | 観点 |
|---|---|
| T-AI-001 | 空質問・500文字超でAPIを呼ばない |
| T-AI-002 | FAQ0件でAPIを呼ばず失敗履歴を保存する |
| T-AI-003 | 設定件数以内のFAQだけをコンテキストに使う |
| T-AI-004 | API成功で回答・参照元・履歴IDを返す |
| T-AI-005 | API失敗でも参照元を表示し失敗履歴を保存する |
| T-AI-006 | 履歴保存失敗でも生成回答を表示する |
| T-AI-007 | 1分6回目を拒否し、待機秒数を表示する |
| T-AI-008 | AI回答がHTMLエンコードされる |
| T-AI-009 | 履歴IDがない場合はフィードバックを表示しない |

### 35.3 履歴・フィードバック

| ID | 観点 |
|---|---|
| T-HIS-001 | 成功・失敗の保存項目が正しい |
| T-HIS-002 | 参照元重複が除外される |
| T-HIS-003 | 参照元がスナップショットとして保持される |
| T-HIS-004 | キーワード・成否で履歴検索できる |
| T-HIS-005 | 評価の新規登録と更新ができる |
| T-HIS-006 | 存在しない履歴への評価が失敗する |
| T-HIS-007 | UTC日時が日本時間へ変換される |

### 35.4 認証・ユーザー

| ID | 観点 |
|---|---|
| T-AUTH-001 | 未認証の管理画面アクセスがLoginへリダイレクトされる |
| T-AUTH-002 | ReturnUrlが認証後に復元される |
| T-AUTH-003 | 外部ReturnUrlが拒否される |
| T-AUTH-004 | 非Adminはログインできない |
| T-AUTH-005 | パスワード失敗回数とロックアウトが機能する |
| T-AUTH-006 | LogoutでCookie・Session・キャッシュが破棄される |
| T-USR-001 | ユーザー一覧が作成日時降順でページングされる |
| T-USR-002 | Adminの状態変更が拒否される |
| T-USR-003 | 一般ユーザー無効化でSecurityStampが更新される |
| T-USR-004 | 無効ユーザーの新規ログイン可否が要件どおりである |

---

## 36. 実装整合確認・改善候補

本節は現行実装を否定するものではなく、要件・基本設計との整合性や将来運用を考慮した確認事項である。

| ID | 項目 | 現行状態 | 確認・改善案 |
|---|---|---|---|
| C-001 | 無効ユーザーのログイン | Login処理に`IsActive`の明示判定が確認できない | パスワード検証後に`user.IsActive`を確認し、無効時は共通認証エラーとする |
| C-002 | 既存セッション無効化 | SecurityStamp検証は30分間隔 | 即時性が必要なら検証間隔短縮または追加失効方式を検討する |
| C-003 | 公開FAQ一覧の例外表示 | 基底例外メッセージを画面に表示する | 利用者には固定文言のみ表示し、詳細はログへ出す |
| C-004 | Literal出力 | 一部でCodeBehind上の明示エンコードがない | ASPXの`Mode="Encode"`または明示エンコードを確認・統一する |
| C-005 | AI履歴詳細のデータアクセス | CodeBehindからDbContextを直接使用 | `IAiSearchHistoryQueryService.GetHistoryById`へ統一すると責務が明確になる |
| C-006 | FAQ・履歴ページング | 全件取得後に画面でページング | 件数増加時はDB側ページングへ変更する |
| C-007 | FAQ検索関連度 | 質問全文の単純Contains、更新日時順 | 単語分割、全文検索、関連度スコアを将来検討する |
| C-008 | 日時 | FAQは`DateTime.Now`、AI・IdentityはUTC | 保存日時をUTCへ統一する |
| C-009 | 閲覧数同時更新 | 読取後加算 | DB側原子的UPDATEやConcurrency制御を検討する |
| C-010 | レート制限 | プロセス内Cache | 複数台運用時はRedis等へ移行する |
| C-011 | 回答長制限 | CodeBehindは4000、Serviceに上限なし | 業務ルールとしてServiceにも同じ上限を持たせるか方針を統一する |
| C-012 | AI履歴検索のnull列 | `Contains`対象にnullable列を含む | Provider挙動をテストし、必要ならnull判定を明示する |
| C-013 | 管理ヘッダー | AI履歴用ActiveItemが未確認 | 管理メニュー全体の選択状態を統一する |

---

## 37. ディレクトリ・実装参照

```text
FaqKnowledgeSearch.WebForms
├─ Account
│  ├─ Login.aspx / Login.aspx.cs
│  └─ Logout.aspx / Logout.aspx.cs
├─ Admin
│  ├─ AdminPageBase.cs
│  ├─ AdminHeader.ascx / AdminHeader.ascx.cs
│  ├─ Default.aspx / Default.aspx.cs
│  ├─ Faqs
│  │  ├─ Index.aspx / Index.aspx.cs
│  │  └─ Edit.aspx / Edit.aspx.cs
│  ├─ AiHistories
│  │  ├─ Index.aspx / Index.aspx.cs
│  │  └─ Detail.aspx / Detail.aspx.cs
│  └─ Users
│     └─ Index.aspx / Index.aspx.cs
├─ AiSearch
│  └─ Index.aspx / Index.aspx.cs
├─ Faqs
│  ├─ Index.aspx / Index.aspx.cs
│  └─ Detail.aspx / Detail.aspx.cs
├─ Data
│  └─ FaqKnowledgeDbContext.cs
├─ Identity
│  ├─ ApplicationUser.cs
│  └─ ApplicationIdentityDbContext.cs
├─ Dtos
├─ Models
├─ Services
│  └─ Ai
├─ Settings
│  └─ AiSettings.cs
├─ Startup.cs
└─ Web.config
```

---

## 38. 関連ドキュメント

- `requirements-webforms.md`
- `basic-design-webforms.md`
- ルートREADME
- ER図
- 一般利用者向け画面遷移図
- 管理者向け画面遷移図
- `database/*.sql`
- Service層ユニットテスト
- GitHub Actions Workflow

---

## 39. 補足

本書は2026年7月16日時点で提示された現行実装を基に作成している。今後、ASPXマークアップ、`ApplicationUserManager`設定、Entity属性、SQLスキーマ、テストコードまたは画面仕様を変更した場合は、要件定義書・基本設計書・本詳細設計書を同時に更新する。
