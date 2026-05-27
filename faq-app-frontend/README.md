# Frontend - FAQ Knowledge Search

社内FAQ・業務ナレッジ検索アプリのフロントエンドです。

Next.js / React / TypeScript を使用し、FAQ検索、AI FAQ検索、管理画面、AI検索履歴、ユーザー管理画面を提供します。

---

## 使用技術

- Next.js
- React
- TypeScript
- CSS / Tailwind CSS
- Jest
- React Testing Library
- Azure Static Web Apps

---

## 主な機能

### 一般利用者向け

- ホーム画面
- FAQ一覧表示
- FAQ詳細表示
- キーワード検索
- AI FAQ検索
- AI回答表示
- 参照元FAQ表示
- AI回答フィードバック

### 管理者向け

- 管理者ログイン
- ログアウト確認
- 管理画面
- FAQ新規登録
- FAQ編集
- FAQ削除
- AI検索履歴一覧
- AI検索履歴詳細
- ユーザー管理
- ユーザー有効 / 無効管理
- ユーザーロール管理

---

## ディレクトリ構成

```text
faq-app-frontend
├── src
│   ├── app
│   ├── components
│   ├── lib
│   └── types
│
├── package.json
├── next.config.ts
├── jest.config.ts
└── jest.setup.ts
```

### 主なディレクトリ

| ディレクトリ | 役割 |
|---|---|
| src/app | App Router のページ・ルーティング |
| src/components | 共通コンポーネント |
| src/lib | API通信などの共通処理 |
| src/types | APIレスポンスや画面表示用の型定義 |

---

## 主な画面

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

## 環境変数

`.env.local` を作成し、バックエンドAPIのURLを設定します。

```env
NEXT_PUBLIC_API_BASE_URL=http://localhost:xxxx
```

デモ環境では以下のバックエンドAPIを使用します。

```env
NEXT_PUBLIC_API_BASE_URL=https://faq-app-api-d060ab93d646.herokuapp.com
```

---

## ローカル起動手順

### 1. フロントエンドディレクトリへ移動

```bash
cd faq-app-frontend
```

### 2. パッケージインストール

```bash
npm install
```

### 3. 環境変数ファイル作成

```bash
copy .env.example .env.local
```

または `.env.local` を手動作成します。

```env
NEXT_PUBLIC_API_BASE_URL=http://localhost:xxxx
```

### 4. 開発サーバー起動

```bash
npm run dev
```

起動後、以下で画面を確認します。

```
http://localhost:3000
```

---

## テスト実行

```bash
npm test -- --watchAll=false
```

---

## ビルド

```bash
npm run build
```

---

## デプロイ

フロントエンドは Azure Static Web Apps へデプロイしています。

### デモ環境

```
https://green-bush-0db40ef00.7.azurestaticapps.net/
```

GitHub Actions により、mainブランチへのpushを契機にデプロイされます。

---

## API連携

フロントエンドは `NEXT_PUBLIC_API_BASE_URL` をもとに、ASP.NET Core Web API と通信します。

```
[Next.js Frontend]
      |
      | REST API / JSON
      v
[ASP.NET Core Web API]
```

主なAPI連携内容は以下です。

- FAQ一覧取得
- FAQ詳細取得
- FAQ登録・編集・削除
- ログイン
- AI FAQ検索
- AI検索履歴取得
- AI回答フィードバック送信
- ユーザー管理

---

## 認証

管理画面ではログイン後に発行されるJWTを使用します。

```
Login
  ↓
JWT取得
  ↓
管理画面APIへリクエスト
  ↓
バックエンド側で認証・認可
```

---

## 関連ドキュメント

- [ルートREADME](../../README.md)
- [要件定義](../requirements/)
- [設計資料](./)
- [画面イメージ](../images/)

