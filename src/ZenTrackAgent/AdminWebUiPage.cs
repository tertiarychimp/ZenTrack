namespace ZenTrackAgent
{
    internal static class AdminWebUiPage
    {
        public const string Html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>ZenTrack Administrator Level Reporting</title>
  <style>
    :root {
      color-scheme: light;
      --bg: #edf4ff;
      --panel: #fbfdff;
      --ink: #133553;
      --muted: #5a7693;
      --line: #c6daf6;
      --accent: #1d63b7;
      --accent-soft: #deebfb;
      --accent-deep: #104b93;
      --shadow: 0 20px 50px rgba(32, 78, 131, 0.14);
    }

    * {
      box-sizing: border-box;
    }

    body {
      margin: 0;
      font-family: ""Segoe UI"", Tahoma, sans-serif;
      color: var(--ink);
      background:
        radial-gradient(circle at top left, rgba(83, 148, 231, 0.2), transparent 28%),
        linear-gradient(180deg, #f7fbff 0%, var(--bg) 100%);
    }

    .shell {
      max-width: 1360px;
      margin: 0 auto;
      padding: 30px 20px 42px;
    }

    .hero {
      display: flex;
      justify-content: space-between;
      gap: 16px;
      align-items: end;
      margin-bottom: 18px;
    }

    .eyebrow {
      font-size: 12px;
      letter-spacing: 0.14em;
      text-transform: uppercase;
      color: var(--accent);
      font-weight: 700;
      margin-bottom: 8px;
    }

    h1 {
      margin: 0;
      font-size: clamp(30px, 4vw, 44px);
      line-height: 1;
      letter-spacing: -0.03em;
    }

    .subtitle {
      margin: 10px 0 0;
      color: var(--muted);
      max-width: 860px;
      line-height: 1.5;
    }

    .hero-note {
      min-width: 260px;
      max-width: 320px;
      padding: 16px 18px;
      border-radius: 18px;
      background: rgba(255, 255, 255, 0.72);
      border: 1px solid rgba(198, 218, 246, 0.9);
      box-shadow: var(--shadow);
      color: var(--muted);
      font-size: 13px;
      line-height: 1.5;
    }

    .card {
      background: rgba(251, 253, 255, 0.94);
      border: 1px solid rgba(198, 218, 246, 0.9);
      border-radius: 24px;
      box-shadow: var(--shadow);
      overflow: hidden;
      backdrop-filter: blur(10px);
      margin-bottom: 18px;
    }

    .toolbar {
      padding: 20px;
      border-bottom: 1px solid var(--line);
      background: linear-gradient(180deg, rgba(222, 235, 251, 0.85), rgba(251, 253, 255, 0.95));
    }

    .toolbar-head {
      display: flex;
      justify-content: space-between;
      gap: 16px;
      align-items: start;
      margin-bottom: 16px;
    }

    .status {
      font-size: 14px;
      color: var(--muted);
    }

    .nav-links,
    .actions {
      display: flex;
      flex-wrap: wrap;
      gap: 10px;
      align-items: center;
    }

    .filters {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: 12px;
    }

    .filter {
      display: grid;
      gap: 6px;
    }

    .filter label {
      font-size: 12px;
      letter-spacing: 0.08em;
      text-transform: uppercase;
      font-weight: 700;
      color: var(--muted);
    }

    input,
    select,
    button,
    a.button-link {
      font: inherit;
    }

    input,
    select {
      width: 100%;
      min-height: 42px;
      border: 1px solid var(--line);
      border-radius: 14px;
      padding: 10px 12px;
      color: var(--ink);
      background: #fff;
    }

    button,
    a.button-link {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-height: 42px;
      border: 0;
      border-radius: 999px;
      padding: 10px 16px;
      font-size: 14px;
      font-weight: 700;
      cursor: pointer;
      text-decoration: none;
      transition: transform 120ms ease, opacity 120ms ease, background 120ms ease;
    }

    button.primary,
    a.button-link.primary {
      background: var(--accent);
      color: #fff;
    }

    button.secondary,
    a.button-link.secondary {
      background: var(--accent-soft);
      color: var(--accent-deep);
    }

    button.ghost {
      background: transparent;
      color: var(--accent-deep);
      border: 1px solid var(--line);
    }

    button:disabled {
      opacity: 0.45;
      cursor: default;
      transform: none;
    }

    button:not(:disabled):hover,
    a.button-link:hover {
      transform: translateY(-1px);
    }

    .summary-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: 14px;
      padding: 20px;
      border-bottom: 1px solid var(--line);
      background: rgba(246, 250, 255, 0.78);
    }

    .summary-card {
      padding: 16px 18px;
      border-radius: 18px;
      background: #fff;
      border: 1px solid rgba(198, 218, 246, 0.9);
    }

    .summary-label {
      font-size: 12px;
      letter-spacing: 0.08em;
      text-transform: uppercase;
      font-weight: 700;
      color: var(--muted);
    }

    .summary-value {
      margin-top: 10px;
      font-size: 30px;
      font-weight: 700;
      letter-spacing: -0.04em;
      color: var(--accent-deep);
    }

    .summary-help {
      margin-top: 8px;
      color: var(--muted);
      font-size: 13px;
      line-height: 1.5;
    }

    .inline-form {
      display: flex;
      flex-wrap: wrap;
      gap: 10px;
      align-items: center;
    }

    .inline-form input[type=""text""] {
      min-width: 220px;
      flex: 1 1 220px;
    }

    .table-link {
      color: var(--accent-deep);
      font-weight: 700;
      text-decoration: none;
    }

    .table-link:hover {
      text-decoration: underline;
    }

    .section-head {
      display: flex;
      justify-content: space-between;
      gap: 16px;
      align-items: center;
      padding: 18px 20px 0;
    }

    .section-title {
      margin: 0;
      font-size: 18px;
    }

    .section-copy {
      margin: 6px 0 0;
      color: var(--muted);
      font-size: 13px;
      line-height: 1.5;
    }

    .table-wrap {
      overflow-x: auto;
      padding: 16px 20px 20px;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      min-width: 820px;
    }

    thead th {
      text-align: left;
      padding: 14px 16px;
      font-size: 12px;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: var(--muted);
      border-bottom: 1px solid var(--line);
      background: rgba(237, 244, 255, 0.88);
    }

    tbody td {
      padding: 14px 16px;
      border-bottom: 1px solid rgba(198, 218, 246, 0.7);
      vertical-align: top;
      font-size: 14px;
    }

    tbody tr:nth-child(even) {
      background: rgba(246, 250, 255, 0.72);
    }

    .mono {
      font-family: Consolas, ""Courier New"", monospace;
      font-size: 13px;
    }

    .pill {
      display: inline-flex;
      align-items: center;
      border-radius: 999px;
      padding: 6px 10px;
      font-size: 12px;
      font-weight: 700;
      background: var(--accent-soft);
      color: var(--accent-deep);
    }

    .pill.warn {
      background: #e9f0fb;
      color: #476a90;
    }

    .footer {
      display: flex;
      justify-content: space-between;
      gap: 12px;
      align-items: center;
      padding: 0 20px 20px;
      color: var(--muted);
      font-size: 13px;
    }

    .empty {
      padding: 24px 20px;
      color: var(--muted);
      text-align: center;
    }

    @media (max-width: 900px) {
      .hero,
      .toolbar-head,
      .section-head,
      .footer {
        flex-direction: column;
        align-items: start;
      }
    }

    @media (max-width: 760px) {
      .shell {
        padding: 18px 12px 28px;
      }
    }
  </style>
</head>
<body>
  <div class=""shell"">
    <section class=""hero"">
      <div>
        <div class=""eyebrow"">ZenTrack Master Reporting</div>
        <h1>ZenTrack Administrator Level Reporting</h1>
        <p class=""subtitle"">Administrative reporting view for the future master SQL database. Use filters for user, group, and local calendar dates, then review session detail and group-level usage totals from one place.</p>
      </div>
      <div class=""hero-note"">
        PDF billing exports will be generated from this area in a later step. The reporting layout below is already shaped to support that workflow.
      </div>
    </section>

    <section class=""card"">
      <div class=""toolbar"">
        <div class=""toolbar-head"">
          <div>
            <div id=""status"" class=""status"">Loading administrator reporting...</div>
            <div id=""pageMeta"" class=""status""></div>
          </div>
          <div class=""nav-links"">
            <a class=""button-link secondary"" href=""/"">Local Viewer</a>
            <a class=""button-link secondary"" href=""/admin/settings"">Agent Settings</a>
            <button id=""pdfButton"" class=""ghost"" type=""button"" disabled>PDF Billing Reports Soon</button>
          </div>
        </div>

        <div class=""filters"">
          <div class=""filter"">
            <label for=""userFilter"">User</label>
            <select id=""userFilter"">
              <option value="""">All users</option>
            </select>
          </div>
          <div class=""filter"">
            <label for=""groupFilter"">Group</label>
            <select id=""groupFilter"">
              <option value="""">All groups</option>
            </select>
          </div>
          <div class=""filter"">
            <label for=""startDateFilter"">Start Date</label>
            <input id=""startDateFilter"" type=""date"" />
          </div>
          <div class=""filter"">
            <label for=""endDateFilter"">End Date</label>
            <input id=""endDateFilter"" type=""date"" />
          </div>
          <div class=""filter"">
            <label for=""pageSizeSelect"">Page Size</label>
            <select id=""pageSizeSelect"">
              <option value=""10"">10 rows</option>
              <option value=""20"" selected>20 rows</option>
              <option value=""50"">50 rows</option>
              <option value=""100"">100 rows</option>
            </select>
          </div>
          <div class=""filter"">
            <label>Actions</label>
            <div class=""actions"">
              <button id=""applyButton"" class=""primary"" type=""button"">Apply Filters</button>
              <button id=""clearButton"" class=""secondary"" type=""button"">Clear</button>
              <button id=""refreshButton"" class=""secondary"" type=""button"">Refresh</button>
            </div>
          </div>
        </div>
      </div>

      <div class=""summary-grid"">
        <div class=""summary-card"">
          <div class=""summary-label"">Filtered Sessions</div>
          <div id=""summarySessions"" class=""summary-value"">0</div>
          <div class=""summary-help"">Total matching usage sessions in the selected period.</div>
        </div>
        <div class=""summary-card"">
          <div class=""summary-label"">Rounded Hours</div>
          <div id=""summaryHours"" class=""summary-value"">0.0</div>
          <div class=""summary-help"">Rounded-up minutes summarized as hours for reporting and billing preparation.</div>
        </div>
        <div class=""summary-card"">
          <div class=""summary-label"">Active Users</div>
          <div id=""summaryUsers"" class=""summary-value"">0</div>
          <div class=""summary-help"">Distinct users represented in the current reporting slice.</div>
        </div>
      </div>

      <div class=""section-head"">
        <div>
          <h2 class=""section-title"">Group Reporting</h2>
          <p class=""section-copy"">Current group totals can be filtered by user and date range, and new groups can be added directly here for future invoicing.</p>
        </div>
        <div class=""inline-form"">
          <input id=""newGroupName"" type=""text"" placeholder=""Add a new group name"" />
          <button id=""addGroupButton"" class=""primary"" type=""button"">Add Group</button>
        </div>
      </div>
      <div class=""table-wrap"">
        <table>
          <thead>
            <tr>
              <th>Group</th>
              <th>Billing Reference</th>
              <th>Users</th>
              <th>Sessions</th>
              <th>Rounded Minutes</th>
            </tr>
          </thead>
          <tbody id=""groupSummaryRows""></tbody>
        </table>
      </div>
      <div id=""groupSummaryEmpty"" class=""empty"" hidden>No group-level summary is available for the current filters.</div>

      <div class=""section-head"">
        <div>
          <h2 class=""section-title"">Session Detail</h2>
          <p class=""section-copy"">Detailed entries for the filtered reporting slice. Local date and time are shown for quick review.</p>
        </div>
        <div class=""actions"">
          <button id=""previousButton"" class=""secondary"" type=""button"">Previous</button>
          <button id=""nextButton"" class=""primary"" type=""button"">Next</button>
        </div>
      </div>
      <div class=""table-wrap"">
        <table>
          <thead>
            <tr>
              <th>User</th>
              <th>Group</th>
              <th>Machine</th>
              <th>Process</th>
              <th>Started Date</th>
              <th>Started Time</th>
              <th>Ended Date</th>
              <th>Ended Time</th>
              <th>Duration</th>
              <th>Sync</th>
            </tr>
          </thead>
          <tbody id=""sessionRows""></tbody>
        </table>
      </div>
      <div id=""sessionEmpty"" class=""empty"" hidden>No matching sessions found for the current filters.</div>

      <div class=""footer"">
        <div id=""footnote"">Prepared for future administrator review, group reporting, settings management, and PDF billing export.</div>
        <div>Blue reporting view for the master SQL rollout.</div>
      </div>
    </section>
  </div>

  <script>
    (function () {
      var page = 1;
      var pageSize = 20;
      var sessionRows = document.getElementById('sessionRows');
      var sessionEmpty = document.getElementById('sessionEmpty');
      var groupSummaryRows = document.getElementById('groupSummaryRows');
      var groupSummaryEmpty = document.getElementById('groupSummaryEmpty');
      var status = document.getElementById('status');
      var pageMeta = document.getElementById('pageMeta');
      var userFilter = document.getElementById('userFilter');
      var groupFilter = document.getElementById('groupFilter');
      var startDateFilter = document.getElementById('startDateFilter');
      var endDateFilter = document.getElementById('endDateFilter');
      var pageSizeSelect = document.getElementById('pageSizeSelect');
      var applyButton = document.getElementById('applyButton');
      var clearButton = document.getElementById('clearButton');
      var refreshButton = document.getElementById('refreshButton');
      var previousButton = document.getElementById('previousButton');
      var nextButton = document.getElementById('nextButton');
      var summarySessions = document.getElementById('summarySessions');
      var summaryHours = document.getElementById('summaryHours');
      var summaryUsers = document.getElementById('summaryUsers');
      var newGroupName = document.getElementById('newGroupName');
      var addGroupButton = document.getElementById('addGroupButton');

      function escapeHtml(value) {
        return String(value == null ? '' : value)
          .replace(/&/g, '&amp;')
          .replace(/</g, '&lt;')
          .replace(/>/g, '&gt;')
          .replace(/""/g, '&quot;')
          .replace(/'/g, '&#39;');
      }

      function parseDate(value) {
        if (!value) {
          return null;
        }

        var date = new Date(value);
        if (isNaN(date.getTime())) {
          return null;
        }

        return date;
      }

      function formatLocalDate(value) {
        var date = parseDate(value);
        if (!date) {
          return '<span class=""pill warn"">Still running</span>';
        }

        return escapeHtml(date.toLocaleDateString());
      }

      function formatLocalTime(value) {
        var date = parseDate(value);
        if (!date) {
          return '<span class=""pill warn"">Still running</span>';
        }

        return '<span class=""mono"">' + escapeHtml(date.toLocaleTimeString()) + '</span>';
      }

      function formatDuration(seconds) {
        if (seconds == null) {
          return '<span class=""pill warn"">In progress</span>';
        }

        var roundedMinutes = Math.ceil(Number(seconds) / 60);
        var hours = Math.floor(roundedMinutes / 60);
        var minutes = roundedMinutes % 60;
        return '<span class=""mono"">' + hours + 'h ' + minutes + 'm</span>';
      }

      function formatHoursFromMinutes(totalMinutes) {
        return (Number(totalMinutes || 0) / 60).toFixed(1);
      }

      function buildQuery(pageValue) {
        var params = [
          'page=' + encodeURIComponent(pageValue),
          'pageSize=' + encodeURIComponent(pageSize),
          'user=' + encodeURIComponent(userFilter.value || ''),
          'group=' + encodeURIComponent(groupFilter.value || ''),
          'startDate=' + encodeURIComponent(startDateFilter.value || ''),
          'endDate=' + encodeURIComponent(endDateFilter.value || '')
        ];

        return params.join('&');
      }

      function setOptions(selectElement, items, defaultLabel) {
        var current = selectElement.value;
        var html = '<option value="""">' + escapeHtml(defaultLabel) + '</option>';
        for (var i = 0; i < items.length; i += 1) {
          html += '<option value=""' + escapeHtml(items[i].value) + '"">' + escapeHtml(items[i].label) + '</option>';
        }

        selectElement.innerHTML = html;
        selectElement.value = current;
        if (selectElement.value !== current) {
          selectElement.value = '';
        }
      }

      function groupLink(item) {
        if (!item.canManage) {
          return escapeHtml(item.groupName);
        }

        return '<a class=""table-link"" href=""/admin/groups?name=' + encodeURIComponent(item.groupName) + '"">' + escapeHtml(item.groupName) + '</a>';
      }

      function renderGroupSummary(items) {
        if (!items || !items.length) {
          groupSummaryRows.innerHTML = '';
          groupSummaryEmpty.hidden = false;
          return;
        }

        groupSummaryEmpty.hidden = true;
        groupSummaryRows.innerHTML = items.map(function (item) {
          return '<tr>' +
            '<td>' + groupLink(item) + '</td>' +
            '<td>' + escapeHtml(item.billingReference || '') + '</td>' +
            '<td>' + escapeHtml(item.userCount) + '</td>' +
            '<td>' + escapeHtml(item.sessionCount) + '</td>' +
            '<td><span class=""mono"">' + escapeHtml(item.totalRoundedMinutes) + 'm</span></td>' +
            '</tr>';
        }).join('');
      }

      function renderSessions(items) {
        if (!items || !items.length) {
          sessionRows.innerHTML = '';
          sessionEmpty.hidden = false;
          return;
        }

        sessionEmpty.hidden = true;
        sessionRows.innerHTML = items.map(function (item) {
          return '<tr>' +
            '<td>' + escapeHtml(item.windowsUsername) + '</td>' +
            '<td>' + escapeHtml(item.groupName || 'Unassigned') + '</td>' +
            '<td><div>' + escapeHtml(item.machineName) + '</div><div class=""mono"">PID ' + escapeHtml(item.processId) + '</div></td>' +
            '<td>' + escapeHtml(item.processName) + '</td>' +
            '<td>' + formatLocalDate(item.startedAtUtc) + '</td>' +
            '<td>' + formatLocalTime(item.startedAtUtc) + '</td>' +
            '<td>' + formatLocalDate(item.endedAtUtc) + '</td>' +
            '<td>' + formatLocalTime(item.endedAtUtc) + '</td>' +
            '<td>' + formatDuration(item.durationSeconds) + '</td>' +
            '<td>' + (item.isSynced ? '<span class=""pill"">Synced</span>' : '<span class=""pill warn"">Local only</span>') + '</td>' +
            '</tr>';
        }).join('');
      }

      function loadFilterOptions() {
        return fetch('/api/admin/filter-options', { cache: 'no-store' })
          .then(function (response) { return response.json(); })
          .then(function (data) {
            setOptions(userFilter, data.users || [], 'All users');
            setOptions(groupFilter, data.groups || [], 'All groups');
          });
      }

      function loadGroupSummary() {
        return fetch('/api/admin/group-summary?' + buildQuery(page), { cache: 'no-store' })
          .then(function (response) { return response.json(); })
          .then(function (data) {
            renderGroupSummary(data.items || []);
            summarySessions.textContent = String(data.totalSessions || 0);
            summaryHours.textContent = formatHoursFromMinutes(data.totalRoundedMinutes || 0);
            summaryUsers.textContent = String(data.totalUsers || 0);
          });
      }

      function loadSessions() {
        status.textContent = 'Loading administrator reporting...';
        return fetch('/api/admin/sessions?' + buildQuery(page), { cache: 'no-store' })
          .then(function (response) { return response.json(); })
          .then(function (data) {
            renderSessions(data.items || []);
            var totalRows = Number(data.totalRows || 0);
            var totalPages = Number(data.totalPages || 1);
            status.textContent = totalRows + ' filtered session' + (totalRows === 1 ? '' : 's');
            pageMeta.textContent = 'Page ' + data.page + ' of ' + totalPages;
            previousButton.disabled = data.page <= 1;
            nextButton.disabled = data.page >= totalPages || totalRows === 0;
          });
      }

      function loadAll() {
        Promise.all([loadSessions(), loadGroupSummary()])
          .catch(function (error) {
            status.textContent = 'Could not load administrator reporting.';
            pageMeta.textContent = error && error.message ? error.message : 'Unknown error';
            sessionRows.innerHTML = '';
            groupSummaryRows.innerHTML = '';
            sessionEmpty.hidden = false;
            groupSummaryEmpty.hidden = false;
          });
      }

      function applyFilters() {
        page = 1;
        loadAll();
      }

      function createGroup() {
        var name = (newGroupName.value || '').trim();
        if (!name) {
          status.textContent = 'Enter a group name before adding it.';
          return;
        }

        addGroupButton.disabled = true;
        status.textContent = 'Adding group...';
        fetch('/api/admin/groups', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8'
          },
          body: 'groupName=' + encodeURIComponent(name)
        })
          .then(function (response) { return response.json(); })
          .then(function (data) {
            if (!data.success) {
              throw new Error(data.message || 'The group could not be added.');
            }

            newGroupName.value = '';
            addGroupButton.disabled = false;
            return loadFilterOptions().then(function () {
              loadAll();
            });
          })
          .catch(function (error) {
            status.textContent = error && error.message ? error.message : 'The group could not be added.';
            addGroupButton.disabled = false;
          });
      }

      applyButton.addEventListener('click', applyFilters);
      addGroupButton.addEventListener('click', createGroup);
      newGroupName.addEventListener('keydown', function (event) {
        if (event.key === 'Enter') {
          event.preventDefault();
          createGroup();
        }
      });

      clearButton.addEventListener('click', function () {
        userFilter.value = '';
        groupFilter.value = '';
        startDateFilter.value = '';
        endDateFilter.value = '';
        page = 1;
        loadAll();
      });

      refreshButton.addEventListener('click', function () {
        loadAll();
      });

      previousButton.addEventListener('click', function () {
        if (page > 1) {
          page -= 1;
          loadAll();
        }
      });

      nextButton.addEventListener('click', function () {
        page += 1;
        loadAll();
      });

      pageSizeSelect.addEventListener('change', function () {
        pageSize = Number(pageSizeSelect.value) || 20;
        page = 1;
        loadAll();
      });

      loadFilterOptions()
        .then(function () {
          loadAll();
        })
        .catch(function (error) {
          status.textContent = 'Could not load administrator filters.';
          pageMeta.textContent = error && error.message ? error.message : 'Unknown error';
        });
    }());
  </script>
</body>
</html>";
    }
}
