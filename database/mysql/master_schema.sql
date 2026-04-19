CREATE DATABASE IF NOT EXISTS zentrack_master
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_0900_ai_ci;

USE zentrack_master;

CREATE TABLE IF NOT EXISTS users (
  user_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  windows_username VARCHAR(128) NOT NULL,
  display_name VARCHAR(128) NULL,
  email_address VARCHAR(255) NULL,
  is_active TINYINT(1) NOT NULL DEFAULT 1,
  created_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3),
  updated_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3) ON UPDATE UTC_TIMESTAMP(3),
  PRIMARY KEY (user_id),
  UNIQUE KEY ux_users_windows_username (windows_username)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS user_groups (
  group_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  group_name VARCHAR(128) NOT NULL,
  billing_code VARCHAR(64) NULL,
  is_active TINYINT(1) NOT NULL DEFAULT 1,
  created_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3),
  updated_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3) ON UPDATE UTC_TIMESTAMP(3),
  PRIMARY KEY (group_id),
  UNIQUE KEY ux_user_groups_group_name (group_name)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS user_group_memberships (
  membership_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  user_id BIGINT UNSIGNED NOT NULL,
  group_id BIGINT UNSIGNED NOT NULL,
  effective_from_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3),
  effective_to_utc DATETIME(3) NULL,
  is_primary_group TINYINT(1) NOT NULL DEFAULT 1,
  created_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3),
  updated_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3) ON UPDATE UTC_TIMESTAMP(3),
  PRIMARY KEY (membership_id),
  UNIQUE KEY ux_user_group_membership_window (user_id, group_id, effective_from_utc),
  KEY ix_user_group_memberships_active (group_id, effective_to_utc, effective_from_utc),
  CONSTRAINT fk_user_group_memberships_user FOREIGN KEY (user_id) REFERENCES users (user_id),
  CONSTRAINT fk_user_group_memberships_group FOREIGN KEY (group_id) REFERENCES user_groups (group_id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS machines (
  machine_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  machine_name VARCHAR(128) NOT NULL,
  site_name VARCHAR(128) NULL,
  operating_system VARCHAR(128) NULL,
  is_active TINYINT(1) NOT NULL DEFAULT 1,
  first_seen_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3),
  last_seen_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3),
  created_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3),
  updated_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3) ON UPDATE UTC_TIMESTAMP(3),
  PRIMARY KEY (machine_id),
  UNIQUE KEY ux_machines_machine_name (machine_name)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS agent_setting_profiles (
  setting_profile_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  profile_name VARCHAR(128) NOT NULL,
  poll_interval_seconds INT UNSIGNED NOT NULL DEFAULT 60,
  monitored_process_names_json JSON NOT NULL,
  mysql_host VARCHAR(255) NULL,
  mysql_database_name VARCHAR(128) NULL DEFAULT 'zentrack_master',
  reporting_page_size INT UNSIGNED NOT NULL DEFAULT 20,
  is_active TINYINT(1) NOT NULL DEFAULT 1,
  created_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3),
  updated_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3) ON UPDATE UTC_TIMESTAMP(3),
  PRIMARY KEY (setting_profile_id),
  UNIQUE KEY ux_agent_setting_profiles_profile_name (profile_name)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS machine_setting_assignments (
  assignment_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  machine_id BIGINT UNSIGNED NOT NULL,
  setting_profile_id BIGINT UNSIGNED NOT NULL,
  effective_from_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3),
  effective_to_utc DATETIME(3) NULL,
  created_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3),
  updated_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3) ON UPDATE UTC_TIMESTAMP(3),
  PRIMARY KEY (assignment_id),
  UNIQUE KEY ux_machine_setting_assignments_window (machine_id, setting_profile_id, effective_from_utc),
  KEY ix_machine_setting_assignments_active (machine_id, effective_to_utc, effective_from_utc),
  CONSTRAINT fk_machine_setting_assignments_machine FOREIGN KEY (machine_id) REFERENCES machines (machine_id),
  CONSTRAINT fk_machine_setting_assignments_profile FOREIGN KEY (setting_profile_id) REFERENCES agent_setting_profiles (setting_profile_id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS usage_sessions (
  master_session_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  source_system_name VARCHAR(128) NOT NULL,
  source_machine_name VARCHAR(128) NOT NULL,
  source_session_id BIGINT UNSIGNED NOT NULL,
  user_id BIGINT UNSIGNED NULL,
  machine_id BIGINT UNSIGNED NULL,
  windows_username VARCHAR(128) NOT NULL,
  process_name VARCHAR(128) NOT NULL,
  process_id INT UNSIGNED NOT NULL,
  started_at_utc DATETIME(3) NOT NULL,
  ended_at_utc DATETIME(3) NULL,
  duration_seconds INT UNSIGNED NULL,
  is_synced TINYINT(1) NOT NULL DEFAULT 1,
  billing_state ENUM('pending', 'ready', 'invoiced', 'excluded') NOT NULL DEFAULT 'pending',
  source_created_at_utc DATETIME(3) NULL,
  source_updated_at_utc DATETIME(3) NULL,
  received_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3),
  raw_payload_json JSON NULL,
  created_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3),
  updated_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3) ON UPDATE UTC_TIMESTAMP(3),
  PRIMARY KEY (master_session_id),
  UNIQUE KEY ux_usage_sessions_source (source_system_name, source_machine_name, source_session_id),
  KEY ix_usage_sessions_started (started_at_utc),
  KEY ix_usage_sessions_user_period (windows_username, started_at_utc),
  KEY ix_usage_sessions_machine_period (source_machine_name, started_at_utc),
  KEY ix_usage_sessions_billing (billing_state, started_at_utc),
  CONSTRAINT fk_usage_sessions_user FOREIGN KEY (user_id) REFERENCES users (user_id),
  CONSTRAINT fk_usage_sessions_machine FOREIGN KEY (machine_id) REFERENCES machines (machine_id)
) ENGINE=InnoDB;

CREATE TABLE IF NOT EXISTS billing_reports (
  billing_report_id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
  report_name VARCHAR(255) NOT NULL,
  period_start_date DATE NOT NULL,
  period_end_date DATE NOT NULL,
  group_id BIGINT UNSIGNED NULL,
  report_status ENUM('draft', 'finalized', 'exported') NOT NULL DEFAULT 'draft',
  generated_by VARCHAR(128) NULL,
  generated_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3),
  pdf_storage_path VARCHAR(512) NULL,
  notes TEXT NULL,
  created_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3),
  updated_at_utc DATETIME(3) NOT NULL DEFAULT UTC_TIMESTAMP(3) ON UPDATE UTC_TIMESTAMP(3),
  PRIMARY KEY (billing_report_id),
  KEY ix_billing_reports_period (period_start_date, period_end_date),
  CONSTRAINT fk_billing_reports_group FOREIGN KEY (group_id) REFERENCES user_groups (group_id)
) ENGINE=InnoDB;

-- Capacity note:
-- The expected volume of roughly 1,000 session events per month is modest for this schema.
-- The included indexes are sized to keep reporting, synchronization checks, and billing-period lookups fast
-- while leaving room for several years of retained history.
