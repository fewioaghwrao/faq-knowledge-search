<%@ Page
    Title="ログアウト"
    Language="C#"
    MasterPageFile="~/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Logout.aspx.cs"
    Inherits="FaqKnowledgeSearch.WebForms.Account.Logout" %>

<asp:Content
    ID="BodyContent"
    ContentPlaceHolderID="MainContent"
    runat="server">

    <main class="container py-4">
        <div class="row justify-content-center">
            <div class="col-12 col-md-7 col-lg-5">

                <h1 class="h3 mb-4">ログアウト</h1>

                <div class="card">
                    <div class="card-body">
                        <p class="mb-4">
                            管理者画面からログアウトしますか？
                        </p>

                        <div class="d-grid gap-2">
                            <asp:Button
                                ID="LogoutButton"
                                runat="server"
                                Text="ログアウトする"
                                CssClass="btn btn-danger"
                                CausesValidation="false"
                                OnClick="LogoutButton_Click" />

                            <asp:HyperLink
                                ID="CancelLink"
                                runat="server"
                                NavigateUrl="~/Admin/Default.aspx"
                                CssClass="btn btn-outline-secondary"
                                Text="管理画面に戻る" />
                        </div>
                    </div>
                </div>

            </div>
        </div>
    </main>

</asp:Content>