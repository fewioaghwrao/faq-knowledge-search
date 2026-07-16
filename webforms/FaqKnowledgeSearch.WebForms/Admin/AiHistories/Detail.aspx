<%@ Page
    Title="AI検索履歴詳細"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Detail.aspx.cs"
    Inherits="FaqKnowledgeSearch.WebForms.Admin.AiHistories.Detail" %>

<asp:Content
    ID="BodyContent"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <main class="container py-4 admin-ai-history-page admin-ai-detail-page">

        <%-- ページ見出し --%>
        <div class="admin-ai-detail-heading">

            <div>
                <span class="admin-ai-history-eyebrow">
                    AI Search History
                </span>

                <h1 class="h2 mb-1">
                    AI検索履歴詳細
                </h1>

                <p class="text-muted mb-0">
                    AI検索時の質問、回答、実行結果、参照元FAQを確認します。
                </p>
            </div>

            <a
                runat="server"
                href="~/Admin/AiHistories/Index.aspx"
                class="btn btn-outline-secondary">
                一覧へ戻る
            </a>

        </div>

        <%-- エラー表示 --%>
        <asp:Panel
            ID="MessagePanel"
            runat="server"
            Visible="false"
            CssClass="alert alert-danger admin-ai-detail-message"
            role="alert">

            <asp:Literal
                ID="MessageLiteral"
                runat="server"
                Mode="Encode" />

        </asp:Panel>

        <%-- 詳細本体 --%>
        <asp:Panel
            ID="DetailContentPanel"
            runat="server"
            Visible="false">

            <%-- AI検索履歴本体 --%>
            <section class="admin-ai-detail-card">

                <div class="admin-ai-detail-card__accent"></div>

                <div class="admin-ai-detail-card__body">

                    <%-- メタ情報 --%>
                    <div class="admin-ai-detail-meta">

                        <span class="admin-ai-detail-badge">
                            ID:
                            <asp:Literal
                                ID="HistoryIdLiteral"
                                runat="server" />
                        </span>

                        <asp:Label
                            ID="StatusLabel"
                            runat="server" />

                        <span class="admin-ai-detail-badge admin-ai-detail-badge--source">
                            参照FAQ
                            <asp:Literal
                                ID="SourceCountLiteral"
                                runat="server" />
                            件
                        </span>

                        <span class="admin-ai-detail-badge">
                            <asp:Literal
                                ID="ExecutedAtLiteral"
                                runat="server" />
                        </span>

                    </div>

                    <%-- 質問 --%>
                    <section class="admin-ai-detail-field">

                        <h2 class="admin-ai-detail-field__title">
                            質問文
                        </h2>

<div class="admin-ai-detail-field__content"><asp:Literal ID="QuestionLiteral" runat="server" Mode="Encode" /></div>

                    </section>

                    <%-- 検索キーワード --%>
                    <asp:Panel
                        ID="SearchKeywordsSection"
                        runat="server"
                        Visible="false"
                        CssClass="admin-ai-detail-field">

                        <h2 class="admin-ai-detail-field__title">
                            検索キーワード
                        </h2>

<div class="admin-ai-detail-field__content"><asp:Literal ID="SearchKeywordsLiteral" runat="server" Mode="Encode" /></div>

                    </asp:Panel>

                    <%-- AI回答 --%>
                    <section class="admin-ai-detail-field">

                        <h2 class="admin-ai-detail-field__title">
                            AI回答
                        </h2>

<asp:Panel
    ID="AnswerPanel"
    runat="server"
    CssClass="admin-ai-detail-field__content"><asp:Literal ID="AnswerLiteral" runat="server" Mode="Encode" /></asp:Panel>

                        <asp:Panel
                            ID="AnswerEmptyPanel"
                            runat="server"
                            Visible="false"
                            CssClass="admin-ai-detail-field__content admin-ai-detail-field__content--empty">

                            AI回答は保存されていません。

                        </asp:Panel>

                    </section>

                    <%-- エラー内容 --%>
                    <asp:Panel
                        ID="ErrorSection"
                        runat="server"
                        Visible="false"
                        CssClass="admin-ai-detail-field">

                        <h2 class="admin-ai-detail-field__title admin-ai-detail-field__title--error">
                            エラー内容
                        </h2>

<div class="admin-ai-detail-field__content admin-ai-detail-field__content--error"><asp:Literal ID="ErrorMessageLiteral" runat="server" Mode="Encode" /></div>

                    </asp:Panel>

                    <%-- フィードバック --%>
                    <section class="admin-ai-detail-field">

                        <h2 class="admin-ai-detail-field__title">
                            フィードバック
                        </h2>

<div class="admin-ai-detail-field__content"><asp:Literal ID="FeedbackLiteral" runat="server" /></div>

                    </section>

                </div>

            </section>

            <%-- 参照元FAQ --%>
            <section class="admin-ai-detail-card">

                <div class="admin-ai-detail-card__accent
                            admin-ai-detail-card__accent--sources">
                </div>

                <div class="admin-ai-detail-card__body">

                    <div class="admin-ai-detail-section-heading">

                        <h2 class="admin-ai-detail-section-heading__title">
                            参照元FAQ
                        </h2>

                        <p class="admin-ai-detail-section-heading__description">
                            AI回答生成時に参照したFAQのスナップショットです。
                        </p>

                    </div>

                    <asp:Panel
                        ID="SourcesEmptyPanel"
                        runat="server"
                        Visible="false"
                        CssClass="admin-ai-detail-empty">

                        参照元FAQはありません。

                    </asp:Panel>

                    <asp:Repeater
                        ID="SourceRepeater"
                        runat="server">

                        <HeaderTemplate>
                            <div class="admin-ai-detail-source-list">
                        </HeaderTemplate>

                        <ItemTemplate>

                            <a
                                class="admin-ai-detail-source"
                                href='<%# ResolveUrl(
                                    "~/Faqs/Detail.aspx?id=" +
                                    Eval("FaqId")) %>'>

                                <div class="admin-ai-detail-source__content">

                                    <div class="admin-ai-detail-source__badges">

                                        <span class="admin-ai-detail-source__order">
                                            #<%#: Eval("DisplayOrder") %>
                                        </span>

                                        <span class="admin-ai-detail-source__id">
                                            FAQ ID:
                                            <%#: Eval("FaqId") %>
                                        </span>

                                        <span class="admin-ai-detail-source__category">
                                            <%#: Eval("CategoryName") %>
                                        </span>

                                    </div>

                                    <div class="admin-ai-detail-source__question">
                                        <%#: Eval("FaqQuestion") %>
                                    </div>

                                    <div class="admin-ai-detail-source__answer">
                                        <%#: Eval("AnswerPreview") %>
                                    </div>

                                </div>

                                <span class="admin-ai-detail-source__link">
                                    FAQを見る →
                                </span>

                            </a>

                        </ItemTemplate>

                        <FooterTemplate>
                            </div>
                        </FooterTemplate>

                    </asp:Repeater>

                </div>

            </section>

        </asp:Panel>

    </main>

</asp:Content>