<%@ Page
    Title="FAQ一覧"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Index.aspx.cs"
    Inherits="FaqKnowledgeSearch.WebForms.Faqs.Index" %>

<asp:Content
    ID="BodyContent"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <section class="faq-list-page">

        <%-- ヒーローエリア --%>
        <section class="faq-list-hero">
            <span class="faq-list-hero__badge">
                Phase 1 / FAQ Search
            </span>

            <h1 class="faq-list-hero__title">
                社内ナレッジをすばやく検索
            </h1>

            <p class="faq-list-hero__description">
                手順書・FAQ・障害対応メモを登録し、
                キーワードから必要な情報を探せる社内FAQ検索アプリです。
            </p>

            <div class="faq-list-hero__features">
                <span>FAQ検索</span>
                <span>根拠表示</span>
                <span>管理者登録</span>
            </div>
        </section>

        <%-- 検索欄 --%>
        <section class="faq-search-panel" aria-label="FAQ検索">
            <div class="faq-search-panel__input">
                <asp:TextBox
                    ID="KeywordTextBox"
                    runat="server"
                    CssClass="faq-search-input"
                    MaxLength="100"
                    placeholder="キーワードでFAQを検索" />
            </div>

            <asp:Button
                ID="SearchButton"
                runat="server"
                Text="検索"
                CssClass="faq-search-button"
                OnClick="SearchButton_Click" />

            <asp:Button
                ID="ClearButton"
                runat="server"
                Text="クリア"
                CssClass="faq-clear-button"
                CausesValidation="false"
                OnClick="ClearButton_Click" />
        </section>

        <asp:Label
            ID="MessageLabel"
            runat="server"
            EnableViewState="false"
            CssClass="faq-message" />

        <%-- 件数表示 --%>
        <div class="faq-list-heading">
            <span class="faq-list-heading__title">
                登録済みFAQ
            </span>

            <asp:Label
                ID="ResultCountLabel"
                runat="server"
                CssClass="faq-list-heading__count" />
        </div>

        <%-- 検索結果なし --%>
        <asp:Panel
            ID="EmptyPanel"
            runat="server"
            CssClass="faq-empty-panel"
            Visible="false">

            <div class="faq-empty-panel__icon">Q</div>

            <h2>該当するFAQはありません</h2>

            <p>
                検索キーワードを変更して、もう一度検索してください。
            </p>
        </asp:Panel>

        <%-- FAQカード一覧 --%>
        <div class="faq-card-list">
            <asp:Repeater
                ID="FaqRepeater"
                runat="server">

                <ItemTemplate>
                    <article class="faq-card">

                        <div class="faq-card__header">
                            <div class="faq-card__heading">

                                <span class="faq-card__category">
                                    <%#: Eval("CategoryName") %>
                                </span>

                                <h2 class="faq-card__title">
                                    <asp:HyperLink
                                        runat="server"
                                        Text='<%# Eval("Question") %>'
                                        NavigateUrl='<%# Eval(
                                            "Id",
                                            "~/Faqs/Detail.aspx?id={0}") %>' />
                                </h2>
                            </div>

                            <div class="faq-card__views">
                                <span>Views</span>

                                <strong>
                                    <%#: Eval("ViewCount") %>
                                </strong>
                            </div>
                        </div>

                        <p class="faq-card__summary">
                            <%#: GetSummary(Eval("Answer")) %>
                        </p>

                        <%-- FAQタグ --%>
<div class="faq-card__tags">
    <asp:Repeater
        ID="TagRepeater"
        runat="server"
        DataSource='<%# SplitTags(Eval("TagNames")) %>'>

        <ItemTemplate>
            <span class="faq-card__tag faq-keyword-tag">
                #<%#: Container.DataItem %>
            </span>
        </ItemTemplate>
    </asp:Repeater>
</div>


                        <footer class="faq-card__footer">
                            <span class="faq-card__updated-at">
                                更新日:
                                <%# Eval(
                                    "UpdatedAt",
                                    "{0:yyyy/MM/dd}") %>
                            </span>

                            <asp:HyperLink
                                runat="server"
                                CssClass="faq-card__detail-link"
                                Text="詳細を見る →"
                                NavigateUrl='<%# Eval(
                                    "Id",
                                    "~/Faqs/Detail.aspx?id={0}") %>' />
                        </footer>
                    </article>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <%-- ページング --%>
        <asp:Panel
            ID="PagerPanel"
            runat="server"
            CssClass="faq-pager">

            <asp:LinkButton
                ID="PreviousButton"
                runat="server"
                CssClass="faq-pager__button"
                CausesValidation="false"
                OnClick="PreviousButton_Click">
                前へ
            </asp:LinkButton>

            <asp:Label
                ID="PageStatusLabel"
                runat="server"
                CssClass="faq-pager__status" />

            <asp:LinkButton
                ID="NextButton"
                runat="server"
                CssClass="faq-pager__button"
                CausesValidation="false"
                OnClick="NextButton_Click">
                次へ
            </asp:LinkButton>
        </asp:Panel>

    </section>
</asp:Content>