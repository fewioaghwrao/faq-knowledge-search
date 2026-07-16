<%@ Page
    Title="FAQ詳細"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Detail.aspx.cs"
    Inherits="FaqKnowledgeSearch.WebForms.Faqs.Detail" %>

<asp:Content
    ID="BodyContent"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <section class="faq-detail-page">

        <%-- エラー表示 --%>
        <asp:Panel
            ID="ErrorPanel"
            runat="server"
            Visible="false"
            CssClass="faq-detail-error">

            <asp:Literal
                ID="ErrorMessageLiteral"
                runat="server"
                Mode="Encode" />
        </asp:Panel>

        <%-- FAQ詳細 --%>
        <asp:Panel
            ID="FaqPanel"
            runat="server"
            Visible="false"
            CssClass="faq-detail-card">

            <asp:HyperLink
                ID="BackLink"
                runat="server"
                NavigateUrl="~/Faqs/Index"
                CssClass="faq-detail-back-link"
                Text="← FAQ一覧へ戻る" />

<div class="faq-detail-tags">
    <span class="faq-detail-tag faq-detail-tag--category">
        <asp:Literal
            ID="CategoryLiteral"
            runat="server"
            Mode="Encode" />
    </span>

    <asp:Repeater
        ID="TagRepeater"
        runat="server">

        <ItemTemplate>
            <span class="
                faq-detail-tag
                faq-detail-tag--keyword
                faq-keyword-tag">
                #<%#: Container.DataItem %>
            </span>
        </ItemTemplate>
    </asp:Repeater>
</div>

            <h1 class="faq-detail-title">
                <asp:Literal
                    ID="QuestionLiteral"
                    runat="server"
                    Mode="Encode" />
            </h1>

            <div class="faq-detail-meta">
                <span>
                    閲覧数:
                    <strong>
                        <asp:Literal
                            ID="ViewCountLiteral"
                            runat="server" />
                    </strong>
                </span>

                <span class="faq-detail-meta__updated">
                    更新日時:
                    <asp:Literal
                        ID="UpdatedAtLiteral"
                        runat="server" />
                </span>
            </div>

            <div class="faq-detail-answer">
                <asp:Literal
                    ID="AnswerLiteral"
                    runat="server"
                    Mode="Encode" />
            </div>

        </asp:Panel>
    </section>

</asp:Content>