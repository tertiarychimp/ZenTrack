namespace ZenTrackAgent
{
    internal static class WebUiPage
    {
        public const string Html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>ZenTrack Sessions</title>
  <style>
    :root {
      color-scheme: light;
      --bg: #eef2ea;
      --panel: #fbfcf8;
      --ink: #1e2b20;
      --muted: #5e6d60;
      --line: #c9d3c4;
      --accent: #2e6b3f;
      --accent-soft: #deecdf;
      --warn: #8a5a14;
      --shadow: 0 18px 50px rgba(35, 52, 36, 0.12);
    }

    * {
      box-sizing: border-box;
    }

    body {
      margin: 0;
      font-family: ""Segoe UI"", Tahoma, sans-serif;
      color: var(--ink);
      background:
        radial-gradient(circle at top left, rgba(112, 154, 108, 0.18), transparent 34%),
        linear-gradient(180deg, #f6f9f2 0%, var(--bg) 100%);
    }

    .shell {
      max-width: 1240px;
      margin: 0 auto;
      padding: 28px 20px 40px;
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
      font-size: clamp(28px, 4vw, 42px);
      line-height: 1;
      letter-spacing: -0.03em;
    }

    .subtitle {
      margin: 10px 0 0;
      color: var(--muted);
      max-width: 720px;
      line-height: 1.5;
    }

    .card {
      background: rgba(251, 252, 248, 0.92);
      border: 1px solid rgba(201, 211, 196, 0.9);
      border-radius: 20px;
      box-shadow: var(--shadow);
      overflow: hidden;
      backdrop-filter: blur(10px);
    }

    .toolbar {
      display: flex;
      flex-wrap: wrap;
      justify-content: space-between;
      gap: 12px;
      padding: 18px 20px;
      border-bottom: 1px solid var(--line);
      background: linear-gradient(180deg, rgba(222, 236, 223, 0.6), rgba(251, 252, 248, 0.9));
    }

    .status {
      font-size: 14px;
      color: var(--muted);
    }

    .actions {
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .page-size {
      display: inline-flex;
      align-items: center;
      gap: 8px;
      color: var(--muted);
      font-size: 13px;
    }

    select {
      border: 1px solid var(--line);
      border-radius: 999px;
      padding: 8px 12px;
      font-size: 13px;
      font-weight: 700;
      color: var(--ink);
      background: var(--panel);
    }

    button {
      border: 0;
      border-radius: 999px;
      padding: 10px 16px;
      font-size: 14px;
      font-weight: 700;
      cursor: pointer;
      transition: transform 120ms ease, opacity 120ms ease, background 120ms ease;
    }

    button.primary {
      background: var(--accent);
      color: #fff;
    }

    button.secondary {
      background: var(--accent-soft);
      color: var(--accent);
    }

    button:disabled {
      opacity: 0.45;
      cursor: default;
      transform: none;
    }

    button:not(:disabled):hover {
      transform: translateY(-1px);
    }

    .table-wrap {
      overflow-x: auto;
    }

    table {
      width: 100%;
      border-collapse: collapse;
      min-width: 980px;
    }

    thead th {
      text-align: left;
      padding: 14px 18px;
      font-size: 12px;
      text-transform: uppercase;
      letter-spacing: 0.08em;
      color: var(--muted);
      border-bottom: 1px solid var(--line);
      background: rgba(238, 242, 234, 0.68);
    }

    tbody td {
      padding: 14px 18px;
      border-bottom: 1px solid rgba(201, 211, 196, 0.6);
      vertical-align: top;
      font-size: 14px;
    }

    tbody tr:nth-child(even) {
      background: rgba(246, 249, 242, 0.8);
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
      background: #e6efe5;
      color: var(--accent);
    }

    .pill.warn {
      background: #fff0d7;
      color: var(--warn);
    }

    .empty {
      padding: 28px 20px;
      color: var(--muted);
      text-align: center;
    }

    .footer {
      display: flex;
      justify-content: space-between;
      gap: 12px;
      align-items: center;
      padding: 16px 20px 20px;
      color: var(--muted);
      font-size: 13px;
    }

    @media (max-width: 760px) {
      .shell {
        padding: 18px 12px 28px;
      }

      .hero {
        align-items: start;
        flex-direction: column;
      }

      .toolbar,
      .footer {
        flex-direction: column;
        align-items: start;
      }

      .actions {
        width: 100%;
      }
    }
  </style>
</head>
<body>
  <div class=""shell"">
    <section class=""hero"">
      <div>
        <div class=""eyebrow"">ZenTrack Local Viewer</div>
        <h1>ZEN Session Log</h1>
        <p class=""subtitle"">Local view of the Zen usage tracking data.</p>
      </div>
    </section>

    <section class=""card"">
      <div class=""toolbar"">
        <div>
          <div id=""status"" class=""status"">Loading sessions...</div>
          <div id=""pageMeta"" class=""status""></div>
        </div>
        <div class=""actions"">
          <button id=""refreshButton"" class=""secondary"" type=""button"">Refresh</button>
          <button id=""previousButton"" class=""secondary"" type=""button"">Previous</button>
          <button id=""nextButton"" class=""primary"" type=""button"">Next</button>
        </div>
      </div>

      <div class=""table-wrap"">
        <table>
          <thead>
            <tr>
              <th>User</th>
              <th>Machine</th>
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

      <div id=""emptyState"" class=""empty"" hidden>No tracked sessions found yet.</div>

      <div class=""footer"">
        <div id=""footnote"">Showing the newest ZEN.exe sessions first.</div>
        <label class=""page-size"" for=""pageSizeSelect"">
          <span>Page size</span>
          <select id=""pageSizeSelect"">
            <option value=""10"" selected>10 rows</option>
            <option value=""20"">20 rows</option>
            <option value=""50"">50 rows</option>
            <option value=""100"">100 rows</option>
          </select>
        </label>
      </div>
    </section>
  </div>

  <script>
    (function () {
      var page = 1;
      var pageSize = 10;
      var tableBody = document.getElementById('sessionRows');
      var status = document.getElementById('status');
      var pageMeta = document.getElementById('pageMeta');
      var emptyState = document.getElementById('emptyState');
      var previousButton = document.getElementById('previousButton');
      var nextButton = document.getElementById('nextButton');
      var refreshButton = document.getElementById('refreshButton');
      var pageSizeSelect = document.getElementById('pageSizeSelect');

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

        var total = Number(seconds);
        var roundedMinutes = Math.ceil(total / 60);
        var hours = Math.floor(roundedMinutes / 60);
        var minutes = roundedMinutes % 60;
        return '<span class=""mono"">' + hours + 'h ' + minutes + 'm</span>';
      }

      function renderRows(items) {
        if (!items || !items.length) {
          tableBody.innerHTML = '';
          emptyState.hidden = false;
          return;
        }

        emptyState.hidden = true;
        tableBody.innerHTML = items.map(function (item) {
          return '<tr>' +
            '<td>' + escapeHtml(item.windowsUsername) + '</td>' +
            '<td><div>' + escapeHtml(item.machineName) + '</div><div class=""mono"">PID ' + escapeHtml(item.processId) + '</div></td>' +
            '<td>' + formatLocalDate(item.startedAtUtc) + '</td>' +
            '<td>' + formatLocalTime(item.startedAtUtc) + '</td>' +
            '<td>' + formatLocalDate(item.endedAtUtc) + '</td>' +
            '<td>' + formatLocalTime(item.endedAtUtc) + '</td>' +
            '<td>' + formatDuration(item.durationSeconds) + '</td>' +
            '<td>' + (item.isSynced ? '<span class=""pill"">Synced</span>' : '<span class=""pill warn"">Local only</span>') + '</td>' +
            '</tr>';
        }).join('');
      }

      function loadSessions() {
        status.textContent = 'Loading sessions...';
        fetch('/api/sessions?page=' + page + '&pageSize=' + pageSize, { cache: 'no-store' })
          .then(function (response) { return response.json(); })
          .then(function (data) {
            renderRows(data.items);
            var totalRows = Number(data.totalRows || 0);
            var totalPages = Number(data.totalPages || 1);
            status.textContent = totalRows + ' total session' + (totalRows === 1 ? '' : 's');
            pageMeta.textContent = 'Page ' + data.page + ' of ' + totalPages;
            previousButton.disabled = data.page <= 1;
            nextButton.disabled = data.page >= totalPages || totalRows === 0;
          })
          .catch(function (error) {
            status.textContent = 'Could not load sessions.';
            pageMeta.textContent = error && error.message ? error.message : 'Unknown error';
            tableBody.innerHTML = '';
            emptyState.hidden = false;
            emptyState.textContent = 'The local web server could not load the SQLite records.';
          });
      }

      previousButton.addEventListener('click', function () {
        if (page > 1) {
          page -= 1;
          loadSessions();
        }
      });

      nextButton.addEventListener('click', function () {
        page += 1;
        loadSessions();
      });

      refreshButton.addEventListener('click', function () {
        loadSessions();
      });

      pageSizeSelect.addEventListener('change', function () {
        pageSize = Number(pageSizeSelect.value) || 10;
        page = 1;
        loadSessions();
      });

      loadSessions();
    }());
  </script>
</body>
</html>";
    }
}
