<%@ Control
    Language="C#"
    AutoEventWireup="true"
    CodeBehind="AdminHeader.ascx.cs"
    Inherits="FaqKnowledgeSearch.WebForms.Admin.AdminHeader" %>

<section class="admin-hero">
    <div class="admin-hero__content">
        <span class="admin-hero__badge">
            Admin Console
        </span>

        <h1 class="admin-hero__title">
            FAQ管理画面
        </h1>

        <p class="admin-hero__description">
            社内FAQの登録・編集・削除を行います。
        </p>
    </div>

<nav
    class="admin-hero__actions"
    aria-label="管理画面メニュー">

    <asp:HyperLink
        ID="FaqListLink"
        runat="server"
        CssClass="admin-menu-button"
        NavigateUrl="~/Admin/Faqs/Index.aspx"
        Text="FAQ管理" />

    <asp:HyperLink
        ID="NewFaqLink"
        runat="server"
        CssClass="admin-menu-button"
        NavigateUrl="~/Admin/Faqs/Edit.aspx"
        Text="新規登録" />

    <asp:HyperLink
        ID="UserListLink"
        runat="server"
        CssClass="admin-menu-button"
        NavigateUrl="~/Admin/Users/Index.aspx"
        Text="ユーザー管理" />

</nav>
</section>