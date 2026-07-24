using FaqKnowledgeSearch.Domain.Faqs;
using Microsoft.EntityFrameworkCore;

namespace FaqKnowledgeSearch.Infrastructure.Persistence.Seed;

public static class AiFaqDemoDataSeeder
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
    }

    private static async Task SeedCategoriesAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var seeds = new[]
        {
            new CategorySeed(
                "データ取込・連携",
                4),

            new CategorySeed(
                "帳票・出力",
                5),

            new CategorySeed(
                "権限・マスタ管理",
                6),

            new CategorySeed(
                "通知・メール",
                7)
        };

        var seedNames = seeds
            .Select(seed => seed.Name)
            .ToArray();

        var existingNames =
            await dbContext.Categories
                .Where(category =>
                    seedNames.Contains(category.Name))
                .Select(category => category.Name)
                .ToListAsync(cancellationToken);

        var existingNameSet =
            existingNames.ToHashSet();

        foreach (var seed in seeds)
        {
            if (existingNameSet.Contains(seed.Name))
            {
                continue;
            }

            dbContext.Categories.Add(
                new Category(
                    seed.Name,
                    seed.DisplayOrder));
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedTagsAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var seeds = new[]
        {
            new TagSeed("CSV", 9),
            new TagSeed("文字コード", 10),
            new TagSeed("必須項目", 11),
            new TagSeed("ファイル取込", 12),
            new TagSeed("PDF", 13),
            new TagSeed("帳票", 14),
            new TagSeed("出力", 15),
            new TagSeed("権限", 16),
            new TagSeed("マスタ", 17),
            new TagSeed("タイムアウト", 18),
            new TagSeed("ブラウザー", 19),
            new TagSeed("キャッシュ", 20),
            new TagSeed("メール", 21),
            new TagSeed("通知", 22),
            new TagSeed("VPN", 23)
        };

        var seedNames = seeds
            .Select(seed => seed.Name)
            .ToArray();

        var existingNames =
            await dbContext.Tags
                .Where(tag =>
                    seedNames.Contains(tag.Name))
                .Select(tag => tag.Name)
                .ToListAsync(cancellationToken);

        var existingNameSet =
            existingNames.ToHashSet();

        foreach (var seed in seeds)
        {
            if (existingNameSet.Contains(seed.Name))
            {
                continue;
            }

            dbContext.Tags.Add(
                new Tag(
                    seed.Name,
                    seed.DisplayOrder));
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task SeedFaqsAsync(
        AppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var categoryNames = new[]
        {
            "アカウント・ログイン",
            "システム・トラブル",
            "データ取込・連携",
            "帳票・出力",
            "権限・マスタ管理",
            "通知・メール"
        };

        var tagNames = new[]
        {
            "ログイン",
            "パスワード",
            "アカウント",
            "エラー",
            "ネットワーク",
            "初期対応",
            "CSV",
            "文字コード",
            "必須項目",
            "ファイル取込",
            "PDF",
            "帳票",
            "出力",
            "権限",
            "マスタ",
            "タイムアウト",
            "ブラウザー",
            "キャッシュ",
            "メール",
            "通知",
            "VPN"
        };

        var categories =
            await dbContext.Categories
                .Where(category =>
                    categoryNames.Contains(category.Name))
                .ToDictionaryAsync(
                    category => category.Name,
                    cancellationToken);

        var tags =
            await dbContext.Tags
                .Where(tag =>
                    tagNames.Contains(tag.Name))
                .ToDictionaryAsync(
                    tag => tag.Name,
                    cancellationToken);

        var seeds = CreateFaqSeeds();

        var seedTitles = seeds
            .Select(seed => seed.Title)
            .ToArray();

        // 論理削除されたSeed FAQも存在済みとして扱う
        var existingTitles =
            await dbContext.Faqs
                .IgnoreQueryFilters()
                .Where(faq =>
                    seedTitles.Contains(faq.Title))
                .Select(faq => faq.Title)
                .ToListAsync(cancellationToken);

        var existingTitleSet =
            existingTitles.ToHashSet();

        foreach (var seed in seeds)
        {
            if (existingTitleSet.Contains(seed.Title))
            {
                continue;
            }

            var category =
                categories[seed.CategoryName];

            var faq = new Faq(
                seed.Title,
                seed.Body,
                category.Id,
                isPublished: true);

            var faqTags = seed.TagNames
                .Select(tagName => tags[tagName])
                .ToArray();

            faq.ReplaceTags(faqTags);

            for (var count = 0;
                 count < seed.InitialViewCount;
                 count++)
            {
                faq.IncrementViewCount();
            }

            dbContext.Faqs.Add(faq);
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static FaqSeed[] CreateFaqSeeds()
    {
        return
        [
            new FaqSeed(
                Title:
                    "ログインできない場合の初期対応を教えてください",
                Body:
                    """
                    ログインできない場合は、最初に入力したメールアドレスと
                    パスワードに誤りがないか確認してください。

                    次に、Caps Lockが有効になっていないことと、
                    ブラウザーへ古いパスワードが自動入力されていないことを
                    確認してください。

                    複数回ログインに失敗している場合は、
                    アカウントが一時的にロックされている可能性があります。
                    15分以上経過してから再度ログインしてください。

                    改善しない場合は、表示されたメッセージと発生時刻を添えて
                    システム管理者へ問い合わせてください。
                    """,
                CategoryName:
                    "アカウント・ログイン",
                InitialViewCount: 61,
                TagNames:
                [
                    "ログイン",
                    "パスワード",
                    "アカウント",
                    "初期対応"
                ]),

            new FaqSeed(
                Title:
                    "CSV取込で文字コードエラーが発生する場合の確認方法",
                Body:
                    """
                    CSV取込時に文字コードエラーが表示された場合は、
                    対象ファイルの文字コードを確認してください。

                    システムへ取り込める文字コードはUTF-8です。
                    Excelから保存したCSVがShift_JISになっている場合は、
                    テキストエディターなどでUTF-8へ変換してください。

                    ファイルを再保存した後、CSVを閉じた状態で
                    再度取込処理を実行してください。
                    """,
                CategoryName:
                    "データ取込・連携",
                InitialViewCount: 55,
                TagNames:
                [
                    "CSV",
                    "文字コード",
                    "ファイル取込",
                    "エラー"
                ]),

            new FaqSeed(
                Title:
                    "CSV取込で必須項目エラーが発生する場合の確認方法",
                Body:
                    """
                    必須項目エラーが発生した場合は、
                    エラーメッセージに表示された行番号と項目名を確認してください。

                    対象行について、社員番号、対象日、区分などの
                    必須列が空欄になっていないか確認します。

                    Excel上では値があるように見えても、
                    数式の結果が空文字になっている場合があります。
                    必要に応じて値として貼り付けてから再度取り込んでください。
                    """,
                CategoryName:
                    "データ取込・連携",
                InitialViewCount: 43,
                TagNames:
                [
                    "CSV",
                    "必須項目",
                    "ファイル取込",
                    "エラー"
                ]),

            new FaqSeed(
                Title:
                    "CSV取込結果が0件になる場合の確認方法",
                Body:
                    """
                    CSV取込結果が0件になる場合は、
                    ヘッダー行の名称と列順を確認してください。

                    システム指定のテンプレートから列名を変更した場合や、
                    対象データがすべて取込済みの場合は、
                    新規登録件数が0件になることがあります。

                    取込結果の警告メッセージと処理ログも確認してください。
                    """,
                CategoryName:
                    "データ取込・連携",
                InitialViewCount: 37,
                TagNames:
                [
                    "CSV",
                    "ファイル取込",
                    "初期対応"
                ]),

            new FaqSeed(
                Title:
                    "PDF出力に失敗した場合の対応を教えてください",
                Body:
                    """
                    PDF出力に失敗した場合は、最初に対象データが
                    正常に保存されていることを確認してください。

                    次にブラウザーのポップアップブロックと、
                    ダウンロード制限を確認してください。

                    大量データを一度に出力している場合は、
                    対象期間を短くして再度実行してください。

                    改善しない場合は、対象帳票名、検索条件、
                    発生時刻をシステム管理者へ連絡してください。
                    """,
                CategoryName:
                    "帳票・出力",
                InitialViewCount: 59,
                TagNames:
                [
                    "PDF",
                    "帳票",
                    "出力",
                    "エラー",
                    "初期対応"
                ]),

            new FaqSeed(
                Title:
                    "PDFに日本語が正しく表示されない場合の確認方法",
Body:
    """
    PDF内の日本語が文字化けする場合は、
    出力元データに環境依存文字や特殊記号が
    含まれていないか確認してください。

    旧字体、丸数字、特殊な記号を一般的な文字へ置き換え、
    再度PDFを出力してください。

    すべてのPDFで発生する場合は、
    PDF生成用フォントの設定不備が考えられるため、
    システム管理者へ問い合わせてください。
    """,
                CategoryName:
                    "帳票・出力",
                InitialViewCount: 34,
                TagNames:
                [
                    "PDF",
                    "帳票",
                    "出力",
                    "文字コード"
                ]),

            new FaqSeed(
                Title:
                    "帳票出力がタイムアウトする場合の対応",
                Body:
                    """
                    帳票出力がタイムアウトする場合は、
                    一度に出力する対象件数を減らしてください。

                    対象期間を月単位から週単位へ変更するなど、
                    検索条件を分割して出力します。

                    同じ条件で繰り返し失敗する場合は、
                    発生時刻と対象件数をシステム管理者へ連絡してください。
                    """,
                CategoryName:
                    "帳票・出力",
                InitialViewCount: 29,
                TagNames:
                [
                    "帳票",
                    "出力",
                    "タイムアウト",
                    "初期対応"
                ]),

            new FaqSeed(
                Title:
                    "権限不足でメニューが表示されない場合",
                Body:
                    """
                    利用する機能のメニューが表示されない場合は、
                    ログインユーザーへ必要な権限が付与されているか確認してください。

                    権限変更直後は、一度ログアウトしてから
                    再度ログインする必要があります。

                    再ログインしても表示されない場合は、
                    対象ユーザーと必要な機能名を管理者へ連絡してください。
                    """,
                CategoryName:
                    "権限・マスタ管理",
                InitialViewCount: 48,
                TagNames:
                [
                    "権限",
                    "アカウント",
                    "ログイン",
                    "初期対応"
                ]),

            new FaqSeed(
                Title:
                    "マスタ更新後に画面へ反映されない場合",
                Body:
                    """
                    マスタを更新しても画面へ反映されない場合は、
                    更新処理が正常終了していることを確認してください。

                    次にブラウザーを再読み込みしてください。
                    改善しない場合は、ブラウザーのキャッシュを削除して
                    再度画面を開いてください。

                    それでも反映されない場合は、
                    更新対象のマスタ名と更新時刻を管理者へ連絡してください。
                    """,
                CategoryName:
                    "権限・マスタ管理",
                InitialViewCount: 33,
                TagNames:
                [
                    "マスタ",
                    "ブラウザー",
                    "キャッシュ",
                    "初期対応"
                ]),

            new FaqSeed(
                Title:
                    "メール通知が届かない場合の確認手順",
Body:
    """
    メール通知が届かない場合は、
    登録されているメールアドレスが正しいことを確認してください。

    次に迷惑メールフォルダーと、
    メール受信ルールを確認してください。

    通知設定が無効になっている場合は、
    ユーザー設定画面から通知を有効にしてください。

    改善しない場合は、通知の種類と発生時刻を
    システム管理者へ連絡してください。
    """,
                CategoryName:
                    "通知・メール",
                InitialViewCount: 46,
                TagNames:
                [
                    "メール",
                    "通知",
                    "アカウント",
                    "初期対応"
                ]),

            new FaqSeed(
                Title:
                    "ブラウザーで画面表示が崩れる場合の対応",
                Body:
                    """
                    画面表示が崩れる場合は、
                    ブラウザーの表示倍率を100パーセントへ戻してください。

                    次に強制再読み込みを実行し、
                    古いキャッシュを使用せず画面を読み込み直してください。

                    改善しない場合は別の対応ブラウザーで確認し、
                    利用ブラウザー名とバージョンを管理者へ連絡してください。
                    """,
                CategoryName:
                    "システム・トラブル",
                InitialViewCount: 38,
                TagNames:
                [
                    "ブラウザー",
                    "キャッシュ",
                    "エラー",
                    "初期対応"
                ]),

            new FaqSeed(
                Title:
                    "VPN接続後も社内システムに接続できない場合",
                Body:
                    """
                    VPN接続後も社内システムへ接続できない場合は、
                    VPNクライアントが接続済みになっていることを確認してください。

                    次に一度VPNを切断し、
                    ネットワーク接続を確認してから再接続してください。

                    特定の社内システムだけ接続できない場合は、
                    対象URL、発生時刻、表示されたエラーを
                    システム管理者へ連絡してください。
                    """,
                CategoryName:
                    "システム・トラブル",
                InitialViewCount: 44,
                TagNames:
                [
                    "VPN",
                    "ネットワーク",
                    "エラー",
                    "初期対応"
                ])
        ];
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
        int InitialViewCount,
        string[] TagNames);
}