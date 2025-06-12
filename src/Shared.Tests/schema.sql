CREATE TABLE IF NOT EXISTS media (
    id UUID PRIMARY KEY,
    aws_s3_path TEXT NOT NULL,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE
);

CREATE TABLE IF NOT EXISTS posts (
    id UUID PRIMARY KEY,
    title TEXT NOT NULL,
    media_id UUID NOT NULL REFERENCES media(id),
    public_id SERIAL UNIQUE,
    json_meta JSONB,
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_posts_media_id ON posts(media_id);
CREATE INDEX IF NOT EXISTS idx_posts_public_id ON posts(public_id);
CREATE INDEX IF NOT EXISTS idx_posts_created_at ON posts(created_at); 