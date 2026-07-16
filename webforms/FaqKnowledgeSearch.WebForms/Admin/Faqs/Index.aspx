<%@ Page
    Title="FAQ管理"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Index.aspx.cs"
    Inherits="FaqKnowledgeSearch.WebForms.Admin.Faqs.Index" %>

<%@ Register
    Src="~/Admin/AdminHeader.ascx"
    TagPrefix="admin"
    TagName="AdminHeader" %>

<asp:Content
    ID="MainContent"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <main class="admin-page">

        <admin:AdminHeader
            ID="AdminHeaderControl"
            runat="server"
            ActiveItem="faq" />

        <section class="admin-panel admin-faq-list-panel">

            <div class="admin-list-heading">
                <div class="admin-list-heading__content">
                    <h2 class="admin-list-heading__title">
                        FAQ一覧
                    </h2>

                    <p class="admin-list-heading__description">
                        登録済みFAQの公開状態・編集・削除を管理します。
                        長いタイトルや本文でも崩れにくい一覧表示です。
                    </p>
                </div>
<div class="admin-list-heading__actions">

    <a
        runat="server"
        href="~/Admin/AiHistories/Index.aspx"
        class="admin-ai-history-button"
        title="AI検索履歴一覧を開きます">

        AI検索履歴を見る
    </a>

    <div class="admin-result-count-badge">


        <asp:Label
            ID="ResultCountLabel"
            runat="server"
            CssClass="admin-result-count-badge__value" />

    </div>

</div>

            </div>

            <asp:Panel
                ID="MessagePanel"
                runat="server"
                Visible="false"
                CssClass="admin-message"
                role="alert">

                <asp:Literal
                    ID="MessageLiteral"
                    runat="server" />
            </asp:Panel>

<section
    class="admin-filter-panel"
    aria-labelledby="adminFilterTitle">

    <div class="admin-filter-panel__header">
        <div>
            <span class="admin-filter-panel__eyebrow">
                Search &amp; Filter
            </span>

            <h3
                id="adminFilterTitle"
                class="admin-filter-panel__title">
                FAQを絞り込む
            </h3>

            <p class="admin-filter-panel__description">
                タイトル・本文・カテゴリ・公開状態から検索できます。
            </p>
        </div>

    </div>

    <div class="admin-filter-panel__body">

        <div class="admin-filter-field admin-filter-field--keyword">
            <asp:Label
                ID="KeywordLabel"
                runat="server"
                AssociatedControlID="KeywordTextBox"
                CssClass="admin-filter-field__label"
                Text="キーワード" />

            <div class="admin-filter-input-wrapper">
                <span
                    class="admin-filter-input-wrapper__icon"
                    aria-hidden="true">
                    Q
                </span>

                <asp:TextBox
                    ID="KeywordTextBox"
                    runat="server"
                    CssClass="admin-filter-field__input"
                    MaxLength="100"
                    autocomplete="off"
                    placeholder="タイトル・本文・カテゴリを検索" />
            </div>
        </div>

        <div class="admin-filter-field">
            <asp:Label
                ID="PublishStatusLabel"
                runat="server"
                AssociatedControlID="PublishStatusDropDownList"
                CssClass="admin-filter-field__label"
                Text="公開状態" />

            <asp:DropDownList
                ID="PublishStatusDropDownList"
                runat="server"
                CssClass="admin-filter-field__select">

                <asp:ListItem
                    Text="すべて"
                    Value="" />

                <asp:ListItem
                    Text="公開"
                    Value="published" />

                <asp:ListItem
                    Text="非公開"
                    Value="unpublished" />

            </asp:DropDownList>
        </div>

        <div class="admin-filter-actions">
            <asp:Button
                ID="SearchButton"
                runat="server"
                Text="検索する"
                CssClass="admin-filter-button admin-filter-button--search"
                CausesValidation="false"
                OnClick="SearchButton_Click" />

            <asp:Button
                ID="ClearButton"
                runat="server"
                Text="条件をクリア"
                CssClass="admin-filter-button admin-filter-button--clear"
                CausesValidation="false"
                OnClick="ClearButton_Click" />
        </div>

    </div>
</section>

            <div class="admin-table-wrapper">

                <asp:GridView
                    ID="FaqGridView"
                    runat="server"
                    AutoGenerateColumns="false"
                    AllowPaging="true"
                    PageSize="5"
                    CssClass="admin-table admin-faq-table"
                    GridLines="None"
                    ShowHeaderWhenEmpty="true"
                    EmptyDataText="条件に一致するFAQはありません。"
                    OnRowCommand="FaqGridView_RowCommand">

                    <PagerSettings Visible="false" />

                    <Columns>

                        <asp:TemplateField HeaderText="ID">
                            <HeaderStyle CssClass="admin-table__id-column" />
                            <ItemStyle CssClass="admin-table__id-column" />

                            <ItemTemplate>
                                <span class="admin-faq-id">
                                    #<%#: Eval("Id") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

<asp:TemplateField HeaderText="タイトル / 本文">
    <HeaderStyle CssClass="admin-table__content-column" />
    <ItemStyle CssClass="admin-table__content-column" />

    <ItemTemplate>
        <div class="admin-faq-content">
            <strong class="admin-faq-content__title">
                <%#: Eval("Question") %>
            </strong>

            <p class="admin-faq-content__preview">
                <%#: GetAnswerPreview(Eval("Answer")) %>
            </p>
        </div>
    </ItemTemplate>
</asp:TemplateField>

<asp:TemplateField HeaderText="カテゴリ">
    <HeaderStyle CssClass="admin-table__category-column" />
    <ItemStyle CssClass="admin-table__category-column" />

    <ItemTemplate>
        <span class='<%# GetCategoryCssClass(
            Convert.ToString(Eval("CategoryName"))) %>'>

            <%#: Eval("CategoryName") %>
        </span>
    </ItemTemplate>
</asp:TemplateField>

<asp:TemplateField HeaderText="公開状態">
    <HeaderStyle CssClass="admin-table__status-column" />
    <ItemStyle CssClass="admin-table__status-column" />

    <ItemTemplate>
        <span class='<%# GetPublishStatusCssClass(
            Convert.ToBoolean(Eval("IsPublished"))) %>'>

            <%#: GetPublishStatusText(
                Convert.ToBoolean(Eval("IsPublished"))) %>
        </span>
    </ItemTemplate>
</asp:TemplateField>

                        <asp:TemplateField HeaderText="閲覧数">
                            <HeaderStyle CssClass="admin-table__view-column" />
                            <ItemStyle CssClass="admin-table__view-column" />

                            <ItemTemplate>
                                <span class="admin-view-count">
                                    <%#: Eval("ViewCount") %>
                                </span>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="操作">
                            <HeaderStyle CssClass="admin-table__action-column" />
                            <ItemStyle CssClass="admin-table__action-column" />

                            <ItemTemplate>
                                <div class="admin-row-actions">

                                    <asp:HyperLink
                                        ID="EditLink"
                                        runat="server"
                                        CssClass="admin-row-button admin-row-button--edit"
                                        NavigateUrl='<%# Eval(
                                            "Id",
                                            "~/Admin/Faqs/Edit.aspx?id={0}") %>'
                                        Text="編集" />

                                    <asp:LinkButton
                                        ID="DeleteButton"
                                        runat="server"
                                        CssClass="admin-row-button admin-row-button--delete"
                                        Text="削除"
                                        CommandName="DeleteFaq"
                                        CommandArgument='<%# Eval("Id") %>'
                                        CausesValidation="false"
                                        OnClientClick="return confirm('このFAQを削除します。よろしいですか？');" />

                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                    </Columns>

                    <EmptyDataRowStyle
                        CssClass="admin-table-empty" />

                </asp:GridView>

            </div>

            <nav
                class="admin-pager"
                aria-label="FAQ一覧ページ切り替え">

                <asp:LinkButton
                    ID="PreviousPageButton"
                    runat="server"
                    CssClass="admin-pager__button"
                    CausesValidation="false"
                    OnClick="PreviousPageButton_Click">
                    前へ
                </asp:LinkButton>

                <asp:Label
                    ID="PageStatusLabel"
                    runat="server"
                    CssClass="admin-pager__status" />

                <asp:LinkButton
                    ID="NextPageButton"
                    runat="server"
                    CssClass="admin-pager__button"
                    CausesValidation="false"
                    OnClick="NextPageButton_Click">
                    次へ
                </asp:LinkButton>

            </nav>

        </section>

    </main>

</asp:Content>