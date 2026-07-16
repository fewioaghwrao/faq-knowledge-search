# FAQ Knowledge Search

[![ASP.NET Core / Next.js Tests](https://github.com/fewioaghwrao/faq-knowledge-search/actions/workflows/tests.yml/badge.svg?branch=main)](https://github.com/fewioaghwrao/faq-knowledge-search/actions/workflows/tests.yml)
[![WebForms Unit Tests](https://github.com/fewioaghwrao/faq-knowledge-search/actions/workflows/webforms-unit-tests.yml/badge.svg?branch=main)](https://github.com/fewioaghwrao/faq-knowledge-search/actions/workflows/webforms-unit-tests.yml)

社内FAQ、業務手順、障害対応ナレッジを一元管理し、通常検索とAI検索から必要な情報を確認できるWebアプリケーションです。

同一の業務要件を、異なる.NET系Webアーキテクチャーで実装しています。

- Next.js + ASP.NET Core Web APIによるフロントエンド・API分離型構成
- ASP.NET Web Formsによるサーバーサイド一体型構成

---

## 実装一覧

| 実装 | アプリケーション構成 | 認証方式 | 実行・公開状況 | 詳細 |
|---|---|---|---|---|
| Next.js + ASP.NET Core版 | フロントエンド・API分離型 | ASP.NET Core Identity / JWT | 公開デモあり | [README](README-aspnetcore-nextjs.md) |
| ASP.NET Web Forms版 | サーバーサイド一体型 | ASP.NET Identity 2 / OWIN Cookie | ローカル実行 | [README](webforms/README.md) |

---

## 公開デモ

公開環境は、Next.js + ASP.NET Core版へ集約しています。

| 対象 | URL |
|---|---|
| フロントエンド（Vercel） | https://faq-knowledge-search.vercel.app/ |
| フロントエンド（Azure Static Web Apps） | https://green-bush-0db40ef00.7.azurestaticapps.net/ |
| バックエンドAPI | https://faq-app-api-d060ab93d646.herokuapp.com/ |
| Swagger UI | https://faq-app-api-d060ab93d646.herokuapp.com/swagger |

Web Forms版は、ホスティング、環境設定、監視、セキュリティ対応などの運用対象を重複させないため、ローカル実行版として管理しています。

画面構成や動作については、[Web Forms版README](webforms/README.md)のスクリーンショット、画面遷移図、ER図、テスト結果を参照してください。

---

## 共通する業務要件

両バージョンでは、主に次の業務要件を共通化しています。

### 一般利用者向け

- 公開FAQの一覧・キーワード検索
- FAQ詳細表示
- カテゴリ・タグ表示
- AI FAQ検索
- 参照元FAQ表示
- AI回答フィードバック

### 管理者向け

- 管理者認証
- FAQ新規登録・編集
- 公開・非公開管理
- FAQ削除
- AI検索履歴一覧・詳細
- AI回答フィードバック確認
- ユーザー一覧・有効状態管理

---

## AI FAQ検索

AI検索では、利用者の質問だけを外部AI APIへ送るのではなく、登録済みのFAQから関連情報を検索し、その内容をコンテキストとして回答を生成します。

```text
利用者が質問を入力
        ↓
関連する公開FAQを検索
        ↓
FAQをAIコンテキストとして整形
        ↓
OpenAI APIへ送信
        ↓
FAQを根拠とした回答を生成
        ↓
回答と参照元FAQを表示
        ↓
検索履歴とフィードバックを保存
```

AI回答には、次のガードレールを設定しています。

- 登録済みFAQを根拠として回答する
- FAQにない情報を断定しない
- 手順、原因、担当部署などを推測で補完しない
- 個人情報、認証情報、機密情報を出力しない
- 参照元FAQの確認を促す

---

## アーキテクチャー比較

| 項目 | Next.js + ASP.NET Core版 | ASP.NET Web Forms版 |
|---|---|---|
| 位置付け | 公開デモ・API分離型実装 | 一体型構成の実装検証 |
| UI | Next.js / React / TypeScript | ASPX / Master Page / UserControl |
| 画面処理 | React / REST API | CodeBehind / PostBack |
| バックエンド | ASP.NET Core Web API | ASP.NET Web Forms |
| ORM | Entity Framework Core | Entity Framework 6 |
| 認証 | ASP.NET Core Identity / JWT | ASP.NET Identity 2 / OWIN Cookie |
| DB | MySQL | MySQL |
| AI連携 | OpenAI API | OpenAI API |
| テスト | xUnit / Jest / React Testing Library | xUnit / ブラウザ操作確認 |
| 公開状況 | Vercel・Azure・Heroku | ローカル / IIS Express |

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
│       └── webforms-unit-tests.yml
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

| Workflow | 対象 |
|---|---|
| tests.yml | ASP.NET Coreバックエンド、Next.jsフロントエンド |
| webforms-unit-tests.yml | ASP.NET Web Forms版のビルド、xUnitテスト |

変更されたディレクトリに応じて、対象となるテストだけを実行する構成です。

---

## 本リポジトリで確認できる内容

- 同一業務要件を異なるWebアーキテクチャーで実装する方法
- ASP.NET Core Web APIとNext.jsによるAPI分離型構成
- ASP.NET Web Formsによるサーバーサイド一体型構成
- JWT認証とCookie認証の違い
- Entity Framework CoreとEntity Framework 6の違い
- REST APIとPostBackイベント処理の違い
- Service層・DTOによる責務分離
- OpenAI APIを利用したFAQベースの回答生成
- AI回答の参照元管理
- AI検索履歴・フィードバック管理
- ユニットテストとGitHub ActionsによるCI

---

## 注意事項

本リポジトリは、ポートフォリオおよび技術検証を目的としています。

登録されているFAQ、ユーザー、検索履歴などはダミーデータであり、実在する企業、顧客、製品、業務システムとは関係ありません。

AIが生成する回答の正確性を保証するものではありません。実際の業務判断では、正式な手順書、管理者、担当部署などへ確認してください。