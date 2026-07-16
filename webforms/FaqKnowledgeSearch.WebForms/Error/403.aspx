<%@ Page
    Title="アクセスできません"
    Language="C#"
    MasterPageFile="~/Error/Error.Master"
    AutoEventWireup="true"
    CodeBehind="403.aspx.cs"
    Inherits="FaqKnowledgeSearch.WebForms.ErrorPages.Forbidden" %>

<asp:Content
    ID="MainContent"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <main class="error-page">
        <section class="error-card">
            <span class="error-card__code error-card__code--warning">
                403
            </span>

            <h1 class="error-card__title">
                このページにはアクセスできません
            </h1>

            <p class="error-card__description">
                この操作を行う権限がありません。
                ログイン状態またはアカウントの権限をご確認ください。
            </p>

            <div class="error-card__actions">
                <a
                    runat="server"
                    href="~/Account/Login.aspx"
                    class="error-card__primary-button">
                    ログイン画面へ
                </a>

                <a
                    runat="server"
                    href="~/Default.aspx"
                    class="error-card__secondary-button">
                    トップページへ戻る
                </a>
            </div>
        </section>
    </main>

</asp:Content>