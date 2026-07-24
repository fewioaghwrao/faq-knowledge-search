using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaqKnowledgeSearch.Infrastructure.Persistence.Seed;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider serviceProvider,
        bool seedDemoData,
        CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        if (seedDemoData)
        {
            await DemoDataSeeder.SeedAsync(
                dbContext,
                cancellationToken);
        }
    }
}