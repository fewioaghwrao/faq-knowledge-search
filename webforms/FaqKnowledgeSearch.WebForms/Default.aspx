<%@ Page
    Title="トップ"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Default.aspx.cs"
    Inherits="FaqKnowledgeSearch.WebForms._Default" %>

<asp:Content
    ID="BodyContent"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <main class="home-page">

        <section class="home-hero">
            <span class="home-hero__badge">
                FAQ Knowledge Search
            </span>

            <h1 class="home-hero__title">
                社内FAQナレッジ検索
            </h1>

            <p class="home-hero__description">
                手順書・FAQ・障害対応メモを登録し、
                通常検索とAI検索の両方から必要な情報を探せるアプリです。
            </p>

            <div class="home-hero__actions">
                <a
                    runat="server"
                    href="~/Faqs/Index.aspx"
                    class="btn home-primary-button">
                    通常FAQ検索を使う
                </a>

                <a
                    runat="server"
                    href="~/AiSearch/Index.aspx"
                    class="btn home-secondary-button">
                    AI FAQ検索を使う
                </a>
            </div>
        </section>

        <section class="row g-4 mt-2">

            <div class="col-md-6">
                <a
                    runat="server"
                    href="~/Faqs/Index.aspx"
                    class="home-feature-card">

                    <span class="home-feature-card__label">
                        FAQ Search
                    </span>

                    <h2 class="home-feature-card__title">
                        通常FAQ検索
                    </h2>

                    <p class="home-feature-card__description">
                        キーワード・カテゴリ・タグをもとに、
                        登録済みのFAQを検索します。
                    </p>

                    <span class="home-feature-card__link">
                        検索画面へ →
                    </span>
                </a>
            </div>

            <div class="col-md-6">
                <a
                    runat="server"
                    href="~/AiSearch/Index.aspx"
                    class="home-feature-card">

                    <span class="home-feature-card__label">
                        AI FAQ Search
                    </span>

                    <h2 class="home-feature-card__title">
                        AI FAQ検索
                    </h2>

                    <p class="home-feature-card__description">
                        関連FAQを検索し、
                        FAQを根拠にAI回答と参照元を表示します。
                    </p>

                    <span class="home-feature-card__link">
                        AI検索画面へ →
                    </span>
                </a>
            </div>

        </section>

    </main>

</asp:Content>