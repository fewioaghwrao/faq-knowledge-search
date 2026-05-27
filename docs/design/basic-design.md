# 基本設計書

**社内FAQ・業務ナレッジ検索アプリ**
**FAQ Knowledge Search**

| 項目 | 内容 |
|---|---|
| プロジェクト名 | 社内FAQ・業務ナレッジ検索アプリ |
| ドキュメント種別 | 基本設計書 |
| バージョン | 1.1 |
| 作成日 | 2026/05/09 |
| 更新日 | 2026/05/28 |
| 作成者 | — |
| 承認者 | — |

---

## 改訂履歴

| バージョン | 日付 | 変更内容 | 作成者 |
|---|---|---|---|
| 1.0 | 2026/05/09 | 初版作成 | — |
| 1.1 | 2026/05/28 | 現行実装に合わせて修正。MySQL、OpenAI API、Heroku、Azure Static Web Apps、AiSearchHistory構成へ整理 | — |

---

## 1. システム概要

### 1.1 目的

本システムは、社内業務アプリで発生しやすい問い合わせ、操作手順、障害対応、APIエラー、CSV取込、PDF出力、メール通知、月次処理などのFAQを一元管理し、通常検索およびAI検索から必要な情報を確認できるWebアプリである。

- 一般利用者は公開済みFAQを検索・閲覧できる。
- 管理者はFAQの登録・編集・削除、AI検索履歴の確認、ユーザー管理を行う。

AI検索では、質問内容をもとに関連FAQを検索し、上位FAQをコンテキストとしてOpenAI APIへ渡して回答を生成する。回答には参照元FAQを表示し、FAQに存在しない内容を断言しないように制御する。

---

### 1.2 想定利用者

| 利用者 | 主な利用内容 |
|---|---|
| 一般利用者 | 公開FAQの検索・閲覧、AI FAQ検索、AI回答フィードバック |
| 管理者 | FAQ管理、AI検索履歴確認、ユーザー管理、公開 / 非公開管理 |

---

### 1.3 主な利用シーン

| シーン | 内容 |
|---|---|
| 業務アプリの操作確認 | 操作手順や画面の使い方をFAQから検索する |
| 障害対応 | エラー内容や障害対応メモを検索する |
| 月次処理確認 | 月次締め、CSV取込、PDF出力などの手順を確認する |
| AIによる要約確認 | FAQを根拠にしたAI回答を確認する |
| ナレッジ管理 | 管理者がFAQを登録・編集・整理する |
| AI利用状況確認 | 管理者がAI検索履歴やフィードバックを確認する |

---

## 2. システム構成

### 2.1 全体アーキテクチャ

本システムは、フロントエンド、バックエンドAPI、データベースを分離した3層構成とする。

```
[User Browser]
      |
      v
[Next.js Frontend]
Azure Static Web Apps
      |
      | REST API / JSON
      v
[ASP.NET Core Web API]
Heroku
      |
      | Entity Framework Core
      v
[MySQL Database]
      |
      | FAQ Context
      v
[OpenAI API]
```

---

### 2.2 現在のデモ環境

| 区分 | 内容 |
|---|---|
| Frontend | Next.js / React / TypeScript |
| Frontend Hosting | Azure Static Web Apps |
| Backend | ASP.NET Core Web API / C# |
| Backend Hosting | Heroku |
| Database | MySQL |
| ORM | Entity Framework Core |
| Authentication | JWT認証 / ASP.NET Core Identity |
| AI | OpenAI API |
| CI/CD | GitHub Actions |

---

### 2.3 外部サービス連携

| 外部サービス | 用途 |
|---|---|
| OpenAI API | FAQコンテキストに基づくAI回答生成 |
| Azure Static Web Apps | フロントエンドホスティング |
| Heroku | バックエンドAPIホスティング |
| MySQL | FAQ、AI検索履歴、ユーザー情報の保存 |
| GitHub Actions | テスト・デプロイ自動化 |

---

## 3. 実装範囲

### 3.1 実装済み機能

| 機能 | 状態 | 補足 |
|---|---|---|
| FAQ一覧・検索 | ✅ 実装済み | キーワード検索、FAQカード表示 |
| FAQ詳細 | ✅ 実装済み | FAQ本文、カテゴリ、タグ、閲覧数を表示 |
| FAQ新規登録 | ✅ 実装済み | 管理画面から登録 |
| FAQ編集 | ✅ 実装済み | 管理画面から編集 |
| FAQ削除 | ✅ 実装済み | 管理画面から削除 |
| 公開 / 非公開管理 | ✅ 実装済み | FAQごとに公開状態を切り替え |
| 管理者ログイン | ✅ 実装済み | JWT認証 |
| ログアウト確認 | ✅ 実装済み | 確認ダイアログあり |
| AI FAQ検索 | ✅ 実装済み | FAQをコンテキストにAI回答を生成 |
| 外部AI API連携 | ✅ 実装済み | OpenAI APIを呼び出し |
| 参照元FAQ表示 | ✅ 実装済み | AI回答の根拠FAQを表示 |
| AI検索履歴一覧 | ✅ 実装済み | 管理画面で履歴を確認 |
| AI検索履歴詳細 | ✅ 実装済み | 質問、回答、参照元FAQを確認 |
| AI回答フィードバック | ✅ 実装済み | 役に立った / 役に立たなかったを記録 |
| ユーザー管理 | ✅ 実装済み | ユーザー一覧、状態管理 |
| バックエンドテスト | ✅ 実装済み | Controller / Service のテスト |
| フロントエンドテスト | ✅ 実装済み | Page / Component / lib のテスト |
| フロントエンドデプロイ | ✅ 実装済み | Azure Static Web Apps |
| バックエンドデプロイ | ✅ 実装済み | Heroku |

---

### 3.2 今後の拡張候補

| 機能 | 状態 | 補足 |
|---|---|---|
| CSVインポート | 🔲 今後の拡張 | FAQ一括登録 |
| ファイルアップロード | 🔲 今後の拡張 | 手順書・添付資料管理 |
| Slack / Teams通知 | 🔲 今後の拡張 | FAQ更新通知 |
| 管理ダッシュボード | 🔲 今後の拡張 | 利用状況の可視化 |
| カテゴリ別アクセス統計 | 🔲 今後の拡張 | FAQ改善分析 |
| 検索サジェスト | 🔲 今後の拡張 | 検索UX改善 |
| よく見られているFAQ表示 | 🔲 今後の拡張 | トップページ強化 |
| FAQ本文のMarkdown表示強化 | 🔲 今後の拡張 | 表現力向上 |
| 通常検索履歴 | 🔲 今後の拡張 | AI検索履歴とは別に管理 |
| Editorロール | 🔲 今後の拡張 | FAQ編集者ロールを追加 |
| Azure App Service移行 | 🔲 今後の拡張 | バックエンドホスティング拡張 |
| Azure SQL Database移行 | 🔲 今後の拡張 | DB基盤拡張 |
| Azure OpenAI Service移行 | 🔲 今後の拡張 | 企業向けAI基盤拡張 |

---

## 4. アプリケーション構成

### 4.1 ディレクトリ構成

```
faq-knowledge-search
├── backend
│   ├── FaqApp.Api
│   │   ├── Controller
│   │   ├── Data
│   │   ├── Dtos
│   │   ├── Entities
│   │   ├── Migrations
│   │   ├── Services
│   │   ├── Settings
│   │   └── Program.cs
│   │
│   └── FaqApp.Api.Tests
│       ├── Controllers
│       └── Services
│
├── faq-app-frontend
│   ├── src
│   │   ├── app
│   │   ├── components
│   │   ├── lib
│   │   └── types
│   ├── package.json
│   └── next.config.ts
│
├── docs
│   ├── design
│   ├── requirements
│   ├── diagrams
│   └── images
│
├── .github
│   └── workflows
│
├── Dockerfile
├── docker-compose.yml
└── README.md
```

---

### 4.2 バックエンド構成

バックエンドは ASP.NET Core Web API で構築する。

```
Controller
  ↓
Service
  ↓
DbContext / Entity
  ↓
MySQL
```

| 層 | 主な責務 |
|---|---|
| Controller | APIエンドポイント、HTTPリクエスト受信、認証・認可、レスポンス返却 |
| Service | FAQ検索、AI回答生成、AI履歴保存、ユーザー管理などの業務ロジック |
| DTO | APIの入出力モデル |
| Entity | DB永続化モデル |
| DbContext | Entity Framework Core によるDB操作 |
| Migration | DBスキーマ管理 |
| Tests | Controller / Service の単体テスト |

---

### 4.3 フロントエンド構成

フロントエンドは Next.js App Router で構築する。

| ディレクトリ | 役割 |
|---|---|
| `src/app` | ページ・ルーティング |
| `src/components` | 共通コンポーネント |
| `src/lib` | API通信などの共通処理 |
| `src/types` | APIレスポンスや画面表示用の型定義 |

---

## 5. 画面設計

### 5.1 画面一覧

| URL | 画面 | 説明 |
|---|---|---|
| `/` | ホーム画面 | アプリ概要と主要導線を表示 |
| `/faqs` | FAQ検索 | 通常FAQ検索 |
| `/faqs/[id]` | FAQ詳細 | FAQ本文、カテゴリ、タグを表示 |
| `/ai-search` | AI FAQ検索 | AI回答と参照元FAQを表示 |
| `/login` | 管理者ログイン | ログインフォーム |
| `/admin` | 管理画面 | 管理者向けトップ |
| `/admin/faqs/new` | FAQ新規登録 | FAQ登録フォーム |
| `/admin/faqs/[id]/edit` | FAQ編集 | FAQ編集フォーム |
| `/admin/ai-histories` | AI検索履歴一覧 | AI検索履歴一覧を表示 |
| `/admin/ai-histories/[id]` | AI検索履歴詳細 | AI回答、質問、参照元FAQを表示 |
| `/admin/users` | ユーザー管理 | ユーザー一覧、有効 / 無効管理 |

---

### 5.2 一般利用者向け画面

#### ホーム画面

| 項目 | 内容 |
|---|---|
| 目的 | アプリ概要とFAQ検索・AI検索への導線を表示する |
| 主な表示 | アプリ概要、FAQ検索リンク、AI検索リンク、管理画面リンク |
| 認証 | 不要 |

#### FAQ検索画面

| 項目 | 内容 |
|---|---|
| 目的 | FAQをキーワードで検索する |
| 主な表示 | 検索フォーム、FAQ一覧カード、カテゴリ、タグ、閲覧数 |
| 検索対象 | FAQタイトル、本文、カテゴリ、タグ |
| 認証 | 不要 |
| 備考 | 一般利用者は公開中FAQのみ閲覧可能 |

#### FAQ詳細画面

| 項目 | 内容 |
|---|---|
| 目的 | FAQ本文を確認する |
| 主な表示 | タイトル、本文、カテゴリ、タグ、閲覧数、更新日時 |
| 認証 | 不要 |
| 備考 | 詳細表示時に閲覧数を加算する |

#### AI FAQ検索画面

| 項目 | 内容 |
|---|---|
| 目的 | 自然文の質問に対して、FAQを根拠にAI回答を生成する |
| 主な表示 | 質問入力欄、AI回答、注意文、参照元FAQ、フィードバックボタン |
| 認証 | 不要または認証済みユーザー向け。実装方針に合わせて制御 |
| 備考 | FAQにない情報は断言しない |

---

### 5.3 管理者向け画面

#### ログイン画面

| 項目 | 内容 |
|---|---|
| 目的 | 管理者がログインする |
| 入力項目 | メールアドレス、パスワード |
| 成功時 | JWTを取得し、管理画面へ遷移 |
| 失敗時 | 認証エラーメッセージを表示 |

#### 管理画面

| 項目 | 内容 |
|---|---|
| 目的 | 管理者向け機能への導線を表示する |
| 主な導線 | FAQ管理、AI検索履歴、ユーザー管理 |
| 認証 | 必須 |

#### FAQ新規登録・編集画面

| 項目 | 内容 |
|---|---|
| 目的 | FAQを登録・編集する |
| 入力項目 | タイトル、本文、カテゴリ、タグ、公開状態 |
| 認証 | 管理者のみ |
| バリデーション | タイトル必須、本文必須、カテゴリ必須 |

#### AI検索履歴一覧・詳細画面

| 項目 | 内容 |
|---|---|
| 目的 | AI検索の利用履歴を確認する |
| 一覧表示 | 質問、回答プレビュー、成功 / 失敗、フィードバック、実行日時 |
| 詳細表示 | 質問全文、AI回答、参照元FAQ、エラー内容 |
| 認証 | 管理者のみ |

#### ユーザー管理画面

| 項目 | 内容 |
|---|---|
| 目的 | ユーザーの状態を管理する |
| 主な表示 | 表示名、メールアドレス、ロール、有効状態、作成日時 |
| 主な操作 | 有効 / 無効切り替え、ロール確認・変更 |
| 認証 | 管理者のみ |
| 制約 | 自分自身の無効化は不可 |

---

## 6. API設計

### 6.1 API共通仕様

| 項目 | 内容 |
|---|---|
| 通信形式 | REST API |
| データ形式 | JSON |
| 認証方式 | JWT |
| 日時形式 | ISO 8601 |
| エラー形式 | JSON |
| ページネーション | `page` / `pageSize` / `total` / `items` |

---

### 6.2 認証API

| メソッド | パス | 内容 | 認証 |
|---|---|---|---|
| POST | `/api/auth/login` | ログイン | 不要 |
| POST | `/api/auth/logout` | ログアウト | 任意 |

---

### 6.3 FAQ API

| メソッド | パス | 内容 | 認証 |
|---|---|---|---|
| GET | `/api/faqs` | FAQ一覧取得 | 不要 |
| GET | `/api/faqs/{id}` | FAQ詳細取得 | 不要 |
| POST | `/api/faqs` | FAQ新規登録 | 管理者 |
| PUT | `/api/faqs/{id}` | FAQ更新 | 管理者 |
| DELETE | `/api/faqs/{id}` | FAQ削除 | 管理者 |

---

### 6.4 AI API

| メソッド | パス | 内容 | 認証 |
|---|---|---|---|
| POST | `/api/ai/search` | AI FAQ検索 | 任意または認証済み |
| POST | `/api/ai/histories/{id}/feedback` | AI回答フィードバック | 任意または認証済み |
| GET | `/api/ai/histories` | AI検索履歴一覧 | 管理者 |
| GET | `/api/ai/histories/{id}` | AI検索履歴詳細 | 管理者 |

---

### 6.5 ユーザー管理API

| メソッド | パス | 内容 | 認証 |
|---|---|---|---|
| GET | `/api/users` | ユーザー一覧取得 | 管理者 |
| PUT | `/api/users/{id}/status` | ユーザー有効 / 無効切り替え | 管理者 |
| PUT | `/api/users/{id}/role` | ユーザーロール変更 | 管理者 |

---

## 7. データベース設計

### 7.1 ER図

> ER図を参照（`docs/diagrams/` 内のER図ドキュメントを開く）

---

### 7.2 主なテーブル

| テーブル | 概要 |
|---|---|
| `Faqs` | FAQ本文、公開状態、カテゴリ、閲覧数など |
| `Categories` | FAQカテゴリ |
| `Tags` | FAQタグ |
| `FaqTag` | FAQとタグの中間テーブル |
| `AiSearchHistories` | AI検索履歴 |
| `AiSearchHistorySources` | AI回答に利用した参照元FAQ |
| `AspNetUsers` | ASP.NET Core Identity のユーザー |
| `AspNetRoles` | ASP.NET Core Identity のロール |
| `AspNetUserRoles` | ユーザーとロールの関連 |

---

### 7.3 テーブル関連

| 関係 | 内容 |
|---|---|
| Category 1 : N Faq | 1カテゴリに複数FAQ |
| Faq N : N Tag | `FaqTag` を介して多対多 |
| AiSearchHistory 1 : N AiSearchHistorySource | 1回のAI検索に複数参照元FAQ |
| Faq 1 : N AiSearchHistorySource | 1FAQが複数AI検索履歴で参照される |
| AspNetUsers N : N AspNetRoles | Identity標準 |

---

### 7.4 削除方針

| 対象 | 方針 |
|---|---|
| Faq | 論理削除 |
| Category | FAQが紐づく場合は削除不可 |
| Tag | FAQが紐づく場合は削除不可、または関連解除後に削除 |
| AiSearchHistory | 原則保持 |
| AiSearchHistorySource | AiSearchHistoryに紐づけて保持 |
| ApplicationUser | 原則削除せず、`IsActive` で無効化 |

---

## 8. 認証・認可設計

### 8.1 認証方式

| 項目 | 内容 |
|---|---|
| 認証方式 | JWT認証 |
| ユーザー管理 | ASP.NET Core Identity |
| ユーザー拡張 | `DisplayName`、`IsActive`、`CreatedAt`、`UpdatedAt` |
| 無効ユーザー | ログイン不可 |
| 管理画面 | 認証済みユーザーのみ利用可能 |

---

### 8.2 ロール

| ロール | 内容 |
|---|---|
| Admin | 管理者 |
| User | 一般ユーザー |

**今後の拡張候補**

| ロール | 内容 |
|---|---|
| Editor | FAQ編集者 |

---

### 8.3 認可方針

| 機能 | 一般利用者 | Admin |
|---|---|---|
| ホーム表示 | ○ | ○ |
| 公開FAQ検索 | ○ | ○ |
| FAQ詳細閲覧 | ○ | ○ |
| AI FAQ検索 | ○ | ○ |
| AI回答フィードバック | ○ | ○ |
| FAQ新規登録 | × | ○ |
| FAQ編集 | × | ○ |
| FAQ削除 | × | ○ |
| AI検索履歴一覧 | × | ○ |
| AI検索履歴詳細 | × | ○ |
| ユーザー管理 | × | ○ |

---

## 9. AI連携設計

### 9.1 AI検索の基本方針

AI検索では、ユーザーの質問をそのままAIに送信するのではなく、バックエンド側で関連FAQを検索し、上位FAQをコンテキストとしてOpenAI APIへ送信する。

```
ユーザー質問
  ↓
キーワード抽出
  ↓
FAQ検索
  ↓
上位FAQ取得
  ↓
AIコンテキスト生成
  ↓
OpenAI API呼び出し
  ↓
AI回答生成
  ↓
AI検索履歴保存
  ↓
回答・参照元FAQ返却
```

---

### 9.2 AIに渡す情報

| 項目 | 内容 |
|---|---|
| ユーザー質問 | AI検索画面で入力された質問 |
| FAQコンテキスト | 関連FAQ上位5件 |
| システムプロンプト | FAQに基づく回答制約 |
| モデル | `AiSettings.Model` で指定 |

---

### 9.3 AI回答のガードレール

AI回答では、以下を制御する。

- 提供されたFAQコンテキストのみを根拠に回答する。
- FAQに記載されていない情報は断言しない。
- FAQにない手順、原因、担当部署、問い合わせ先を推測しない。
- 機密情報、個人情報、認証情報を出力しない。
- 回答末尾に「詳細は参照元FAQをご確認ください。」を付与する。
- 画面上に参照元FAQを表示する。

---

### 9.4 AI検索履歴

AI検索実行時は、以下を保存する。

| 項目 | 内容 |
|---|---|
| Question | ユーザーの質問 |
| SearchKeywords | 検索に利用したキーワード |
| AiAnswer | AI回答 |
| IsSuccess | 成功 / 失敗 |
| ErrorMessage | エラー内容 |
| IsHelpful | フィードバック |
| ExecutedAt | 実行日時 |
| Sources | 参照元FAQ |

---

## 10. バリデーション設計

### 10.1 FAQ登録・編集

| 項目 | ルール |
|---|---|
| `title` | 必須、100文字以内 |
| `body` | 必須 |
| `categoryId` | 必須、存在するカテゴリ |
| `tagIds` | 存在するタグ |
| `isPublished` | bool |

### 10.2 AI検索

| 項目 | ルール |
|---|---|
| `question` | 必須、500文字以内 |

### 10.3 ログイン

| 項目 | ルール |
|---|---|
| `email` | 必須、メール形式 |
| `password` | 必須 |

### 10.4 ユーザー管理

| 項目 | ルール |
|---|---|
| `isActive` | bool |
| `role` | 許可されたロールのみ |
| 自分自身の無効化 | 不可 |
| 自分自身のロール変更 | 不可 |

---

## 11. エラーハンドリング設計

### 11.1 API共通エラー

| HTTP Status | 内容 |
|---|---|
| 400 | 入力値不正 |
| 401 | 未認証 |
| 403 | 権限なし |
| 404 | 対象データなし |
| 500 | サーバーエラー |
| 504 | AI APIタイムアウト |

### 11.2 フロントエンド表示

| エラー | 表示内容 |
|---|---|
| 400 | 入力内容を確認してください |
| 401 | ログインしてください |
| 403 | この操作を行う権限がありません |
| 404 | 対象データが見つかりません |
| 500 | サーバーエラーが発生しました |
| 504 | AI回答の生成がタイムアウトしました |
| 通信エラー | 通信エラーが発生しました |

---

## 12. 非機能設計

### 12.1 セキュリティ

| 項目 | 内容 |
|---|---|
| HTTPS | 本番環境ではHTTPSを利用 |
| CORS | 許可オリジンをフロントエンドURLに制限 |
| APIキー管理 | User Secrets またはホスティング環境の環境変数で管理 |
| JWT署名キー | 環境変数で管理 |
| SQLインジェクション対策 | EF Coreを利用 |
| XSS対策 | ユーザー入力値の表示時に適切にエスケープ |
| AI安全対策 | FAQにない情報を断言しないプロンプトを設定 |
| 無効ユーザー対策 | `IsActive=false` のユーザーはログイン不可 |

### 12.2 パフォーマンス

| 項目 | 内容 |
|---|---|
| FAQ検索 | ページネーションを利用 |
| FAQ一覧 | 必要な件数のみ取得 |
| AI検索 | AIに渡すFAQ件数を上位5件に制限 |
| AIタイムアウト | 一定時間でタイムアウト |
| DBアクセス | EF Coreで必要な関連データを取得 |

### 12.3 ログ

| 項目 | 内容 |
|---|---|
| APIエラー | エラー内容をログ出力 |
| AI検索 | 成功 / 失敗、エラー内容をAI検索履歴として保存 |
| 認証 | ログイン失敗や無効ユーザーを適切に扱う |
| 機密情報 | APIキー、JWT、パスワードはログに出力しない |

---

## 13. テスト設計

### 13.1 Backendテスト

| 対象 | 内容 |
|---|---|
| Controller | APIレスポンス、認可、エラー確認 |
| Service | FAQ検索、AI検索、履歴保存、ユーザー管理 |
| Auth | ログイン、有効 / 無効判定 |
| AiService | FAQ0件、AI成功、AI失敗、フィードバック |

**実行コマンド**

```bash
cd backend
dotnet test
```

### 13.2 Frontendテスト

| 対象 | 内容 |
|---|---|
| Page | 主要画面の表示確認 |
| Component | 検索フォーム、FAQカード、管理画面部品 |
| lib | APIクライアント、エラーハンドリング |
| Layout | ヘッダー、管理画面レイアウト |

**実行コマンド**

```bash
cd faq-app-frontend
npm test -- --watchAll=false
```

---

## 14. デプロイ設計

### 14.1 Frontend

| 項目 | 内容 |
|---|---|
| ホスティング | Azure Static Web Apps |
| デプロイ | GitHub Actions |
| 環境変数 | `NEXT_PUBLIC_API_BASE_URL` |

### 14.2 Backend

| 項目 | 内容 |
|---|---|
| ホスティング | Heroku |
| アプリ | ASP.NET Core Web API |
| DB | MySQL |
| 環境変数 | `ConnectionStrings`、`JwtSettings`、`AiSettings`、`Cors` |

### 14.3 CI/CD

```
GitHub Repository
      |
      v
GitHub Actions
      |
      ├─ Backend Restore / Build / Test
      |
      ├─ Frontend Install / Test
      |
      ├─ Deploy Frontend
      |
      └─ Deploy Backend
```

---

## 15. 環境変数・設定

### 15.1 Backend

#### ConnectionStrings

| 項目 | 内容 |
|---|---|
| `DefaultConnection` | MySQL接続文字列 |

#### JwtSettings

| 項目 | 内容 |
|---|---|
| `Issuer` | 発行者 |
| `Audience` | 利用対象 |
| `SigningKey` | 署名キー |
| `AccessTokenMinutes` | 有効期限 |

#### AiSettings

| 項目 | 内容 |
|---|---|
| `ApiKey` | OpenAI APIキー |
| `Endpoint` | APIエンドポイント |
| `Model` | 使用モデル |
| `TimeoutSeconds` | タイムアウト秒数 |

#### Cors

| 項目 | 内容 |
|---|---|
| `AllowedOrigins` | フロントエンドURL |

### 15.2 Frontend

| 環境変数 | 内容 |
|---|---|
| `NEXT_PUBLIC_API_BASE_URL` | バックエンドAPIのベースURL |

---

## 16. ローカル起動手順

### 16.1 Backend

```bash
cd backend/FaqApp.Api
dotnet restore
dotnet ef database update
dotnet run
```

### 16.2 Frontend

```bash
cd faq-app-frontend
npm install
npm run dev
```

---

## 17. 関連ドキュメント

- ルートREADME
- アーキテクチャ
- 詳細設計書
- 要件定義
- ER図
- 画面遷移図（一般利用者）
- 画面遷移図（管理者）
- 画面イメージ

---

## 18. 補足

本基本設計書は、現在公開しているポートフォリオ版の実装済み範囲に合わせて整理している。

以下は、現時点では**今後の拡張候補**として扱う。

- CSVインポート
- ファイルアップロード
- Slack / Teams通知
- 管理ダッシュボード
- 通常検索履歴
- Editorロール
- Azure App Service
- Azure SQL Database
- Azure OpenAI Service
- SQL Server Full-Text Search
- Claude連携

現行版では、**Next.js / ASP.NET Core Web API / MySQL / OpenAI API / JWT認証 / AI検索履歴 / ユーザー管理 / デプロイ / テスト**を中心に設計を整理する。