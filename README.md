# faq-knowledge-search

社内FAQ・業務ナレッジ検索アプリです。  
FAQ、手順書、障害対応メモを登録し、キーワード・カテゴリ・タグで検索できるWebアプリです。

フェーズ1では、AI機能を含めず、FAQの検索・閲覧・管理を行う基本機能を実装しています。  
将来的には、RAG検索、AI要約、参照元・根拠表示の追加を想定しています。

---

## 概要

このアプリは、社内に散在しがちなFAQ、手順書、障害対応メモを一元管理し、  
必要な情報をすばやく検索できるようにすることを目的としたナレッジ検索アプリです。

管理者はFAQを登録・編集・削除でき、一般利用者は公開済みFAQを検索・閲覧できます。

---

## フェーズ1で実装した機能

### 一般利用者向け

- FAQ一覧表示
- FAQ詳細表示
- キーワード検索
- カテゴリによる絞り込み
- タグによる絞り込み
- 公開中FAQのみ表示
- 閲覧数の表示

### 管理者向け

- 管理者ログイン
- FAQ登録
- FAQ編集
- FAQ削除
- 公開 / 非公開の切り替え
- カテゴリ選択
- タグ設定
- 管理者用FAQ一覧

---

## 画面イメージ

### トップ画面

![トップ画面](docs/images/top.png)

### ログイン画面

![ログイン画面](docs/images/login.png)

### FAQ管理画面

![FAQ管理画面](docs/images/admin-faq-list.png)

### FAQ新規登録画面

![FAQ新規登録](docs/images/admin-faq-new.png)

### FAQ編集画面

![FAQ編集](docs/images/admin-faq-edit.png)

---

## 使用技術

### Frontend

- Next.js
- TypeScript
- React
- CSS Modules / Tailwind CSS

### Backend

- ASP.NET Core Web API
- C#
- Entity Framework Core
- ASP.NET Core Identity
- JWT認証

### Database

- SQL Server / LocalDB

---

## 主な構成

```text
faq-knowledge-search
├── backend
│   └── FaqApp.Api
│       ├── Controllers
│       ├── Data
│       ├── Entities
│       ├── DTOs
│       ├── Services
│       └── Program.cs
│
└── faq-app-frontend
    ├── src
    │   ├── app
    │   ├── components
    │   ├── lib
    │   └── types
    └── package.json
```

---

## 認証・認可

フェーズ1では、管理者向け機能にJWT認証を導入しています。

- 管理者ログイン後、JWTを保存
- FAQ登録・編集・削除は管理者のみ実行可能
- 一般利用者は公開済みFAQの検索・閲覧のみ可能

管理者の初期アカウントは、開発環境用のSeedデータとして登録します。

---

## データベース設計

フェーズ1では、以下の主なテーブルを使用しています。

- FAQs
- Categories
- Tags
- FaqTags
- SearchHistories
- AspNetUsers
- AspNetRoles
- AspNetUserRoles

FAQ、カテゴリ、タグを管理し、FAQとタグは中間テーブルで多対多の関係にしています。  
認証関連テーブルは ASP.NET Core Identity により管理しています。

---

## 今後の予定

フェーズ2以降では、以下の機能追加を予定しています。

- PDF / CSV / Markdownファイルの取込
- FAQ本文の全文検索強化
- AIによる回答要約
- RAG検索
- 回答に対する参照元・根拠表示
- 検索履歴の分析
- 管理者ダッシュボード
- 権限管理の拡張

---

## 開発状況

フェーズ1として、FAQ検索・閲覧・管理機能を実装済みです。  
現在は、AI機能を追加する前段階として、  
FAQデータの登録・検索・管理ができる基本的なWebアプリとして動作します。

---

## 備考

本アプリはポートフォリオ用途として作成しています。  
実務で扱うような社内FAQ、手順書、障害対応ナレッジの管理を想定し、  
段階的にAI検索アプリへ拡張できる構成を意識しています。