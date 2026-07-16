SET NAMES utf8mb4;

USE faq_knowledge_search_webforms;

START TRANSACTION;

-- =========================================================
-- タグ
-- ASP.NET Core版 PortfolioFaqSeeder のタグ構成に合わせる
-- =========================================================
INSERT INTO tags
(
    name,
    display_order
)
VALUES
    ('CSV',          10),
    ('エラー対応',   20),
    ('取込',         30),
    ('文字コード',   40),
    ('ログイン',     50),
    ('認証',         60),
    ('パスワード',   70),
    ('権限',         80),
    ('403',          90),
    ('PDF',         100),
    ('API',         110),
    ('タイムアウト', 120),
    ('月次',        130),
    ('請求',        140),
    ('メール',      150),
    ('SMTP',        160),
    ('検索',        170),
    ('公開設定',    180)
ON DUPLICATE KEY UPDATE
    display_order = VALUES(display_order);

-- =========================================================
-- FAQ・タグ関連投入用一時テーブル
-- =========================================================
DROP TEMPORARY TABLE IF EXISTS seed_faq_tags;

CREATE TEMPORARY TABLE seed_faq_tags
(
    question VARCHAR(500) NOT NULL,
    tag_name VARCHAR(50)  NOT NULL,

    PRIMARY KEY (question, tag_name)
)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_unicode_ci;

INSERT INTO seed_faq_tags
(
    question,
    tag_name
)
VALUES
    ('CSV取込でエラー行が発生した場合の確認手順', 'CSV'),
    ('CSV取込でエラー行が発生した場合の確認手順', 'エラー対応'),
    ('CSV取込でエラー行が発生した場合の確認手順', '取込'),

    ('CSVの文字コードエラーへの対応', 'CSV'),
    ('CSVの文字コードエラーへの対応', '文字コード'),
    ('CSVの文字コードエラーへの対応', 'エラー対応'),

    ('ログインできない場合の初期対応手順', 'ログイン'),
    ('ログインできない場合の初期対応手順', '認証'),
    ('ログインできない場合の初期対応手順', 'パスワード'),

    ('権限エラー（403）が表示された場合の確認手順', '権限'),
    ('権限エラー（403）が表示された場合の確認手順', '403'),
    ('権限エラー（403）が表示された場合の確認手順', 'エラー対応'),

    ('PDF出力に失敗した場合の確認手順', 'PDF'),
    ('PDF出力に失敗した場合の確認手順', 'エラー対応'),

    ('外部APIが応答しない場合の初期対応', 'API'),
    ('外部APIが応答しない場合の初期対応', 'タイムアウト'),
    ('外部APIが応答しない場合の初期対応', 'エラー対応'),

    ('月次締め処理の実行手順', '月次'),
    ('月次締め処理の実行手順', '請求'),

    ('請求書CSVのインポートエラー対応', '請求'),
    ('請求書CSVのインポートエラー対応', 'CSV'),
    ('請求書CSVのインポートエラー対応', '取込'),
    ('請求書CSVのインポートエラー対応', 'エラー対応'),

    ('メール通知が届かない場合の確認手順', 'メール'),
    ('メール通知が届かない場合の確認手順', 'SMTP'),
    ('メール通知が届かない場合の確認手順', 'エラー対応'),

    ('検索結果に表示されないFAQがある場合の確認', '検索'),
    ('検索結果に表示されないFAQがある場合の確認', '公開設定');

-- Seed対象FAQのタグ構成を毎回同じ状態へ同期する
DELETE ft
FROM faq_tags ft
INNER JOIN faqs f
    ON f.id = ft.faq_id
WHERE EXISTS
(
    SELECT 1
    FROM seed_faq_tags s
    WHERE s.question = f.question
);

INSERT INTO faq_tags
(
    faq_id,
    tag_id
)
SELECT
    f.id,
    t.id
FROM seed_faq_tags s
INNER JOIN faqs f
    ON f.question = s.question
INNER JOIN tags t
    ON t.name = s.tag_name;

COMMIT;
