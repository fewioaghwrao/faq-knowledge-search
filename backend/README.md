# Backend - FAQ Knowledge Search

社内FAQ・業務ナレッジ検索アプリのバックエンドです。

ASP.NET Core Web API を使用し、FAQ管理、JWT認証、ユーザー管理、AI FAQ検索、AI検索履歴管理を提供します。

---

## 使用技術

- C#
- ASP.NET Core Web API
- Entity Framework Core
- ASP.NET Core Identity
- JWT認証
- MySQL
- OpenAI API
- xUnit
- Heroku

---

## 主な機能

- 管理者ログイン
- JWTトークン発行
- FAQ一覧取得
- FAQ詳細取得
- FAQ新規登録
- FAQ編集
- FAQ削除
- FAQ公開 / 非公開管理
- キーワード検索
- AI FAQ検索
- OpenAI API連携
- AI検索履歴保存
- AI検索履歴一覧・詳細取得
- AI回答フィードバック保存
- ユーザー一覧取得
- ユーザー有効 / 無効切り替え
- ユーザーロール管理

---

## ディレクトリ構成

```text
backend
├── FaqApp.Api
│   ├── Controller
│   ├── Data
│   ├── Dtos
│   ├── Entities
│   ├── Migrations
│   ├── Services
│   ├── Settings
│   ├── Program.cs
│   └── appsettings.json
│
└── FaqApp.Api.Tests
    ├── Controllers
    └── Services
```

---

## レイヤー構成

```
Controller
  ↓
Service
  ↓
DbContext / Entity
  ↓
MySQL
```

| 層 | 役割 |
|---|---|
| Controller | APIエンドポイント、認証・認可、HTTPレスポンス返却 |
| Service | FAQ検索、AI回答生成、履歴保存、ユーザー管理などの業務ロジック |
| DTO | APIの入出力モデル |
| Entity | DB永続化モデル |
| DbContext | EF Core によるDB操作 |
| Migrations | DBスキーマ管理 |
| Tests | Controller / Service の単体テスト |

---

## 主なAPI

| Controller | 役割 |
|---|---|
| AuthController | ログイン、JWT発行 |
| FaqsController | FAQ一覧、詳細、登録、編集、削除 |
| AiController | AI FAQ検索、AI検索履歴、フィードバック |
| UsersController | ユーザー管理 |

---

## 主なEntity

| Entity | 役割 |
|---|---|
| ApplicationUser | Identityユーザー |
| Faq | FAQ本文、公開状態、カテゴリ、閲覧数など |
| Category | FAQカテゴリ |
| Tag | FAQタグ |
| AiSearchHistory | AI検索履歴 |
| AiSearchHistorySource | AI回答に利用した参照元FAQ |

---

## 環境変数・設定

### Database

ローカル環境では `appsettings.Development.json` または User Secrets で接続文字列を設定します。

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;port=3306;database=faq_app;user=root;password=your-password"
  }
}
```

### JWT Settings

```json
{
  "JwtSettings": {
    "Issuer": "FaqApp",
    "Audience": "FaqApp",
    "SigningKey": "your-signing-key",
    "AccessTokenMinutes": 60
  }
}
```

### AI Settings

OpenAI API連携を利用する場合は、以下を設定します。

```json
{
  "AiSettings": {
    "ApiKey": "your-api-key",
    "Endpoint": "your-api-endpoint",
    "Model": "your-model"
  }
}
```

> APIキーは `appsettings.json` に直接記載せず、User Secrets またはホスティング環境の環境変数で管理します。

### User Secrets 設定例

```bash
cd backend/FaqApp.Api

dotnet user-secrets init

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "server=localhost;port=3306;database=faq_app;user=root;password=your-password"

dotnet user-secrets set "JwtSettings:SigningKey" "your-signing-key"

dotnet user-secrets set "AiSettings:ApiKey" "your-api-key"
dotnet user-secrets set "AiSettings:Endpoint" "your-api-endpoint"
dotnet user-secrets set "AiSettings:Model" "your-model"
```

---

## ローカル起動手順

### 1. バックエンドディレクトリへ移動

```bash
cd backend/FaqApp.Api
```

### 2. パッケージ復元

```bash
dotnet restore
```

### 3. DBマイグレーション実行

```bash
dotnet ef database update
```

### 4. アプリ起動

```bash
dotnet run
```

起動後、APIはローカル環境で利用できます。

```
https://localhost:xxxx
http://localhost:xxxx
```

---

## テスト実行

```bash
cd backend
dotnet test
```

またはソリューション / テストプロジェクト単位で実行します。

```bash
dotnet test FaqApp.Api.Tests
```

---

## デプロイ

バックエンドは Heroku へデプロイしています。

### デモ環境

```
https://faq-app-api-d060ab93d646.herokuapp.com/
```

Heroku環境では、接続文字列、JWT設定、AI API設定を環境変数として管理します。

---

## 関連ドキュメント

- [ルートREADME](../../README.md)
- [要件定義](../requirements/)
- [設計資料](./)
- [画面イメージ](../images/)
