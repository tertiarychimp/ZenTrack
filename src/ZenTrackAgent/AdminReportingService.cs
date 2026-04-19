using System;
using System.Collections.Generic;
using System.Linq;

namespace ZenTrackAgent
{
    internal sealed class AdminReportingService
    {
        private readonly TrackerDatabase _trackerDatabase;
        private readonly GroupDatabase _groupDatabase;

        public AdminReportingService(TrackerDatabase trackerDatabase, GroupDatabase groupDatabase)
        {
            _trackerDatabase = trackerDatabase;
            _groupDatabase = groupDatabase;
        }

        public AdminFilterOptionsResponse GetFilterOptions()
        {
            return new AdminFilterOptionsResponse
            {
                Users = _trackerDatabase.GetAdminUserOptions(),
                Groups = _groupDatabase.GetGroupOptions()
            };
        }

        public AdminUsageSessionPage GetSessionsPage(int page, int pageSize, string userFilter, string groupFilter, string startDate, string endDate)
        {
            List<AdminUsageSessionListItem> filteredSessions = GetAnnotatedSessions(userFilter, startDate, endDate);

            if (!string.IsNullOrWhiteSpace(groupFilter))
            {
                filteredSessions = filteredSessions
                    .Where(item => string.Equals(item.GroupName, groupFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            pageSize = NormalizePageSize(pageSize, 20);
            if (page < 1)
            {
                page = 1;
            }

            int totalRows = filteredSessions.Count;
            int totalPages = totalRows == 0 ? 1 : (int)Math.Ceiling(totalRows / (double)pageSize);
            if (page > totalPages)
            {
                page = totalPages;
            }

            int skip = (page - 1) * pageSize;
            List<AdminUsageSessionListItem> pageItems = filteredSessions
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return new AdminUsageSessionPage
            {
                Page = page,
                PageSize = pageSize,
                TotalRows = totalRows,
                TotalPages = totalPages,
                Items = pageItems
            };
        }

        public AdminGroupSummaryResponse GetGroupSummary(string userFilter, string groupFilter, string startDate, string endDate)
        {
            List<AdminUsageSessionListItem> filteredSessions = GetAnnotatedSessions(userFilter, startDate, endDate);
            List<GroupRecord> groups = _groupDatabase.GetGroups();
            Dictionary<string, AdminGroupSummaryItem> summaryByGroup = new Dictionary<string, AdminGroupSummaryItem>(StringComparer.OrdinalIgnoreCase);

            foreach (GroupRecord group in groups)
            {
                summaryByGroup[group.GroupName] = new AdminGroupSummaryItem
                {
                    GroupName = group.GroupName,
                    BillingReference = group.BillingReference,
                    UserCount = 0,
                    SessionCount = 0,
                    TotalRoundedMinutes = 0,
                    CanManage = true
                };
            }

            foreach (IGrouping<string, AdminUsageSessionListItem> groupSessions in filteredSessions.GroupBy(item => item.GroupName ?? "Unassigned", StringComparer.OrdinalIgnoreCase))
            {
                AdminGroupSummaryItem summary;
                if (!summaryByGroup.TryGetValue(groupSessions.Key, out summary))
                {
                    summary = new AdminGroupSummaryItem
                    {
                        GroupName = groupSessions.Key,
                        BillingReference = null,
                        CanManage = false
                    };
                    summaryByGroup[groupSessions.Key] = summary;
                }

                summary.UserCount = groupSessions
                    .Select(item => item.WindowsUsername ?? string.Empty)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count();
                summary.SessionCount = groupSessions.Count();
                summary.TotalRoundedMinutes = groupSessions.Sum(item => RoundUpMinutes(item.DurationSeconds));
            }

            List<AdminGroupSummaryItem> items = summaryByGroup.Values
                .Where(item => string.IsNullOrWhiteSpace(groupFilter) || string.Equals(item.GroupName, groupFilter.Trim(), StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.GroupName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            int totalRoundedMinutes = filteredSessions.Sum(item => RoundUpMinutes(item.DurationSeconds));
            int totalUsers = filteredSessions
                .Select(item => item.WindowsUsername ?? string.Empty)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            return new AdminGroupSummaryResponse
            {
                TotalSessions = filteredSessions.Count,
                TotalRoundedMinutes = totalRoundedMinutes,
                TotalUsers = totalUsers,
                Items = items
            };
        }

        public void CreateGroup(string groupName)
        {
            _groupDatabase.CreateGroup(groupName);
        }

        public AdminGroupDetailsResponse GetGroupDetails(string groupName)
        {
            GroupRecord group = _groupDatabase.GetGroup(groupName);
            if (group == null)
            {
                return null;
            }

            HashSet<string> assignedUsers = new HashSet<string>(_groupDatabase.GetAssignedUsernames(), StringComparer.OrdinalIgnoreCase);
            List<AdminFilterOptionItem> currentMembers = _groupDatabase.GetUsersForGroup(groupName)
                .Select(CreateFilterItem)
                .ToList();

            List<AdminFilterOptionItem> unassignedUsers = _trackerDatabase.GetDistinctWindowsUsernames()
                .Where(username => !assignedUsers.Contains(username))
                .Select(CreateFilterItem)
                .ToList();

            return new AdminGroupDetailsResponse
            {
                GroupName = group.GroupName,
                BillingReference = group.BillingReference,
                CurrentMembers = currentMembers,
                UnassignedUsers = unassignedUsers
            };
        }

        public void SaveGroupDetails(string groupName, string billingReference, IEnumerable<string> windowsUsernames)
        {
            _groupDatabase.SaveGroupDetails(groupName, billingReference, windowsUsernames ?? new string[0]);
        }

        private List<AdminUsageSessionListItem> GetAnnotatedSessions(string userFilter, string startDate, string endDate)
        {
            Dictionary<string, GroupRecord> groupsByUser = _groupDatabase.GetGroupsByUser();
            List<AdminUsageSessionListItem> sessions = _trackerDatabase.GetAdminUsageSessions(userFilter, startDate, endDate);

            foreach (AdminUsageSessionListItem session in sessions)
            {
                GroupRecord group;
                if (session.WindowsUsername != null && groupsByUser.TryGetValue(session.WindowsUsername, out group))
                {
                    session.GroupName = group.GroupName;
                }
                else
                {
                    session.GroupName = "Unassigned";
                }
            }

            return sessions;
        }

        private static AdminFilterOptionItem CreateFilterItem(string value)
        {
            return new AdminFilterOptionItem
            {
                Value = value,
                Label = value
            };
        }

        private static int NormalizePageSize(int pageSize, int fallback)
        {
            if (pageSize <= 0)
            {
                return fallback;
            }

            if (pageSize > 100)
            {
                return 100;
            }

            return pageSize;
        }

        private static int RoundUpMinutes(int? durationSeconds)
        {
            if (!durationSeconds.HasValue)
            {
                return 0;
            }

            return (int)Math.Ceiling(durationSeconds.Value / 60d);
        }
    }
}
