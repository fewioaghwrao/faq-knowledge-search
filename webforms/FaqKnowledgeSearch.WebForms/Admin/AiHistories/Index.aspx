<%@ Page
    Title="AI検索履歴"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Index.aspx.cs"
    Inherits="FaqKnowledgeSearch.WebForms.Admin.AiHistories.Index" %>

<asp:Content
    ID="BodyContent"
    ContentPlaceHolderID="MainContent"
    runat="server">

<main class="container py-4 admin-ai-history-page">

        <div class="d-flex flex-wrap
                    justify-content-between
                    align-items-center
                    gap-3
                    mb-4">

            <div>
<span class="admin-ai-history-eyebrow">
    AI Search History
</span>

                <h1 class="h2 mb-1">
                    AI検索履歴
                </h1>

                <p class="text-muted mb-0">
                    AI検索の質問、回答結果、参照FAQを確認します。
                </p>
            </div>

            <a
                runat="server"
                href="~/Admin/Default.aspx"
                class="btn btn-outline-secondary">
                管理トップへ戻る
            </a>
        </div>

        <asp:Panel
            ID="MessagePanel"
            runat="server"
            Visible="false"
            CssClass="alert alert-danger"
            role="alert">

            <asp:Literal
                ID="MessageLiteral"
                runat="server" />
        </asp:Panel>

        <%-- 検索条件 --%>
        <section class="card shadow-sm mb-4">
            <div class="card-body">

                <div class="row g-3 align-items-end">

                    <div class="col-lg-7">
                        <asp:Label
                            ID="KeywordLabel"
                            runat="server"
                            AssociatedControlID="KeywordTextBox"
                            Text="キーワード"
                            CssClass="form-label" />

                        <asp:TextBox
                            ID="KeywordTextBox"
                            runat="server"
                            MaxLength="500"
                            CssClass="form-control"
                            placeholder="質問、AI回答、エラー内容から検索" />
                    </div>

                    <div class="col-lg-3">
                        <asp:Label
                            ID="StatusLabel"
                            runat="server"
                            AssociatedControlID="StatusDropDownList"
                            Text="実行結果"
                            CssClass="form-label" />

                        <asp:DropDownList
                            ID="StatusDropDownList"
                            runat="server"
                            CssClass="form-select">

                            <asp:ListItem
                                Text="すべて"
                                Value="" />

                            <asp:ListItem
                                Text="成功"
                                Value="true" />

                            <asp:ListItem
                                Text="失敗"
                                Value="false" />

                        </asp:DropDownList>
                    </div>

                    <div class="col-lg-2">
                        <div class="d-grid gap-2">

                            <asp:Button
                                ID="SearchButton"
                                runat="server"
                                Text="検索"
                                CssClass="btn btn-primary"
                                CausesValidation="false"
                                OnClick="SearchButton_Click" />

                            <asp:Button
                                ID="ResetButton"
                                runat="server"
                                Text="条件クリア"
                                CssClass="btn btn-outline-secondary"
                                CausesValidation="false"
                                OnClick="ResetButton_Click" />

                        </div>
                    </div>

                </div>
            </div>
        </section>

        <%-- 検索結果件数 --%>
        <div class="d-flex
                    justify-content-between
                    align-items-center
                    mb-3">

            <h2 class="h5 mb-0">
                履歴一覧
            </h2>

            <span class="text-muted">
                <asp:Literal
                    ID="ResultCountLiteral"
                    runat="server" />
            </span>
        </div>

        <%-- 履歴一覧 --%>
        <div class="card shadow-sm">
            <div class="card-body p-0">

                <div class="table-responsive">

                    <asp:GridView
                        ID="HistoryGrid"
                        runat="server"
                        AutoGenerateColumns="false"
                        AllowPaging="true"
                        PageSize="20"
                        GridLines="None"
                        CssClass="table table-hover align-middle mb-0"
                        EmptyDataText="条件に一致するAI検索履歴はありません。"
                        OnPageIndexChanging="HistoryGrid_PageIndexChanging">

                        <Columns>

                            <asp:TemplateField
                                HeaderText="実行日時">

                                <ItemTemplate>
                                    <span class="text-nowrap">
                                        <%#: FormatExecutedAt(
                                            Eval("ExecutedAt")) %>
                                    </span>
                                </ItemTemplate>

                                <ItemStyle Width="160px" />
                            </asp:TemplateField>

                            <asp:TemplateField
                                HeaderText="結果">

                                <ItemTemplate>
                                    <span
                                        class="<%# GetStatusCss(
                                            Eval("IsSuccess")) %>">

                                        <%# GetStatusText(
                                            Eval("IsSuccess")) %>
                                    </span>
                                </ItemTemplate>

                                <ItemStyle Width="90px" />
                            </asp:TemplateField>

                            <asp:TemplateField
                                HeaderText="質問">

                                <ItemTemplate>
                                    <div class="fw-semibold">
                                        <%#: Eval("Question") %>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField
                                HeaderText="回答・エラー">

                                <ItemTemplate>
                                    <span class="text-muted">
                                        <%#: GetResultPreview(
                                            Eval("AnswerPreview"),
                                            Eval("ErrorMessage")) %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField
                                HeaderText="参照FAQ">

                                <ItemTemplate>
                                    <span class="badge text-bg-secondary">
                                        <%#: Eval("SourceCount") %> 件
                                    </span>
                                </ItemTemplate>

                                <ItemStyle
                                    Width="100px"
                                    HorizontalAlign="Center" />
                            </asp:TemplateField>
                            <asp:TemplateField
    HeaderText="FB">

    <ItemTemplate>
        <span
            class="<%# GetFeedbackCss(
                Eval("IsHelpful")) %>"
            title="<%#: GetFeedbackTitle(
                Eval("IsHelpful")) %>">

            <%#: GetFeedbackText(
                Eval("IsHelpful")) %>
        </span>
    </ItemTemplate>

    <ItemStyle
        Width="70px"
        HorizontalAlign="Center" />
</asp:TemplateField>
                            <asp:TemplateField
                                HeaderText="操作">

                                <ItemTemplate>
                                    <a
                                        class="btn btn-sm btn-outline-primary"
                                        href="<%# ResolveUrl(
                                            "~/Admin/AiHistories/Detail.aspx?id=" +
                                            Eval("Id")) %>">
                                        詳細
                                    </a>
                                </ItemTemplate>

                                <ItemStyle
                                    Width="90px"
                                    HorizontalAlign="Center" />
                            </asp:TemplateField>

                        </Columns>

                      <HeaderStyle CssClass="admin-ai-history-table-header" />

                        <PagerStyle
                            CssClass="pagination-container"
                            HorizontalAlign="Center" />

                    </asp:GridView>

                </div>
            </div>
        </div>

    </main>

</asp:Content>
