CREATE TABLE IF NOT EXISTS groups (
  group_id INTEGER PRIMARY KEY AUTOINCREMENT,
  group_name TEXT NOT NULL UNIQUE,
  billing_reference TEXT NULL,
  created_at_utc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now')),
  updated_at_utc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now'))
);

CREATE TABLE IF NOT EXISTS group_memberships (
  membership_id INTEGER PRIMARY KEY AUTOINCREMENT,
  group_id INTEGER NOT NULL,
  windows_username TEXT NOT NULL UNIQUE,
  created_at_utc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now')),
  updated_at_utc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now'))
);

CREATE INDEX IF NOT EXISTS ix_groups_group_name ON groups (group_name);
CREATE INDEX IF NOT EXISTS ix_group_memberships_group_id ON group_memberships (group_id);
CREATE INDEX IF NOT EXISTS ix_group_memberships_username ON group_memberships (windows_username);
