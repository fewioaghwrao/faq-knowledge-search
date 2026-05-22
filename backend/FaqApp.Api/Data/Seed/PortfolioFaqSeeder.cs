using FaqApp.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FaqApp.Api.Data.Seed;

public static class PortfolioFaqSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Faqs.AnyAsync())
        {
            return;
        }

        var categories = await db.Categories.ToDictionaryAsync(x => x.Name);
        var tags = await db.Tags.ToDictionaryAsync(x => x.Name);

        var now = DateTime.UtcNow;

        var faqs = new List<Faq>
        {
            CreateFaq(
                title: "CSV取込でエラー行が発生した場合の確認手順",
                body:
@"CSV取込時にエラー行が発生した場合は、まずエラー一覧に表示された行番号とエラー内容を確認します。

主な確認ポイントは以下です。

1. 必須列が空になっていないか
2. 日付形式が yyyy/MM/dd になっているか
3. 金額列にカンマや全角文字が混在していないか
4. 取込対象のコードがマスタに存在しているか

修正後は、対象行のみを再作成するか、CSV全体を再出力して再取込してください。",
                category: categories["CSV取込"],
                tagList: new[] { tags["CSV"], tags["エラー対応"], tags["取込"] },
                viewCount: 42,
                now: now
            ),

            CreateFaq(
                title: "CSVの文字コードエラーへの対応",
                body:
@"CSVファイルを取り込んだ際に文字化けや文字コードエラーが発生する場合は、ファイルの文字コードを確認してください。

推奨文字コードは UTF-8 です。

Excelで保存したCSVはShift_JISになる場合があります。文字化けが発生する場合は、テキストエディタでUTF-8形式に変換してから再取込してください。",
                category: categories["CSV取込"],
                tagList: new[] { tags["CSV"], tags["文字コード"], tags["エラー対応"] },
                viewCount: 28,
                now: now
            ),

            CreateFaq(
                title: "ログインできない場合の初期対応手順",
                body:
@"ログインできない場合は、以下を順番に確認してください。

1. メールアドレスに誤りがないか
2. パスワードの大文字・小文字が正しいか
3. アカウントが無効化されていないか
4. ブラウザのキャッシュやCookieの影響がないか

複数回ログインに失敗している場合、一時的にロックされている可能性があります。",
                category: categories["ログイン障害"],
                tagList: new[] { tags["ログイン"], tags["認証"], tags["パスワード"] },
                viewCount: 35,
                now: now
            ),

            CreateFaq(
                title: "権限エラー（403）が表示された場合の確認手順",
                body:
@"403エラーが表示される場合、ログイン自体は成功していますが、対象画面を表示する権限が不足している可能性があります。

確認ポイントは以下です。

1. 対象ユーザーに必要なロールが付与されているか
2. 管理画面へのアクセスにAdmin権限が必要ではないか
3. FAQ編集機能にEditor以上の権限が必要ではないか
4. トークンの再ログインが必要ではないか

権限変更後は、一度ログアウトして再ログインしてください。",
                category: categories["ユーザー権限"],
                tagList: new[] { tags["権限"], tags["403"], tags["エラー対応"] },
                viewCount: 31,
                now: now
            ),

            CreateFaq(
                title: "PDF出力に失敗した場合の確認手順",
                body:
@"PDF出力に失敗する場合は、出力対象データとテンプレート設定を確認してください。

主な原因は以下です。

1. 出力対象の請求データが存在しない
2. 必須項目が未入力
3. PDFテンプレートの参照先が不正
4. サーバー側で一時ファイルを作成できない

再実行しても失敗する場合は、対象データのIDとエラーメッセージを控えて管理者に連絡してください。",
                category: categories["PDF出力"],
                tagList: new[] { tags["PDF"], tags["エラー対応"] },
                viewCount: 22,
                now: now
            ),

            CreateFaq(
                title: "外部APIが応答しない場合の初期対応",
                body:
@"外部APIが応答しない場合は、まず一時的な通信遅延か継続的な障害かを確認します。

確認ポイントは以下です。

1. APIのヘルスチェックが成功するか
2. タイムアウトが発生しているか
3. 認証トークンの期限が切れていないか
4. 同時間帯に他のAPIも失敗していないか

一時的な失敗の場合は、時間を置いて再実行してください。継続する場合はログを確認します。",
                category: categories["API障害"],
                tagList: new[] { tags["API"], tags["タイムアウト"], tags["エラー対応"] },
                viewCount: 19,
                now: now
            ),

            CreateFaq(
                title: "月次締め処理の実行手順",
                body:
@"月次締め処理は、対象月の請求データと入金データを確認してから実行します。

実行前の確認項目は以下です。

1. 対象月の請求データが登録済みであること
2. 未確定の入金データが残っていないこと
3. CSV取込エラーが解消済みであること
4. 締め処理の実行権限があること

締め処理後は、対象月のデータがロックされるため、実行前に内容を確認してください。",
                category: categories["月次締め"],
                tagList: new[] { tags["月次"], tags["請求"] },
                viewCount: 16,
                now: now
            ),

            CreateFaq(
                title: "請求書CSVのインポートエラー対応",
                body:
@"請求書CSVのインポート時にエラーが発生した場合は、CSVの列構成と請求先コードを確認してください。

特に以下の項目を確認します。

1. 請求先コードがマスタに登録されているか
2. 請求金額が数値で入力されているか
3. 請求日が正しい日付形式か
4. ヘッダー行がテンプレートと一致しているか

テンプレートを変更している場合は、最新フォーマットをダウンロードして再作成してください。",
                category: categories["請求処理"],
                tagList: new[] { tags["請求"], tags["CSV"], tags["取込"], tags["エラー対応"] },
                viewCount: 24,
                now: now
            ),

            CreateFaq(
                title: "メール通知が届かない場合の確認手順",
                body:
@"メール通知が届かない場合は、通知設定とメール送信ログを確認してください。

確認ポイントは以下です。

1. ユーザーのメールアドレスが正しいか
2. 通知設定が有効になっているか
3. 迷惑メールフォルダに入っていないか
4. SMTPサーバーで送信エラーが発生していないか

管理者は送信ログを確認し、エラーコードが出ている場合はSMTP設定を見直してください。",
                category: categories["メール通知"],
                tagList: new[] { tags["メール"], tags["SMTP"], tags["エラー対応"] },
                viewCount: 14,
                now: now
            ),

            CreateFaq(
                title: "検索結果に表示されないFAQがある場合の確認",
                body:
@"登録済みFAQが検索結果に表示されない場合は、公開状態と検索キーワードを確認してください。

確認ポイントは以下です。

1. FAQが公開状態になっているか
2. 論理削除されていないか
3. タイトル・本文・タグに検索キーワードが含まれているか
4. 管理者画面では表示されるか

非公開FAQは一般利用者の検索結果には表示されません。",
                category: categories["システム設定"],
                tagList: new[] { tags["検索"], tags["公開設定"] },
                viewCount: 11,
                now: now
            )
        };

        await db.Faqs.AddRangeAsync(faqs);
        await db.SaveChangesAsync();
    }

    private static Faq CreateFaq(
        string title,
        string body,
        Category category,
        IEnumerable<Tag> tagList,
        int viewCount,
        DateTime now)
    {
        var faq = new Faq(
            title: title,
            body: body,
            categoryId: category.Id,
            isPublished: true);

        faq.Tags.AddRange(tagList);
        faq.SetInitialViewCount(viewCount);

        return faq;
    }
}