<%@ Page
    Title="管理トップ"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Default.aspx.cs"
    Inherits="FaqKnowledgeSearch.WebForms.Admin.Default" %>

<%@ Register
    Src="~/Admin/AdminHeader.ascx"
    TagPrefix="admin"
    TagName="AdminHeader" %>

<asp:Content
    ID="BodyContent"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <main class="admin-page">

        <admin:AdminHeader
            ID="AdminHeaderControl"
            runat="server" />

        <section class="admin-dashboard-grid">

            <a
                runat="server"
                href="~/Admin/Faqs/Index.aspx"
                class="admin-dashboard-card">

                <span class="admin-dashboard-card__label">
                    FAQ Management
                </span>

                <h2 class="admin-dashboard-card__title">
                    FAQ管理
                </h2>

                <p class="admin-dashboard-card__description">
                    FAQの一覧確認、登録、編集、公開状態の変更を行います。
                </p>

                <span class="admin-dashboard-card__link">
                    FAQ管理を開く →
                </span>
            </a>

            <section class="admin-dashboard-card">
                <span class="admin-dashboard-card__label">
                    Account
                </span>

                <h2 class="admin-dashboard-card__title">
                    ログイン情報
                </h2>

                <p class="admin-dashboard-card__description">
                    現在ログインしている管理者
                </p>

                <strong class="admin-dashboard-card__user">
                    <asp:LoginName
                        ID="CurrentLoginName"
                        runat="server" />
                </strong>

                <button
                    type="button"
                    class="admin-dashboard-card__logout"
                    data-bs-toggle="modal"
                    data-bs-target="#logoutConfirmModal">
                    ログアウト
                </button>
            </section>

        </section>
    </main>

</asp:Content>