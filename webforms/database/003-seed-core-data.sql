SET NAMES utf8mb4;

USE faq_knowledge_search_webforms;

START TRANSACTION;

-- =========================================================
-- カテゴリ
-- ASP.NET Core版 PortfolioFaqSeeder のカテゴリ構成に合わせる
-- =========================================================
INSERT INTO categories
(
    name,
    display_order,
    is_active
)
VALUES
    ('CSV取込',       10, 1),
    ('ログイン障害',  20, 1),
    ('ユーザー権限',  30, 1),
    ('PDF出力',       40, 1),
    ('API障害',       50, 1),
    ('月次締め',      60, 1),
    ('請求処理',      70, 1),
    ('メール通知',    80, 1),
    ('システム設定',  90, 1)
ON DUPLICATE KEY UPDATE
    display_order = VALUES(display_order),
    is_active = VALUES(is_active);

-- =========================================================
-- FAQ投入用一時テーブル
-- question を識別キーとして、再実行時は既存データを更新する
-- =========================================================
DROP TEMPORARY TABLE IF EXISTS seed_faqs;

CREATE TEMPORARY TABLE seed_faqs
(
    category_name VARCHAR(100) NOT NULL,
    question      VARCHAR(500) NOT NULL,
    answer        LONGTEXT     NOT NULL,
    is_published  TINYINT(1)   NOT NULL,
    is_deleted    TINYINT(1)   NOT NULL,
    view_count    INT          NOT NULL,

    PRIMARY KEY (question)
)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_unicode_ci;

INSERT INTO seed_faqs
(
    category_name,
    question,
    answer,
    is_published,
    is_deleted,
    view_count
)
VALUES
(
    'CSV取込',
    'CSV取込でエラー行が発生した場合の確認手順',
    'CSV取込時にエラー行が発生した場合は、まずエラー一覧に表示された行番号とエラー内容を確認します。\n\n主な確認ポイントは以下です。\n\n1. 必須列が空になっていないか\n2. 日付形式が yyyy/MM/dd になっているか\n3. 金額列にカンマや全角文字が混在していないか\n4. 取込対象のコードがマスタに存在しているか\n\n修正後は、対象行のみを再作成するか、CSV全体を再出力して再取込してください。',
    1,
    0,
    42
),
(
    'CSV取込',
    'CSVの文字コードエラーへの対応',
    'CSVファイルを取り込んだ際に文字化けや文字コードエラーが発生する場合は、ファイルの文字コードを確認してください。\n\n推奨文字コードは UTF-8 です。\n\nExcelで保存したCSVはShift_JISになる場合があります。文字化けが発生する場合は、テキストエディタでUTF-8形式に変換してから再取込してください。',
    1,
    0,
    28
),
(
    'ログイン障害',
    'ログインできない場合の初期対応手順',
    'ログインできない場合は、以下を順番に確認してください。\n\n1. メールアドレスに誤りがないか\n2. パスワードの大文字・小文字が正しいか\n3. アカウントが無効化されていないか\n4. ブラウザのキャッシュやCookieの影響がないか\n\n複数回ログインに失敗している場合、一時的にロックされている可能性があります。',
    1,
    0,
    35
),
(
    'ユーザー権限',
    '権限エラー（403）が表示された場合の確認手順',
    '403エラーが表示される場合、ログイン自体は成功していますが、対象画面を表示する権限が不足している可能性があります。\n\n確認ポイントは以下です。\n\n1. 対象ユーザーに必要なロールが付与されているか\n2. 管理画面へのアクセスにAdmin権限が必要ではないか\n3. FAQ編集機能にEditor以上の権限が必要ではないか\n4. トークンの再ログインが必要ではないか\n\n権限変更後は、一度ログアウトして再ログインしてください。',
    1,
    0,
    31
),
(
    'PDF出力',
    'PDF出力に失敗した場合の確認手順',
    'PDF出力に失敗する場合は、出力対象データとテンプレート設定を確認してください。\n\n主な原因は以下です。\n\n1. 出力対象の請求データが存在しない\n2. 必須項目が未入力\n3. PDFテンプレートの参照先が不正\n4. サーバー側で一時ファイルを作成できない\n\n再実行しても失敗する場合は、対象データのIDとエラーメッセージを控えて管理者に連絡してください。',
    1,
    0,
    22
),
(
    'API障害',
    '外部APIが応答しない場合の初期対応',
    '外部APIが応答しない場合は、まず一時的な通信遅延か継続的な障害かを確認します。\n\n確認ポイントは以下です。\n\n1. APIのヘルスチェックが成功するか\n2. タイムアウトが発生しているか\n3. 認証トークンの期限が切れていないか\n4. 同時間帯に他のAPIも失敗していないか\n\n一時的な失敗の場合は、時間を置いて再実行してください。継続する場合はログを確認します。',
    1,
    0,
    19
),
(
    '月次締め',
    '月次締め処理の実行手順',
    '月次締め処理は、対象月の請求データと入金データを確認してから実行します。\n\n実行前の確認項目は以下です。\n\n1. 対象月の請求データが登録済みであること\n2. 未確定の入金データが残っていないこと\n3. CSV取込エラーが解消済みであること\n4. 締め処理の実行権限があること\n\n締め処理後は、対象月のデータがロックされるため、実行前に内容を確認してください。',
    1,
    0,
    16
),
(
    '請求処理',
    '請求書CSVのインポートエラー対応',
    '請求書CSVのインポート時にエラーが発生した場合は、CSVの列構成と請求先コードを確認してください。\n\n特に以下の項目を確認します。\n\n1. 請求先コードがマスタに登録されているか\n2. 請求金額が数値で入力されているか\n3. 請求日が正しい日付形式か\n4. ヘッダー行がテンプレートと一致しているか\n\nテンプレートを変更している場合は、最新フォーマットをダウンロードして再作成してください。',
    1,
    0,
    24
),
(
    'メール通知',
    'メール通知が届かない場合の確認手順',
    'メール通知が届かない場合は、通知設定とメール送信ログを確認してください。\n\n確認ポイントは以下です。\n\n1. ユーザーのメールアドレスが正しいか\n2. 通知設定が有効になっているか\n3. 迷惑メールフォルダに入っていないか\n4. SMTPサーバーで送信エラーが発生していないか\n\n管理者は送信ログを確認し、エラーコードが出ている場合はSMTP設定を見直してください。',
    1,
    0,
    14
),
(
    'システム設定',
    '検索結果に表示されないFAQがある場合の確認',
    '登録済みFAQが検索結果に表示されない場合は、公開状態と検索キーワードを確認してください。\n\n確認ポイントは以下です。\n\n1. FAQが公開状態になっているか\n2. 論理削除されていないか\n3. タイトル・本文・タグに検索キーワードが含まれているか\n4. 管理者画面では表示されるか\n\n非公開FAQは一般利用者の検索結果には表示されません。',
    1,
    0,
    11
);

-- 既存の同名FAQをASP.NET Core版のSeed内容に同期する
UPDATE faqs f
INNER JOIN seed_faqs s
    ON s.question = f.question
INNER JOIN categories c
    ON c.name = s.category_name
SET
    f.category_id  = c.id,
    f.answer       = s.answer,
    f.is_published = s.is_published,
    f.is_deleted   = s.is_deleted,
    f.view_count   = s.view_count,
    f.updated_at   = CURRENT_TIMESTAMP;

-- 未登録FAQのみ追加する
INSERT INTO faqs
(
    category_id,
    question,
    answer,
    is_published,
    is_deleted,
    view_count
)
SELECT
    c.id,
    s.question,
    s.answer,
    s.is_published,
    s.is_deleted,
    s.view_count
FROM seed_faqs s
INNER JOIN categories c
    ON c.name = s.category_name
LEFT JOIN faqs f
    ON f.question = s.question
WHERE f.id IS NULL;

COMMIT;
