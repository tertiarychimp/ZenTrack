using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ZenTrackAgent
{
    internal sealed class TrackerDatabase : IDisposable
    {
        private readonly SqliteNativeConnection _connection;

        public TrackerDatabase(string databasePath)
        {
            DatabasePath = databasePath;
            _connection = new SqliteNativeConnection(databasePath);
        }

        public string DatabasePath { get; private set; }

        public void Initialize()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(DatabasePath) ?? AppDomain.CurrentDomain.BaseDirectory);
            _connection.Open();
            _connection.ExecuteNonQuery("PRAGMA busy_timeout = 5000;");
            _connection.ExecuteNonQuery(
                "CREATE TABLE IF NOT EXISTS usage_sessions (" +
                "session_id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "machine_name TEXT NOT NULL, " +
                "windows_username TEXT NOT NULL, " +
                "process_name TEXT NOT NULL, " +
                "process_id INTEGER NOT NULL, " +
                "started_at_utc TEXT NOT NULL, " +
                "ended_at_utc TEXT NULL, " +
                "duration_seconds INTEGER NULL, " +
                "is_synced INTEGER NOT NULL DEFAULT 0, " +
                "created_at_utc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now')), " +
                "updated_at_utc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now'))" +
                ");");
            _connection.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS ix_usage_sessions_sync ON usage_sessions (is_synced, started_at_utc);");
            _connection.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS ix_usage_sessions_open ON usage_sessions (ended_at_utc);");
        }

        public void CloseOpenSessions(DateTime closedAtUtc)
        {
            string closedAt = ToSqlUtc(closedAtUtc);
            string sql =
                "UPDATE usage_sessions " +
                "SET ended_at_utc = @ended_at_utc, " +
                "duration_seconds = CASE " +
                "WHEN started_at_utc IS NULL THEN NULL " +
                "ELSE CAST((julianday(@ended_at_utc) - julianday(started_at_utc)) * 86400 AS INTEGER) " +
                "END, " +
                "updated_at_utc = strftime('%Y-%m-%dT%H:%M:%fZ', 'now') " +
                "WHERE ended_at_utc IS NULL;";

            _connection.ExecuteNonQuery(
                sql,
                new SqliteParameter("@ended_at_utc", closedAt));
        }

        public long InsertSession(SessionRecord session)
        {
            string sql =
                "INSERT INTO usage_sessions (" +
                "machine_name, windows_username, process_name, process_id, started_at_utc, ended_at_utc, duration_seconds, is_synced, created_at_utc, updated_at_utc" +
                ") VALUES (" +
                "@machine_name, @windows_username, @process_name, @process_id, @started_at_utc, NULL, NULL, 0, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), strftime('%Y-%m-%dT%H:%M:%fZ', 'now')" +
                ");";

            _connection.ExecuteNonQuery(
                sql,
                new SqliteParameter("@machine_name", session.MachineName),
                new SqliteParameter("@windows_username", session.WindowsUserName),
                new SqliteParameter("@process_name", session.ProcessName),
                new SqliteParameter("@process_id", session.ProcessId),
                new SqliteParameter("@started_at_utc", ToSqlUtc(session.StartedAtUtc)));

            return _connection.ExecuteScalarInt64("SELECT last_insert_rowid();");
        }

        public void CloseSession(SessionRecord session)
        {
            if (!session.EndedAtUtc.HasValue)
            {
                throw new InvalidOperationException("Session end time is required before closing a session.");
            }

            string endedAt = ToSqlUtc(session.EndedAtUtc.Value);
            string sql =
                "UPDATE usage_sessions " +
                "SET ended_at_utc = @ended_at_utc, " +
                "duration_seconds = CAST((julianday(@ended_at_utc) - julianday(started_at_utc)) * 86400 AS INTEGER), " +
                "updated_at_utc = strftime('%Y-%m-%dT%H:%M:%fZ', 'now') " +
                "WHERE session_id = @session_id;";

            _connection.ExecuteNonQuery(
                sql,
                new SqliteParameter("@ended_at_utc", endedAt),
                new SqliteParameter("@session_id", session.SessionId));
        }

        public UsageSessionPage GetUsageSessionsPage(int page, int pageSize)
        {
            if (page < 1)
            {
                page = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 10;
            }

            lock (_connection)
            {
                int totalRows = GetUsageSessionCount();
                int totalPages = totalRows == 0 ? 1 : (int)Math.Ceiling(totalRows / (double)pageSize);
                if (page > totalPages)
                {
                    page = totalPages;
                }

                int offset = (page - 1) * pageSize;
                List<UsageSessionListItem> items = _connection.ExecuteQuery(
                    "SELECT session_id, machine_name, windows_username, process_name, process_id, started_at_utc, ended_at_utc, duration_seconds, is_synced " +
                    "FROM usage_sessions ORDER BY session_id DESC LIMIT @limit OFFSET @offset;",
                    delegate(SqliteRowReader reader)
                    {
                        return new UsageSessionListItem
                        {
                            SessionId = reader.GetInt64(0),
                            MachineName = reader.GetString(1),
                            WindowsUsername = reader.GetString(2),
                            ProcessName = reader.GetString(3),
                            ProcessId = reader.GetInt32(4),
                            StartedAtUtc = ReadNullableString(reader, 5),
                            EndedAtUtc = ReadNullableString(reader, 6),
                            DurationSeconds = reader.IsNull(7) ? (int?)null : reader.GetInt32(7),
                            IsSynced = !reader.IsNull(8) && reader.GetInt32(8) != 0
                        };
                    },
                    new SqliteParameter("@limit", pageSize),
                    new SqliteParameter("@offset", offset));

                return new UsageSessionPage
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalRows = totalRows,
                    TotalPages = totalPages,
                    Items = items
                };
            }
        }

        public List<AdminFilterOptionItem> GetAdminUserOptions()
        {
            lock (_connection)
            {
                return _connection.ExecuteQuery(
                    "SELECT DISTINCT windows_username FROM usage_sessions ORDER BY windows_username ASC;",
                    delegate(SqliteRowReader reader)
                    {
                        string windowsUsername = reader.GetString(0);
                        return new AdminFilterOptionItem
                        {
                            Value = windowsUsername,
                            Label = windowsUsername
                        };
                    });
            }
        }

        public List<string> GetDistinctWindowsUsernames()
        {
            lock (_connection)
            {
                return _connection.ExecuteQuery(
                    "SELECT DISTINCT windows_username FROM usage_sessions ORDER BY windows_username ASC;",
                    delegate(SqliteRowReader reader)
                    {
                        return reader.GetString(0);
                    });
            }
        }

        public List<AdminUsageSessionListItem> GetAdminUsageSessions(string userFilter, string startDate, string endDate)
        {
            startDate = NormalizeDateFilter(startDate);
            endDate = NormalizeDateFilter(endDate);

            lock (_connection)
            {
                List<SqliteParameter> parameters = new List<SqliteParameter>();
                string sql =
                    "SELECT machine_name, windows_username, process_name, process_id, started_at_utc, ended_at_utc, duration_seconds, is_synced " +
                    "FROM usage_sessions WHERE 1 = 1" +
                    BuildAdminSessionFilters(parameters, userFilter, startDate, endDate) +
                    " ORDER BY session_id DESC;";

                return _connection.ExecuteQuery(
                    sql,
                    delegate(SqliteRowReader reader)
                    {
                        return new AdminUsageSessionListItem
                        {
                            MachineName = reader.GetString(0),
                            WindowsUsername = reader.GetString(1),
                            ProcessName = reader.GetString(2),
                            ProcessId = reader.GetInt32(3),
                            StartedAtUtc = ReadNullableString(reader, 4),
                            EndedAtUtc = ReadNullableString(reader, 5),
                            DurationSeconds = reader.IsNull(6) ? (int?)null : reader.GetInt32(6),
                            IsSynced = !reader.IsNull(7) && reader.GetInt32(7) != 0
                        };
                    },
                    parameters.ToArray());
            }
        }

        private int GetUsageSessionCount()
        {
            return Convert.ToInt32(_connection.ExecuteScalarInt64("SELECT COUNT(*) FROM usage_sessions;"), CultureInfo.InvariantCulture);
        }

        private static string BuildAdminSessionFilters(List<SqliteParameter> parameters, string userFilter, string startDate, string endDate)
        {
            string sql = string.Empty;

            if (!string.IsNullOrWhiteSpace(userFilter))
            {
                sql += " AND windows_username = @user_name";
                parameters.Add(new SqliteParameter("@user_name", userFilter.Trim()));
            }

            if (!string.IsNullOrEmpty(startDate))
            {
                sql += " AND date(started_at_utc, 'localtime') >= @start_date";
                parameters.Add(new SqliteParameter("@start_date", startDate));
            }

            if (!string.IsNullOrEmpty(endDate))
            {
                sql += " AND date(started_at_utc, 'localtime') <= @end_date";
                parameters.Add(new SqliteParameter("@end_date", endDate));
            }

            return sql;
        }

        private static string NormalizeDateFilter(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            DateTime parsedDate;
            if (!DateTime.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
            {
                return null;
            }

            return parsedDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
        private static string ReadNullableString(SqliteRowReader reader, int ordinal)
        {
            return reader.IsNull(ordinal) ? null : reader.GetString(ordinal);
        }

        private static string ToSqlUtc(DateTime value)
        {
            return value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }
}
