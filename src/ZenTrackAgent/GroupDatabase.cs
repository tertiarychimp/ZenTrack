using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ZenTrackAgent
{
    internal sealed class GroupDatabase : IDisposable
    {
        private readonly SqliteNativeConnection _connection;

        public GroupDatabase(string databasePath)
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
                "CREATE TABLE IF NOT EXISTS groups (" +
                "group_id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "group_name TEXT NOT NULL UNIQUE, " +
                "billing_reference TEXT NULL, " +
                "created_at_utc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now')), " +
                "updated_at_utc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now'))" +
                ");");
            _connection.ExecuteNonQuery(
                "CREATE TABLE IF NOT EXISTS group_memberships (" +
                "membership_id INTEGER PRIMARY KEY AUTOINCREMENT, " +
                "group_id INTEGER NOT NULL, " +
                "windows_username TEXT NOT NULL UNIQUE, " +
                "created_at_utc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now')), " +
                "updated_at_utc TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now'))" +
                ");");
            _connection.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS ix_groups_group_name ON groups (group_name);");
            _connection.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS ix_group_memberships_group_id ON group_memberships (group_id);");
            _connection.ExecuteNonQuery("CREATE INDEX IF NOT EXISTS ix_group_memberships_username ON group_memberships (windows_username);");
        }

        public List<AdminFilterOptionItem> GetGroupOptions()
        {
            return GetGroups()
                .Select(group => new AdminFilterOptionItem
                {
                    Value = group.GroupName,
                    Label = group.GroupName
                })
                .ToList();
        }

        public List<GroupRecord> GetGroups()
        {
            lock (_connection)
            {
                return _connection.ExecuteQuery(
                    "SELECT group_id, group_name, billing_reference FROM groups ORDER BY group_name ASC;",
                    delegate(SqliteRowReader reader)
                    {
                        return new GroupRecord
                        {
                            GroupId = reader.GetInt64(0),
                            GroupName = reader.GetString(1),
                            BillingReference = ReadNullableString(reader, 2)
                        };
                    });
            }
        }

        public GroupRecord GetGroup(string groupName)
        {
            string normalizedGroupName = NormalizeGroupName(groupName);

            lock (_connection)
            {
                List<GroupRecord> results = _connection.ExecuteQuery(
                    "SELECT group_id, group_name, billing_reference FROM groups WHERE group_name = @group_name LIMIT 1;",
                    delegate(SqliteRowReader reader)
                    {
                        return new GroupRecord
                        {
                            GroupId = reader.GetInt64(0),
                            GroupName = reader.GetString(1),
                            BillingReference = ReadNullableString(reader, 2)
                        };
                    },
                    new SqliteParameter("@group_name", normalizedGroupName));

                return results.Count == 0 ? null : results[0];
            }
        }

        public Dictionary<string, GroupRecord> GetGroupsByUser()
        {
            lock (_connection)
            {
                List<GroupMembershipRecord> memberships = _connection.ExecuteQuery(
                    "SELECT gm.windows_username, g.group_id, g.group_name, g.billing_reference " +
                    "FROM group_memberships gm INNER JOIN groups g ON g.group_id = gm.group_id;",
                    delegate(SqliteRowReader reader)
                    {
                        return new GroupMembershipRecord
                        {
                            WindowsUsername = reader.GetString(0),
                            Group = new GroupRecord
                            {
                                GroupId = reader.GetInt64(1),
                                GroupName = reader.GetString(2),
                                BillingReference = ReadNullableString(reader, 3)
                            }
                        };
                    });

                return memberships.ToDictionary(
                    membership => membership.WindowsUsername,
                    membership => membership.Group,
                    StringComparer.OrdinalIgnoreCase);
            }
        }

        public List<string> GetUsersForGroup(string groupName)
        {
            string normalizedGroupName = NormalizeGroupName(groupName);

            lock (_connection)
            {
                return _connection.ExecuteQuery(
                    "SELECT gm.windows_username " +
                    "FROM group_memberships gm INNER JOIN groups g ON g.group_id = gm.group_id " +
                    "WHERE g.group_name = @group_name ORDER BY gm.windows_username ASC;",
                    delegate(SqliteRowReader reader)
                    {
                        return reader.GetString(0);
                    },
                    new SqliteParameter("@group_name", normalizedGroupName));
            }
        }

        public List<string> GetAssignedUsernames()
        {
            lock (_connection)
            {
                return _connection.ExecuteQuery(
                    "SELECT windows_username FROM group_memberships ORDER BY windows_username ASC;",
                    delegate(SqliteRowReader reader)
                    {
                        return reader.GetString(0);
                    });
            }
        }

        public void CreateGroup(string groupName)
        {
            string normalizedGroupName = NormalizeGroupName(groupName);

            lock (_connection)
            {
                _connection.ExecuteNonQuery(
                    "INSERT OR IGNORE INTO groups (group_name, billing_reference, created_at_utc, updated_at_utc) " +
                    "VALUES (@group_name, NULL, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), strftime('%Y-%m-%dT%H:%M:%fZ', 'now'));",
                    new SqliteParameter("@group_name", normalizedGroupName));
            }
        }

        public void SaveGroupDetails(string groupName, string billingReference, IEnumerable<string> windowsUsernames)
        {
            string normalizedGroupName = NormalizeGroupName(groupName);
            CreateGroup(normalizedGroupName);
            UpdateBillingReference(normalizedGroupName, billingReference);

            List<string> distinctUsers = (windowsUsernames ?? Enumerable.Empty<string>())
                .Where(username => !string.IsNullOrWhiteSpace(username))
                .Select(username => username.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (distinctUsers.Count == 0)
            {
                return;
            }

            GroupRecord group = GetGroup(normalizedGroupName);
            if (group == null)
            {
                throw new InvalidOperationException("Group could not be found after creation.");
            }

            lock (_connection)
            {
                foreach (string windowsUsername in distinctUsers)
                {
                    _connection.ExecuteNonQuery(
                        "DELETE FROM group_memberships WHERE windows_username = @windows_username;",
                        new SqliteParameter("@windows_username", windowsUsername));
                    _connection.ExecuteNonQuery(
                        "INSERT INTO group_memberships (group_id, windows_username, created_at_utc, updated_at_utc) " +
                        "VALUES (@group_id, @windows_username, strftime('%Y-%m-%dT%H:%M:%fZ', 'now'), strftime('%Y-%m-%dT%H:%M:%fZ', 'now'));",
                        new SqliteParameter("@group_id", group.GroupId),
                        new SqliteParameter("@windows_username", windowsUsername));
                }
            }
        }

        private void UpdateBillingReference(string groupName, string billingReference)
        {
            string normalizedReference = string.IsNullOrWhiteSpace(billingReference) ? null : billingReference.Trim();

            lock (_connection)
            {
                _connection.ExecuteNonQuery(
                    "UPDATE groups SET billing_reference = @billing_reference, " +
                    "updated_at_utc = strftime('%Y-%m-%dT%H:%M:%fZ', 'now') " +
                    "WHERE group_name = @group_name;",
                    new SqliteParameter("@billing_reference", normalizedReference),
                    new SqliteParameter("@group_name", groupName));
            }
        }

        private static string NormalizeGroupName(string groupName)
        {
            string normalizedGroupName = string.IsNullOrWhiteSpace(groupName) ? null : groupName.Trim();
            if (string.IsNullOrWhiteSpace(normalizedGroupName))
            {
                throw new InvalidOperationException("Group name is required.");
            }

            return normalizedGroupName;
        }

        private static string ReadNullableString(SqliteRowReader reader, int ordinal)
        {
            return reader.IsNull(ordinal) ? null : reader.GetString(ordinal);
        }

        public void Dispose()
        {
            _connection.Dispose();
        }
    }

    internal sealed class GroupRecord
    {
        public long GroupId { get; set; }

        public string GroupName { get; set; }

        public string BillingReference { get; set; }
    }

    internal sealed class GroupMembershipRecord
    {
        public string WindowsUsername { get; set; }

        public GroupRecord Group { get; set; }
    }
}
