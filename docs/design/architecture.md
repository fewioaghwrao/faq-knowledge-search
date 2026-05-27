# Architecture - FAQ Knowledge Search

社内FAQ・業務ナレッジ検索アプリのアーキテクチャ資料です。

本アプリは、Next.js フロントエンドと ASP.NET Core Web API バックエンドを分離した構成で実装しています。
FAQ検索、AI回答生成、参照元FAQ表示、AI検索履歴管理、ユーザー管理を備えた、社内ナレッジ検索向けのWebアプリです。

---

## 全体構成

本アプリは、以下のような3層構成を基本としています。

```text
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

### 現在のデモ環境

| 区分 | 内容 |
|---|---|
| Frontend | Next.js / React / TypeScript |
| Frontend Hosting | Azure Static Web Apps |
| Backend | ASP.NET Core Web API / C# / .NET |
| Backend Hosting | Heroku |
| Database | MySQL |
| ORM | Entity Framework Core |
| Authentication | JWT認証 / ASP.NET Core Identity |
| AI | OpenAI API |
| CI/CD | GitHub Actions |

---

## アプリケーション構成

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

## バックエンド構成

バックエンドは ASP.NET Core Web API で構築しています。

### レイヤー構成

```
Controller
  ↓
Service
  ↓
DbContext / Entity
  ↓
MySQL
```

### 各層の責務

| 層 | 主な責務 |
|---|---|
| Controller | APIエンドポイント、認証・認可、HTTPレスポンス変換 |
| Service | FAQ検索、AI回答生成、履歴保存、ユーザー管理などの業務ロジック |
| DTO | APIの入出力モデル |
| Entity | DB永続化モデル |
| DbContext | EF Core によるDB操作 |
| Migration | DBスキーマ管理 |
| Tests | Controller / Service の単体テスト |

### 主なバックエンド機能

- JWT認証
- ASP.NET Core Identity によるユーザー・ロール管理
- FAQ一覧・詳細取得
- FAQ登録・編集・削除
- FAQ公開 / 非公開管理
- キーワード検索
- AI FAQ検索
- OpenAI API連携
- 参照元FAQ表示
- AI検索履歴保存
- AI回答フィードバック保存
- ユーザー管理
- Controller / Service の単体テスト

---

## フロントエンド構成

フロントエンドは Next.js App Router で構築しています。

### 主な構成

| ディレクトリ | 役割 |
|---|---|
| src/app | ページ・ルーティング |
| src/components | 共通コンポーネント |
| src/lib | API通信処理 |
| src/types | APIレスポンスや画面表示用の型定義 |

### 主な画面

| URL | 画面 |
|---|---|
| / | ホーム画面 |
| /faqs | FAQ検索 |
| /faqs/[id] | FAQ詳細 |
| /ai-search | AI FAQ検索 |
| /login | 管理者ログイン |
| /admin | 管理画面 |
| /admin/faqs/new | FAQ新規登録 |
| /admin/faqs/[id]/edit | FAQ編集 |
| /admin/ai-histories | AI検索履歴一覧 |
| /admin/ai-histories/[id] | AI検索履歴詳細 |
| /admin/users | ユーザー管理 |

---

## AI検索フロー

AI検索では、ユーザーの質問文をそのままAIへ送るのではなく、バックエンド側で関連FAQを検索し、その結果をAIのコンテキストとして利用します。

```
User
  ↓
Next.js Frontend
  ↓
POST /api/ai/search
  ↓
ASP.NET Core Web API
  ↓
FAQ Search
  ↓
Top FAQ Context
  ↓
OpenAI API
  ↓
AI Answer
  ↓
Save AI Search History
  ↓
Return Answer + Sources
```

### 処理の流れ

1. ユーザーがAI検索画面で質問を入力
2. フロントエンドからバックエンドAPIへリクエスト
3. バックエンドで関連FAQを検索
4. 上位FAQをAIコンテキストとして整形
5. OpenAI APIへ送信
6. FAQに基づいた回答を生成
7. 回答と参照元FAQを画面に返却
8. AI検索履歴としてDBに保存
9. ユーザーからのフィードバックを記録

---

## AI回答のガードレール

AI回答では、FAQに基づいた回答を行うため、以下の方針を設定しています。

- 提供されたFAQコンテキストのみを根拠に回答する
- FAQに記載されていない内容は断言しない
- FAQにない手順・原因・担当部署・問い合わせ先を推測しない
- 機密情報、個人情報、認証情報を出力しない
- 回答末尾に参照元FAQの確認を促す
- 参照元FAQを画面に表示する

---

## 認証・認可

管理者向け機能にはJWT認証を導入しています。

```
Login
  ↓
JWT Issued
  ↓
Frontend stores token
  ↓
Request Admin API with token
  ↓
Backend validates JWT
  ↓
Authorize Admin Operation
```

### 認証・認可の方針

| 区分 | 内容 |
|---|---|
| 一般利用者 | 公開FAQの検索・閲覧、AI検索を利用 |
| 管理者 | FAQ管理、AI検索履歴確認、ユーザー管理を利用 |
| 無効ユーザー | ログイン不可 |
| 管理API | 認証済みユーザーのみアクセス可能 |

---

## データベース構成

主なテーブルは以下です。

| テーブル | 概要 |
|---|---|
| Faqs | FAQ本文、公開状態、カテゴリ、閲覧数など |
| Categories | FAQカテゴリ |
| Tags | FAQタグ |
| AiSearchHistories | AI検索履歴 |
| AiSearchHistorySources | AI回答に利用した参照元FAQ |
| AspNetUsers | Identityユーザー |
| AspNetRoles | Identityロール |
| AspNetUserRoles | ユーザーとロールの関連 |

---

## CI/CD・デプロイ構成

CI/CDにはGitHub Actionsを使用しています。

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
      ├─ Deploy Frontend to Azure Static Web Apps
      |
      └─ Deploy Backend to Heroku
```

### Frontend Deploy

```
GitHub Actions
  ↓
Azure Static Web Apps
  ↓
Next.js Frontend
```

### Backend Deploy

```
GitHub / Heroku
  ↓
Heroku
  ↓
ASP.NET Core Web API
  ↓
MySQL
```

### 外部サービス連携

| 外部サービス | 用途 |
|---|---|
| OpenAI API | FAQコンテキストに基づくAI回答生成 |
| Azure Static Web Apps | フロントエンドホスティング |
| Heroku | バックエンドAPIホスティング |
| MySQL | FAQ、AI検索履歴、ユーザー情報の保存 |
| GitHub Actions | テスト・デプロイ自動化 |

---

## 設計上のポイント

### 1. フロントエンド / バックエンド分離

Next.js と ASP.NET Core Web API を分離し、画面表示とAPI処理の責務を分けています。

### 2. FAQ検索とAI回答の分離

AI検索では、AIに直接質問を投げるのではなく、まずFAQ検索を行い、その結果をAIコンテキストとして利用します。
これにより、回答の根拠をFAQに寄せやすくしています。

### 3. 参照元FAQの表示

AI回答だけでなく、回答の根拠となったFAQを表示します。
ポートフォリオとしても、RAG風の構成や説明可能性を示しやすいポイントです。

### 4. AI検索履歴の保存

AI検索の質問、回答、成否、参照元FAQ、フィードバックを保存します。
管理者がAI機能の利用状況や改善ポイントを確認できる構成です。

### 5. 管理機能の実装

FAQ登録・編集・削除、AI検索履歴、ユーザー管理を管理画面にまとめています。
単なる検索アプリではなく、業務管理アプリとして説明しやすい構成です。

### 6. テストしやすいService構成

Controller と Service を分け、Service単位で業務ロジックをテストしやすい構成にしています。
バックエンドは xUnit、フロントエンドは Jest / React Testing Library を使用しています。

---

## 設計上の拡張候補

以下は、現時点では今後の拡張候補です。

- CSVインポート
- ファイルアップロード
- Slack / Teams通知
- FAQ改善提案
- 管理ダッシュボード
- カテゴリ別アクセス統計
- 検索サジェスト
- よく見られているFAQ表示
- Azure App Service への移行
- Azure SQL Database への移行
- Azure OpenAI Service への移行

---

## 関連ドキュメント

- [ルートREADME](../../README.md)
- [要件定義書](../requirements/requirements.md)
- [アーキテクチャ](./architecture.md)
- [基本設計書](./basic-design.md)
- [詳細設計書](./detail-design.md)
- [ER図](../diagrams/faq_app_ERD.drawio.png)
- [画面遷移図（一般利用者）](../diagrams/state-transition-user.drawio.png)
- [画面遷移図（管理者）](../diagrams/state-transition-admin.drawio.png)
- [画面イメージ](../images/)