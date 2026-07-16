USE faq_knowledge_search_webforms;

CREATE TABLE IF NOT EXISTS categories
(
    id            BIGINT       NOT NULL AUTO_INCREMENT,
    name          VARCHAR(100) NOT NULL,
    display_order INT          NOT NULL DEFAULT 0,
    is_active     TINYINT(1)   NOT NULL DEFAULT 1,
    created_at    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
                                  ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT pk_categories PRIMARY KEY (id),
    CONSTRAINT uq_categories_name UNIQUE (name)
)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS faqs
(
    id            BIGINT       NOT NULL AUTO_INCREMENT,
    category_id   BIGINT       NOT NULL,
    question      VARCHAR(500) NOT NULL,
    answer        LONGTEXT     NOT NULL,
    is_published  TINYINT(1)   NOT NULL DEFAULT 0,
    is_deleted    TINYINT(1)   NOT NULL DEFAULT 0,
    view_count    INT          NOT NULL DEFAULT 0,
    created_at    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
                                  ON UPDATE CURRENT_TIMESTAMP,

    CONSTRAINT pk_faqs PRIMARY KEY (id),

    CONSTRAINT fk_faqs_categories
        FOREIGN KEY (category_id)
        REFERENCES categories (id)
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    INDEX ix_faqs_category_id (category_id),
    INDEX ix_faqs_publication (is_published, is_deleted),
    INDEX ix_faqs_updated_at (updated_at)
)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_unicode_ci;