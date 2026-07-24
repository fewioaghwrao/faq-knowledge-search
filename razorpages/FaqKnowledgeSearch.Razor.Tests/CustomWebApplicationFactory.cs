using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace FaqKnowledgeSearch.Razor.Tests;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration(
            (_, configuration) =>
            {
                var settings =
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] =
                            "Server=127.0.0.1;"
                            + "Port=3306;"
                            + "Database=faq_knowledge_search_test;"
                            + "User=test;"
                            + "Password=test;"
                    };

                configuration.AddInMemoryCollection(
                    settings);
            });
    }
}