SET NAMES utf8mb4;

USE faq_knowledge_search_webforms;

-- =========================================================
-- AI検索履歴
-- 質問、AI回答、成功・失敗、エラー内容を保存する
-- executed_at はアプリケーション側で設定する
-- =========================================================
CREATE TABLE IF NOT EXISTS ai_search_histories
(
    id BIGINT NOT NULL AUTO_INCREMENT,

    question VARCHAR(500) NOT NULL,

    -- 現時点では質問文と同じ値を保存する。
    -- 将来、検索語抽出を実装した場合に分離して利用する。
    search_keywords VARCHAR(500) NULL,

    ai_answer TEXT NULL,

    is_success TINYINT(1) NOT NULL DEFAULT 0,

    -- APIキーやレスポンス全文などの機密情報は保存しない。
    error_message VARCHAR(1000) NULL,

    executed_at DATETIME NOT NULL,

    PRIMARY KEY (id),

    INDEX ix_ai_search_histories_executed_at
        (executed_at),

    INDEX ix_ai_search_histories_success_executed
        (is_success, executed_at)
)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_unicode_ci;


-- =========================================================
-- AI検索履歴の参照元FAQ
-- AI回答生成時に利用したFAQを検索実行時点の内容で保存する
-- =========================================================
CREATE TABLE IF NOT EXISTS ai_search_history_sources
(
    id BIGINT NOT NULL AUTO_INCREMENT,

    ai_search_history_id BIGINT NOT NULL,

    faq_id BIGINT NOT NULL,

    -- FAQが後から編集されても当時の内容を確認できるよう、
    -- 質問・回答・カテゴリ名をスナップショットとして保存する。
    faq_question VARCHAR(500) NOT NULL,

    faq_answer TEXT NULL,

    category_name VARCHAR(100) NULL,

    display_order INT NOT NULL,

    PRIMARY KEY (id),

    CONSTRAINT uq_ai_search_history_sources_faq
        UNIQUE
        (
            ai_search_history_id,
            faq_id
        ),

    CONSTRAINT uq_ai_search_history_sources_order
        UNIQUE
        (
            ai_search_history_id,
            display_order
        ),

    INDEX ix_ai_search_history_sources_history
        (ai_search_history_id),

    INDEX ix_ai_search_history_sources_faq
        (faq_id),

    CONSTRAINT fk_ai_search_history_sources_history
        FOREIGN KEY (ai_search_history_id)
        REFERENCES ai_search_histories(id)
        ON DELETE CASCADE
        ON UPDATE RESTRICT,

    CONSTRAINT fk_ai_search_history_sources_faq
        FOREIGN KEY (faq_id)
        REFERENCES faqs(id)
        ON DELETE RESTRICT
        ON UPDATE RESTRICT
)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_unicode_ci;