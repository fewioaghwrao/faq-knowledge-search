using FaqKnowledgeSearch.Domain.Faqs;
using Microsoft.EntityFrameworkCore;

namespace FaqKnowledgeSearch.Infrastructure.Persistence.Seed;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken = default)
    {
        await SeedCategoriesAsync(
            dbContext,
            cancellationToken);

        await SeedTagsAsync(
            dbContext,
            cancellationToken);

        await SeedFaqsAsync(
            dbContext,
            cancellationToken);

        await AiFaqDemoDataSeeder.SeedAsync(
            dbContext,
            cancellationToken);
    }

    private static async Task SeedCategoriesAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var seeds = new[]
        {
            new CategorySeed("アカウント・ログイン", 1),
            new CategorySeed("勤怠・申請", 2),
            new CategorySeed("システム・トラブル", 3)
        };

        var seedNames = seeds
            .Select(x => x.Name)
            .ToArray();

        var existingNames = await dbContext.Categories
            .Where(x => seedNames.Contains(x.Name))
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        var existingNameSet = existingNames.ToHashSet();

        foreach (var seed in seeds)
        {
            if (existingNameSet.Contains(seed.Name))
            {
                continue;
            }

            dbContext.Categories.Add(
                new Category(seed.Name, seed.DisplayOrder));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedTagsAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var seeds = new[]
        {
            new TagSeed("ログイン", 1),
            new TagSeed("パスワード", 2),
            new TagSeed("アカウント", 3),
            new TagSeed("勤怠", 4),
            new TagSeed("申請", 5),
            new TagSeed("エラー", 6),
            new TagSeed("ネットワーク", 7),
            new TagSeed("初期対応", 8)
        };

        var seedNames = seeds
            .Select(x => x.Name)
            .ToArray();

        var existingNames = await dbContext.Tags
            .Where(x => seedNames.Contains(x.Name))
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        var existingNameSet = existingNames.ToHashSet();

        foreach (var seed in seeds)
        {
            if (existingNameSet.Contains(seed.Name))
            {
                continue;
            }

            dbContext.Tags.Add(
                new Tag(seed.Name, seed.DisplayOrder));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedFaqsAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var categoryNames = new[]
        {
            "アカウント・ログイン",
            "勤怠・申請",
            "システム・トラブル"
        };

        var tagNames = new[]
        {
            "ログイン",
            "パスワード",
            "アカウント",
            "勤怠",
            "申請",
            "エラー",
            "ネットワーク",
            "初期対応"
        };

        var categories = await dbContext.Categories
            .Where(x => categoryNames.Contains(x.Name))
            .ToDictionaryAsync(
                x => x.Name,
                cancellationToken);

        var tags = await dbContext.Tags
            .Where(x => tagNames.Contains(x.Name))
            .ToDictionaryAsync(
                x => x.Name,
                cancellationToken);

        var seeds = new[]
        {
            new FaqSeed(
                Title: "パスワードを忘れた場合はどうすればよいですか？",
                Body:
                    """
                    ログイン画面の「パスワードを忘れた方はこちら」から、
                    登録済みメールアドレスを入力してください。

                    パスワード再設定用のメールが送信されます。
                    メールが届かない場合は、迷惑メールフォルダーも確認してください。
                    """,
                CategoryName: "アカウント・ログイン",
                IsPublished: true,
                InitialViewCount: 35,
                TagNames: ["ログイン", "パスワード"]),

            new FaqSeed(
                Title: "アカウントがロックされた場合の解除方法を教えてください",
                Body:
                    """
                    ログインに複数回失敗すると、セキュリティ保護のため
                    アカウントが一時的にロックされます。

                    30分経過してから再度ログインするか、
                    システム管理者へアカウント解除を依頼してください。
                    """,
                CategoryName: "アカウント・ログイン",
                IsPublished: true,
                InitialViewCount: 28,
                TagNames: ["ログイン", "アカウント", "初期対応"]),

            new FaqSeed(
                Title: "有給休暇の申請方法を教えてください",
                Body:
                    """
                    勤怠管理システムの休暇申請画面を開き、
                    休暇の種類、取得日、申請理由を入力して申請してください。

                    申請後は承認状況を申請履歴画面から確認できます。
                    """,
                CategoryName: "勤怠・申請",
                IsPublished: true,
                InitialViewCount: 42,
                TagNames: ["勤怠", "申請"]),

            new FaqSeed(
                Title: "打刻を忘れた場合はどうすればよいですか？",
                Body:
                    """
                    勤怠管理システムの打刻修正画面から、
                    対象日、修正後の時刻、修正理由を入力してください。

                    入力後、所属長へ修正申請を提出してください。
                    """,
                CategoryName: "勤怠・申請",
                IsPublished: true,
                InitialViewCount: 31,
                TagNames: ["勤怠", "申請"]),

            new FaqSeed(
                Title: "社内システムに接続できない場合の確認事項は？",
                Body:
                    """
                    最初にネットワークへ接続できているか確認してください。

                    次にブラウザーを再起動し、社内システムへ再度アクセスしてください。
                    改善しない場合は、表示されたエラーメッセージを添えて
                    システム管理者へ問い合わせてください。
                    """,
                CategoryName: "システム・トラブル",
                IsPublished: true,
                InitialViewCount: 53,
                TagNames: ["エラー", "ネットワーク", "初期対応"]),

            new FaqSeed(
                Title: "VPN設定変更時の管理者向け確認手順",
                Body:
                    """
                    このFAQは管理者による内容確認中です。
                    VPN設定の変更手順が確定するまで一般公開しないでください。
                    """,
                CategoryName: "システム・トラブル",
                IsPublished: false,
                InitialViewCount: 0,
                TagNames: ["ネットワーク"])
        };

        var seedTitles = seeds
            .Select(x => x.Title)
            .ToArray();

        var existingTitles = await dbContext.Faqs
            .IgnoreQueryFilters()
            .Where(x => seedTitles.Contains(x.Title))
            .Select(x => x.Title)
            .ToListAsync(cancellationToken);

        var existingTitleSet = existingTitles.ToHashSet();

        foreach (var seed in seeds)
        {
            if (existingTitleSet.Contains(seed.Title))
            {
                continue;
            }

            var category = categories[seed.CategoryName];

            var faq = new Faq(
                seed.Title,
                seed.Body,
                category.Id,
                seed.IsPublished);

            var faqTags = seed.TagNames
                .Select(tagName => tags[tagName])
                .ToArray();

            faq.ReplaceTags(faqTags);

            for (var i = 0; i < seed.InitialViewCount; i++)
            {
                faq.IncrementViewCount();
            }

            dbContext.Faqs.Add(faq);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private sealed record CategorySeed(
        string Name,
        int DisplayOrder);

    private sealed record TagSeed(
        string Name,
        int DisplayOrder);

    private sealed record FaqSeed(
        string Title,
        string Body,
        string CategoryName,
        bool IsPublished,
        int InitialViewCount,
        string[] TagNames);
}
