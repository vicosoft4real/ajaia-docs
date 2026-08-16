CREATE TABLE IF NOT EXISTS schema_migrations (
    version varchar(100) PRIMARY KEY,
    applied_at timestamptz NOT NULL
);

CREATE TABLE app_users (
    id uuid PRIMARY KEY,
    email varchar(254) NOT NULL UNIQUE,
    display_name varchar(120) NOT NULL CHECK (length(btrim(display_name)) BETWEEN 1 AND 120),
    avatar_color varchar(7) NOT NULL CHECK (avatar_color ~ '^#[0-9A-Fa-f]{6}$'),
    is_seeded boolean NOT NULL DEFAULT false,
    created_at timestamptz NOT NULL
);

CREATE TABLE documents (
    id uuid PRIMARY KEY,
    owner_id uuid NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
    title varchar(120) NOT NULL CHECK (length(btrim(title)) BETWEEN 1 AND 120),
    content_format varchar(20) NOT NULL
        CHECK (content_format IN ('lexical', 'markdown', 'plainText')),
    content text NOT NULL,
    plain_text text NOT NULL,
    version integer NOT NULL DEFAULT 1 CHECK (version > 0),
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL
);

CREATE TABLE document_shares (
    document_id uuid NOT NULL REFERENCES documents(id) ON DELETE CASCADE,
    user_id uuid NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
    shared_by_user_id uuid NOT NULL REFERENCES app_users(id),
    created_at timestamptz NOT NULL,
    PRIMARY KEY (document_id, user_id)
);

CREATE INDEX ix_documents_owner_updated ON documents(owner_id, updated_at DESC);
CREATE INDEX ix_document_shares_user_document ON document_shares(user_id, document_id);

CREATE FUNCTION prevent_owner_document_share()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM documents
        WHERE id = NEW.document_id
          AND owner_id = NEW.user_id
    ) THEN
        RAISE EXCEPTION 'A document cannot be shared with its owner.'
            USING ERRCODE = '23514';
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER trg_prevent_owner_document_share
BEFORE INSERT OR UPDATE ON document_shares
FOR EACH ROW
EXECUTE FUNCTION prevent_owner_document_share();
