using FaqKnowledgeSearch.WebForms.Data.Seed;
using System;
using System.Web;
using System.Web.Optimization;
using System.Web.Routing;

namespace FaqKnowledgeSearch.WebForms
{
    public class Global : HttpApplication
    {
        protected void Application_Start(
            object sender,
            EventArgs e)
        {
            // ルーティング設定
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // CSS・JavaScriptのBundle設定
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // デモユーザー・ロールの初期登録
            DemoUserSeeder
                .SeedAsync()
                .GetAwaiter()
                .GetResult();
        }

        protected void Application_Error(
    object sender,
    EventArgs e)
        {
            Exception exception =
                Server.GetLastError();

            if (exception == null)
            {
                return;
            }

            // Serilog、NLog、log4netなどに置き換える
            System.Diagnostics.Trace.TraceError(
                exception.ToString());
        }
    }
}