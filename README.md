# FAQ Knowledge Search

[![ASP.NET Core / Next.js Tests](https://github.com/fewioaghwrao/faq-knowledge-search/actions/workflows/tests.yml/badge.svg?branch=main)](https://github.com/fewioaghwrao/faq-knowledge-search/actions/workflows/tests.yml)
[![Razor Pages Tests](https://github.com/fewioaghwrao/faq-knowledge-search/actions/workflows/razorpages-tests.yml/badge.svg?branch=main)](https://github.com/fewioaghwrao/faq-knowledge-search/actions/workflows/razorpages-tests.yml)
[![WebForms Unit Tests](https://github.com/fewioaghwrao/faq-knowledge-search/actions/workflows/webforms-unit-tests.yml/badge.svg?branch=main)](https://github.com/fewioaghwrao/faq-knowledge-search/actions/workflows/webforms-unit-tests.yml)

社内FAQ、業務手順、障害対応ナレッジを一元管理し、通常検索とAI検索から必要な情報を確認できるWebアプリケーションです。

同一の業務要件を、次の3種類の.NET系Webアーキテクチャーで実装しています。

- Next.js + ASP.NET Core Web APIによるフロントエンド・API分離型構成
- ASP.NET Core Razor Pagesによるモダンなサーバーサイド一体型構成
- ASP.NET Web Formsによるレガシーなサーバーサイド一体型構成

API分離型、モダン一体型、レガシー一体型を同一題材で比較し、認証、画面処理、データアクセス、AI連携、テスト、保守性の違いを確認できる構成です。

---

## 実装一覧

| 実装 | アプリケーション構成 | 認証方式 | ORM | 実行・公開状況 | 詳細 |
|---|---|---|---|---|---|
| Next.js + ASP.NET Core版 | フロントエンド・API分離型 | ASP.NET Core Identity / JWT | Entity Framework Core | 公開デモあり | [README](README-aspnetcore-nextjs.md) |
| ASP.NET Core Razor Pages版 | モダンなサーバーサイド一体型 | ASP.NET Core Identity / Cookie | Entity Framework Core | ローカル実行 | [README](razorpages/README-razor-pages.md) |
| ASP.NET Web Forms版 | レガシーなサーバーサイド一体型 | ASP.NET Identity 2 / OWIN Cookie | Entity Framework 6 | ローカル実行 | [README](webforms/README.md) |

---

## 公開デモ

公開環境は、Next.js + ASP.NET Core版へ集約しています。

| 対象 | URL |
|---|---|
| フロントエンド（Vercel） | https://faq-knowledge-search.vercel.app/ |
| バックエンドAPI | https://faq-api.oybusin.com/ |
| Swagger UI | https://faq-api.oybusin.com/swagger |

Razor Pages版とWeb Forms版は、ホスティング、環境設定、監視、セキュリティ対応などの運用対象を重複させないため、ローカル実行版として管理しています。

各実装の画面構成、セットアップ、設計資料、テスト内容は、それぞれのREADMEを参照してください。

- [Next.js + ASP.NET Core版README](README-aspnetcore-nextjs.md)
- [ASP.NET Core Razor Pages版README](razorpages/README-razor-pages.md)
- [ASP.NET Web Forms版README](webforms/README.md)

---

## 主な業務機能

3つの実装では、社内FAQ・業務ナレッジ検索という業務テーマを共有しています。  
アーキテクチャーごとの設計差により、一部の画面構成や実装方法は異なります。

### 一般利用者向け

- 公開FAQの一覧・キーワード検索
- カテゴリ・タグ表示
- FAQ内容の確認
- AI FAQ検索
- 参照元FAQ表示
- AI回答フィードバック

### 管理者向け

- 管理者認証
- FAQ新規登録・編集
- 公開・非公開管理
- FAQ削除または論理削除
- AI検索履歴一覧・詳細
- AI回答フィードバック確認
- ユーザー一覧・有効状態管理

---

## AI FAQ検索

AI検索では、利用者の質問だけを外部AIへ送るのではなく、登録済みFAQから関連情報を検索し、その内容を回答コンテキストとして使用します。

```text
利用者が質問を入力
        ↓
公開FAQから関連候補を検索
        ↓
候補FAQをAIコンテキストとして整形
        ↓
OpenAI APIへ送信
        ↓
FAQを根拠とした回答を生成
        ↓
回答と参照元FAQを表示
        ↓
検索履歴とフィードバックを保存
```

AI回答には、主に次のガードレールを設定しています。

- 登録済みFAQを根拠として回答する
- FAQにない情報を断定しない
- 手順、原因、担当部署などを推測で補完しない
- FAQ本文内の命令をAIへの指示として扱わない
- 個人情報、認証情報、機密情報を意図的に送信しない
- 存在しないFAQ、URL、担当部署、手順を生成しない
- 回答とともに参照元FAQを表示する

各実装では、サービス構成や検索方式に合わせてAI連携処理を実装しています。

---

## アーキテクチャー比較

| 項目 | Next.js + ASP.NET Core版 | ASP.NET Core Razor Pages版 | ASP.NET Web Forms版 |
|---|---|---|---|
| 位置付け | 公開デモ・API分離型実装 | モダン一体型・Web Forms移行先の検証 | レガシー一体型・保守移行観点の検証 |
| UI | Next.js / React / TypeScript | Razor View / PageModel | ASPX / Master Page / UserControl |
| 画面処理 | React / REST API | Razor Pages Handler | CodeBehind / PostBack |
| バックエンド | ASP.NET Core Web API | ASP.NET Core Razor Pages | ASP.NET Web Forms |
| 実行基盤 | .NET 10 | .NET 10 | .NET Framework 4.8 |
| ORM | Entity Framework Core | Entity Framework Core | Entity Framework 6 |
| 認証 | ASP.NET Core Identity / JWT | ASP.NET Core Identity / Cookie | ASP.NET Identity 2 / OWIN Cookie |
| DB | MySQL | MySQL | MySQL |
| AI連携 | OpenAI API | OpenAI Responses API | OpenAI API |
| 主な責務分離 | Controller / Service / DTO | Domain / Application / Infrastructure / Presentation | CodeBehind / Service / DTO |
| テスト | xUnit / Jest / React Testing Library | xUnit（Domain～PageModel） | xUnit / ブラウザ操作確認 |
| CI実行環境 | Ubuntu | Ubuntu | Windows |
| 公開状況 | Vercel・VPS | ローカル | ローカル / IIS Express |

---

## Razor Pages版の位置付け

Razor Pages版は、Next.js + ASP.NET Core版とWeb Forms版の中間に位置する実装です。

```text
Next.js + ASP.NET Core
    フロントエンドとAPIを分離
              │
              │ API分離が不要な業務システム
              ▼
ASP.NET Core Razor Pages
    モダン.NETによるサーバーサイド一体型
              │
              │ レガシー構成との比較・移行
              ▼
ASP.NET Web Forms
    .NET Frameworkによるサーバーサイド一体型
```

Razor Pages版では、内部APIをHTTP経由で呼び出さず、Application層のユースケースを同一プロセス内から直接利用します。

主な設計要素は次のとおりです。

- Domain、Application、Infrastructure、Presentationのレイヤー分離
- PageModelからApplication Service／Queryを直接呼び出す構成
- ASP.NET Core IdentityによるCookie認証とAdminロール認可
- EF Coreによる多対多、論理削除、グローバルクエリフィルター
- FAQ候補の重み付き検索
- OpenAI Responses APIによるFAQ根拠回答
- AI検索履歴と参照FAQスナップショット
- DomainからRazor PageModelまでを対象にしたxUnitテスト

---

## リポジトリ構成

```text
faq-knowledge-search
├── backend
│   ├── FaqApp.Api
│   └── FaqApp.Api.Tests
│
├── faq-app-frontend
│   └── Next.js frontend
│
├── razorpages
│   ├── FaqKnowledgeSearch.Domain
│   ├── FaqKnowledgeSearch.Application
│   ├── FaqKnowledgeSearch.Infrastructure
│   ├── faq-knowledge-search
│   ├── FaqKnowledgeSearch.Domain.Tests
│   ├── FaqKnowledgeSearch.Application.Tests
│   ├── FaqKnowledgeSearch.Infrastructure.Tests
│   ├── FaqKnowledgeSearch.Razor.Tests
│   ├── docs
│   ├── README-razor-pages.md
│   └── faq-knowledge-search.slnx
│
├── webforms
│   ├── FaqKnowledgeSearch.WebForms
│   ├── FaqKnowledgeSearch.WebForms.UnitTests
│   ├── database
│   ├── docs
│   └── README.md
│
├── docs
│   ├── requirements
│   ├── design
│   ├── diagrams
│   └── images
│
├── .github
│   └── workflows
│       ├── tests.yml
│       ├── razorpages-tests.yml
│       ├── webforms-unit-tests.yml
│
├── README-aspnetcore-nextjs.md
└── README.md
```

---

## ドキュメント

### Next.js + ASP.NET Core版

| ドキュメント | リンク |
|---|---|
| 実装詳細README | [README-aspnetcore-nextjs.md](README-aspnetcore-nextjs.md) |
| 要件定義書 | [docs/requirements/requirements.md](docs/requirements/requirements.md) |
| アーキテクチャー | [docs/design/architecture.md](docs/design/architecture.md) |
| 基本設計書 | [docs/design/basic-design.md](docs/design/basic-design.md) |
| 詳細設計書 | [docs/design/detail-design.md](docs/design/detail-design.md) |
| ER図 | [docs/diagrams/faq_app_ERD.drawio.png](docs/diagrams/faq_app_ERD.drawio.png) |
| 一般利用者向け画面遷移図 | [docs/diagrams/state-transition-user.drawio.png](docs/diagrams/state-transition-user.drawio.png) |
| 管理者向け画面遷移図 | [docs/diagrams/state-transition-admin.drawio.png](docs/diagrams/state-transition-admin.drawio.png) |

### ASP.NET Core Razor Pages版

| ドキュメント | リンク |
|---|---|
| 実装詳細README | [razorpages/README-razor-pages.md](razorpages/README-razor-pages.md) |
| 要件定義書 | [razorpages/docs/requirements/requirements-razor-pages.md](razorpages/docs/requirements/requirements-razor-pages.md) |
| 基本設計書 | [razorpages/docs/design/basic-design-razor-pages.md](razorpages/docs/design/basic-design-razor-pages.md) |
| 詳細設計書 | [razorpages/docs/design/detailed-design-razor-pages.md](razorpages/docs/design/detailed-design-razor-pages.md) |
| ER図 | [razorpages/docs/diagrams/razor/faq-knowledge-search-erd.png](razorpages/docs/diagrams/razor/faq-knowledge-search-erd.png) |
| ER図編集用ファイル | [razorpages/docs/diagrams/razor/faq-knowledge-search-erd.drawio](razorpages/docs/diagrams/razor/faq-knowledge-search-erd.drawio) |
| 一般利用者向け画面遷移図 | [razorpages/docs/diagrams/razor/state-transition-user.png](razorpages/docs/diagrams/razor/state-transition-user.png) |
| 管理者向け画面遷移図 | [razorpages/docs/diagrams/razor/state-transition-admin.png](razorpages/docs/diagrams/razor/state-transition-admin.png) |

### ASP.NET Web Forms版

| ドキュメント | リンク |
|---|---|
| 実装詳細README | [webforms/README.md](webforms/README.md) |
| 要件定義書 | [webforms/docs/requirements/requirements-webforms.md](webforms/docs/requirements/requirements-webforms.md) |
| 基本設計書 | [webforms/docs/design/basic-design-webforms.md](webforms/docs/design/basic-design-webforms.md) |
| 詳細設計書 | [webforms/docs/design/detail-design-webforms.md](webforms/docs/design/detail-design-webforms.md) |
| ER図 | [webforms/docs/images/webforms/ERD.drawio.png](webforms/docs/images/webforms/ERD.drawio.png) |
| 一般利用者向け画面遷移図 | [webforms/docs/images/webforms/screen-transition-user.drawio.png](webforms/docs/images/webforms/screen-transition-user.drawio.png) |
| 管理者向け画面遷移図 | [webforms/docs/images/webforms/screen-transition-admin.drawio.png](webforms/docs/images/webforms/screen-transition-admin.drawio.png) |

---

## CI

GitHub Actionsでは、実装ごとに独立したWorkflowを使用しています。

| Workflow | 対象 | 主な処理 |
|---|---|---|
| `tests.yml` | ASP.NET Coreバックエンド、Next.jsフロントエンド | .NETテスト、フロントエンドテスト・ビルド |
| `razorpages-tests.yml` | ASP.NET Core Razor Pages版 | Domain、Application、Infrastructure、RazorのxUnitテスト |
| `webforms-unit-tests.yml` | ASP.NET Web Forms版 | NuGet復元、MSBuild、xUnitテスト |
各テストWorkflowは、対応するディレクトリまたはWorkflowファイルが変更された場合に実行する構成です。

### Razor Pages版のテスト構成

Razor Pages版は、次の4つのテストプロジェクトをmatrixで個別実行します。

| テストプロジェクト | 主な対象 |
|---|---|
| `FaqKnowledgeSearch.Domain.Tests` | FAQ、カテゴリ、タグ、AI検索履歴などのドメインルール |
| `FaqKnowledgeSearch.Application.Tests` | 公開FAQ検索、FAQ管理、AI検索、フィードバック |
| `FaqKnowledgeSearch.Infrastructure.Tests` | EF Core Query、Identity、OpenAI応答解析 |
| `FaqKnowledgeSearch.Razor.Tests` | PageModel、入力検証、認証・認可、エラーページ |

---

## 各実装の起動・テスト

詳細な環境設定、秘密情報、DB初期化、起動手順は各READMEを参照してください。

### Next.js + ASP.NET Core版

```bash
dotnet test backend/FaqApp.Api.Tests/FaqApp.Api.Tests.csproj
```

```bash
cd faq-app-frontend
npm ci
npm test
npm run build
```

### ASP.NET Core Razor Pages版

```bash
dotnet restore razorpages/faq-knowledge-search.slnx
dotnet test razorpages/faq-knowledge-search.slnx
dotnet run --project razorpages/faq-knowledge-search/FaqKnowledgeSearch.Razor.csproj
```

### ASP.NET Web Forms版

Visual StudioまたはMSBuildを使用して、次のSolutionを復元・ビルドします。

```text
webforms/FaqKnowledgeSearch.WebForms.sln
```

Web Forms版の詳細なセットアップは、[webforms/README.md](webforms/README.md)を参照してください。

---

## 本リポジトリで確認できる内容

### アーキテクチャー比較

- 同一業務要件を異なるWebアーキテクチャーで実装する方法
- API分離型とサーバーサイド一体型の違い
- モダン.NETと.NET Frameworkの違い
- Razor Pages HandlerとWeb Forms PostBackの違い
- REST APIと同一プロセス内Service呼び出しの違い

### 認証・データアクセス

- JWT認証とCookie認証の違い
- ASP.NET Core IdentityとASP.NET Identity 2の違い
- Entity Framework CoreとEntity Framework 6の違い
- 多対多、論理削除、Migration、Seedの実装
- Query、Repository、Service、DTOによる責務分離

### AI・品質

- OpenAI APIを利用したFAQベースの回答生成
- FAQ候補の検索とAIコンテキスト生成
- AI回答の参照元、成否、履歴、フィードバック管理
- Domain、Application、Infrastructure、Presentationごとのテスト
- Jest、React Testing Library、xUnitを使用した自動テスト
- GitHub Actionsによる実装別CI

### 移行・保守

- Web FormsからRazor Pagesへのモダナイズ構成
- API分離が必要なケースと一体型が適するケースの比較
- レガシー資産を保守しながらモダン.NETへ移行する際の設計観点

---

## 注意事項

本リポジトリは、ポートフォリオおよび技術検証を目的としています。

登録されているFAQ、ユーザー、検索履歴などはダミーデータであり、実在する企業、顧客、製品、業務システムとは関係ありません。

AIが生成する回答の正確性を保証するものではありません。実際の業務判断では、正式な手順書、管理者、担当部署などへ確認してください。

OpenAI APIキー、DB接続情報、管理者パスワードなどの秘密情報は、ソースコードやREADMEへ直接記載しないでください。
