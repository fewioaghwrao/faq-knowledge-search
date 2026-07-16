SET NAMES utf8mb4;

USE faq_knowledge_search_webforms;

-- =========================================================
-- AI検索フィードバック
-- 1件のAI検索履歴に対して1件の評価を保存する
-- =========================================================
CREATE TABLE IF NOT EXISTS ai_search_feedbacks
(
    id BIGINT NOT NULL AUTO_INCREMENT,

    ai_search_history_id BIGINT NOT NULL,

    -- 1: 役に立った
    -- 0: 役に立たなかった
    is_helpful TINYINT(1) NOT NULL,

    comment VARCHAR(1000) NULL,

    created_at DATETIME NOT NULL,

    updated_at DATETIME NOT NULL,

    PRIMARY KEY (id),

    -- 同一履歴に複数の評価を作らない
    CONSTRAINT uq_ai_search_feedbacks_history
        UNIQUE (ai_search_history_id),

    INDEX ix_ai_search_feedbacks_helpful
        (is_helpful),

    INDEX ix_ai_search_feedbacks_created_at
        (created_at),

    CONSTRAINT fk_ai_search_feedbacks_history
        FOREIGN KEY (ai_search_history_id)
        REFERENCES ai_search_histories(id)
        ON DELETE CASCADE
        ON UPDATE RESTRICT
)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_unicode_ci;