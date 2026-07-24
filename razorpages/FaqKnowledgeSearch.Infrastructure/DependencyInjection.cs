using FaqKnowledgeSearch.Application.Faqs.Abstractions;
using FaqKnowledgeSearch.Application.Faqs.Public;
using FaqKnowledgeSearch.Infrastructure.Persistence;
using FaqKnowledgeSearch.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FaqKnowledgeSearch.Application.Faqs.Admin;
using FaqKnowledgeSearch.Infrastructure.Persistence.Queries;
using FaqKnowledgeSearch.Application.Users.Admin;
using FaqKnowledgeSearch.Infrastructure.Identity;
using FaqKnowledgeSearch.Application.AiSearch;
using FaqKnowledgeSearch.Infrastructure.Ai;
using Microsoft.Extensions.Configuration;
using FaqKnowledgeSearch.Application.AiSearch.History;
using FaqKnowledgeSearch.Application.AiSearch.History.Admin;
using FaqKnowledgeSearch.Application.AiSearch.Feedback;

using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace FaqKnowledgeSearch.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException(
                "DB接続文字列が設定されていません。",
                nameof(connectionString));
        }

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseMySql(
                connectionString,
                new MySqlServerVersion(
                    new Version(8, 4, 0)));
        });

        services.AddScoped<
            IFaqRepository,
            FaqRepository>();

        services.AddScoped<
            IPublicFaqService,
            PublicFaqService>();

        services.AddScoped<IAdminFaqQuery, AdminFaqQuery>();

        services.AddScoped<IAdminFaqRepository, AdminFaqRepository>();
        services.AddScoped<IAdminFaqService, AdminFaqService>();
        services.AddScoped<
    IAdminUserService,
    AdminUserService>();

        return services;
    }

    public static IServiceCollection AddAiInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var maxContextFaqCount = GetPositiveInt(
            configuration,
            "AiSettings:MaxContextFaqCount",
            defaultValue: 5,
            maximum: 10);

        var timeoutSeconds = GetPositiveInt(
            configuration,
            "AiSettings:TimeoutSeconds",
            defaultValue: 30,
            maximum: 120);

        var maxOutputTokens = GetPositiveInt(
            configuration,
            "AiSettings:MaxOutputTokens",
            defaultValue: 800,
            maximum: 4_000);

        var modelName =
            configuration["AiSettings:Model"]
            ?? "gpt-5.4-mini";

        var searchOptions = new AiFaqSearchOptions
        {
            MaxContextFaqCount = maxContextFaqCount,
            ModelName = modelName
        };

        var openAiSettings = new OpenAiSettings
        {
            Provider =
                configuration["AiSettings:Provider"]
                ?? "OpenAI",

            Model = modelName,

            ApiKey =
                configuration["AiSettings:ApiKey"]
                ?? string.Empty,

            TimeoutSeconds = timeoutSeconds,
            MaxOutputTokens = maxOutputTokens
        };

        services.AddSingleton(searchOptions);
        services.AddSingleton(openAiSettings);

        services.AddScoped<
            IAiFaqCandidateQuery,
            AiFaqCandidateQuery>();

        services.AddScoped<
            IAiSearchHistoryRepository,
            AiSearchHistoryRepository>();

        services.AddScoped<
    IAdminAiSearchHistoryQuery,
    AdminAiSearchHistoryQuery>();

        services.AddScoped<
    IAiSearchFeedbackService,
    AiSearchFeedbackService>();

        services.AddScoped<
            IAiFaqSearchService,
            AiFaqSearchService>();

        services.AddHttpClient<
            IAiAnswerGenerator,
            OpenAiAnswerGenerator>(
            client =>
            {
                client.BaseAddress =
                    new Uri(
                        "https://api.openai.com/v1/");

                client.Timeout =
                    TimeSpan.FromSeconds(
                        timeoutSeconds);
            });

        return services;
    }

    private static int GetPositiveInt(
        IConfiguration configuration,
        string key,
        int defaultValue,
        int maximum)
    {
        var value = configuration.GetValue<int?>(key);

        if (value is null || value <= 0)
        {
            return defaultValue;
        }

        return Math.Min(value.Value, maximum);
    }
}