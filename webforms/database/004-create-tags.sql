SET NAMES utf8mb4;

USE faq_knowledge_search_webforms;

CREATE TABLE IF NOT EXISTS tags
(
    id            INT         NOT NULL AUTO_INCREMENT,
    name          VARCHAR(50) NOT NULL,
    display_order INT         NOT NULL DEFAULT 0,

    CONSTRAINT pk_tags
        PRIMARY KEY (id),

    CONSTRAINT uq_tags_name
        UNIQUE (name),

    CONSTRAINT chk_tags_name_not_blank
        CHECK (CHAR_LENGTH(TRIM(name)) > 0)
)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS faq_tags
(
    faq_id BIGINT NOT NULL,
    tag_id INT    NOT NULL,

    CONSTRAINT pk_faq_tags
        PRIMARY KEY (faq_id, tag_id),

    CONSTRAINT fk_faq_tags_faq
        FOREIGN KEY (faq_id)
        REFERENCES faqs (id)
        ON DELETE CASCADE,

    CONSTRAINT fk_faq_tags_tag
        FOREIGN KEY (tag_id)
        REFERENCES tags (id)
        ON DELETE CASCADE,

    INDEX ix_faq_tags_tag_id (tag_id)
)
ENGINE = InnoDB
DEFAULT CHARACTER SET = utf8mb4
COLLATE = utf8mb4_unicode_ci;
