namespace ZenTrackAgent
{
    internal static class GroupDetailsPage
    {
        public const string Html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>ZenTrack Group Properties</title>
  <style>
    :root {
      color-scheme: light;
      --bg: #edf4ff;
      --panel: rgba(255, 255, 255, 0.94);
      --ink: #14324f;
      --muted: #5f7993;
      --line: #c7daf5;
      --accent: #1c61b2;
      --accent-soft: #dfebfc;
      --shadow: 0 18px 44px rgba(31, 80, 136, 0.14);
    }

    * {
      box-sizing: border-box;
    }

    body {
      margin: 0;
      font-family: ""Segoe UI"", Tahoma, sans-serif;
      color: var(--ink);
      background:
        radial-gradient(circle at top left, rgba(83, 148, 231, 0.18), transparent 28%),
        linear-gradient(180deg, #f7fbff 0%, var(--bg) 100%);
    }

    .shell {
      max-width: 1040px;
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
      font-size: clamp(28px, 4vw, 40px);
      line-height: 1;
      letter-spacing: -0.03em;
    }

    .subtitle {
      margin: 10px 0 0;
      color: var(--muted);
      max-width: 760px;
      line-height: 1.5;
    }

    .card {
      background: var(--panel);
      border: 1px solid rgba(199, 218, 245, 0.95);
      border-radius: 22px;
      box-shadow: var(--shadow);
      padding: 20px;
      margin-bottom: 18px;
      backdrop-filter: blur(10px);
    }

    .status {
      color: var(--muted);
      margin-bottom: 14px;
      line-height: 1.5;
    }

    .field {
      display: grid;
      gap: 6px;
      margin-bottom: 18px;
    }

    .field label {
      font-size: 12px;
      letter-spacing: 0.08em;
      text-transform: uppercase;
      font-weight: 700;
      color: var(--muted);
    }

    input[type=""text""] {
      width: 100%;
      min-height: 44px;
      border: 1px solid var(--line);
      border-radius: 14px;
      padding: 10px 12px;
      font: inherit;
      color: var(--ink);
      background: #fff;
    }

    .chip-list {
      display: flex;
      flex-wrap: wrap;
      gap: 10px;
      min-height: 20px;
    }

    .chip {
      display: inline-flex;
      align-items: center;
      border-radius: 999px;
      padding: 7px 12px;
      background: var(--accent-soft);
      color: var(--accent);
      font-size: 13px;
      font-weight: 700;
    }

    .checkbox-grid {
      display: grid;
      gap: 10px;
      max-height: 320px;
      overflow-y: auto;
      padding: 14px;
      border: 1px solid var(--line);
      border-radius: 16px;
      background: #fff;
    }

    .checkbox-item {
      display: flex;
      align-items: center;
      gap: 10px;
      color: var(--ink);
    }

    .checkbox-item input {
      width: 18px;
      height: 18px;
    }

    .empty {
      color: var(--muted);
      font-size: 14px;
    }

    .actions,
    .nav-links {
      display: flex;
      flex-wrap: wrap;
      gap: 10px;
      align-items: center;
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
      font: inherit;
      font-size: 14px;
      font-weight: 700;
      text-decoration: none;
      cursor: pointer;
    }

    button.primary,
    a.button-link.primary {
      color: #fff;
      background: var(--accent);
    }

    button.secondary,
    a.button-link.secondary {
      color: var(--accent);
      background: var(--accent-soft);
    }

    @media (max-width: 760px) {
      .shell {
        padding: 20px 12px 28px;
      }

      .hero {
        flex-direction: column;
        align-items: start;
      }
    }
  </style>
</head>
<body>
  <div class=""shell"">
    <section class=""hero"">
      <div>
        <div class=""eyebrow"">ZenTrack Group Administration</div>
        <h1 id=""pageTitle"">Group Properties</h1>
        <p class=""subtitle"">Set the group billing reference and associate any currently unassigned users with this group.</p>
      </div>
      <div class=""nav-links"">
        <a class=""button-link secondary"" href=""/admin"">Back To Reporting</a>
        <a class=""button-link secondary"" href=""/admin/settings"">Agent Settings</a>
      </div>
    </section>

    <section class=""card"">
      <div id=""status"" class=""status"">Loading group details...</div>

      <div class=""field"">
        <label for=""billingReference"">Billing Reference</label>
        <input id=""billingReference"" type=""text"" placeholder=""Enter a billing reference"" />
      </div>

      <div class=""field"">
        <label>Current Members</label>
        <div id=""currentMembers"" class=""chip-list""></div>
      </div>

      <div class=""field"">
        <label>Currently Unassigned Users</label>
        <div id=""unassignedUsers"" class=""checkbox-grid""></div>
      </div>

      <div class=""actions"">
        <button id=""saveButton"" class=""primary"" type=""button"">Save Group</button>
        <a class=""button-link secondary"" href=""/admin"">Cancel</a>
      </div>
    </section>
  </div>

  <script>
    (function () {
      var pageTitle = document.getElementById('pageTitle');
      var status = document.getElementById('status');
      var billingReference = document.getElementById('billingReference');
      var currentMembers = document.getElementById('currentMembers');
      var unassignedUsers = document.getElementById('unassignedUsers');
      var saveButton = document.getElementById('saveButton');
      var groupName = new URLSearchParams(window.location.search).get('name') || '';

      function escapeHtml(value) {
        return String(value == null ? '' : value)
          .replace(/&/g, '&amp;')
          .replace(/</g, '&lt;')
          .replace(/>/g, '&gt;')
          .replace(/""/g, '&quot;')
          .replace(/'/g, '&#39;');
      }

      function renderMembers(items) {
        if (!items || !items.length) {
          currentMembers.innerHTML = '<div class=""empty"">No users are assigned to this group yet.</div>';
          return;
        }

        currentMembers.innerHTML = items.map(function (item) {
          return '<span class=""chip"">' + escapeHtml(item.label) + '</span>';
        }).join('');
      }

      function renderUnassigned(items) {
        if (!items || !items.length) {
          unassignedUsers.innerHTML = '<div class=""empty"">There are no unassigned users available right now.</div>';
          return;
        }

        unassignedUsers.innerHTML = items.map(function (item, index) {
          var id = 'user-' + index;
          return '<label class=""checkbox-item"" for=""' + id + '"">' +
            '<input id=""' + id + '"" type=""checkbox"" value=""' + escapeHtml(item.value) + '"" />' +
            '<span>' + escapeHtml(item.label) + '</span>' +
            '</label>';
        }).join('');
      }

      function loadGroup() {
        if (!groupName) {
          status.textContent = 'No group name was provided.';
          saveButton.disabled = true;
          return;
        }

        pageTitle.textContent = groupName + ' Group Properties';
        status.textContent = 'Loading group details...';
        fetch('/api/admin/group-details?name=' + encodeURIComponent(groupName), { cache: 'no-store' })
          .then(function (response) {
            if (response.status === 404) {
              throw new Error('Group not found.');
            }

            return response.json();
          })
          .then(function (data) {
            billingReference.value = data.billingReference || '';
            renderMembers(data.currentMembers || []);
            renderUnassigned(data.unassignedUsers || []);
            status.textContent = 'Editing group: ' + data.groupName;
          })
          .catch(function (error) {
            status.textContent = error && error.message ? error.message : 'Could not load the group details.';
            saveButton.disabled = true;
          });
      }

      saveButton.addEventListener('click', function () {
        var selectedUsers = Array.prototype.slice.call(unassignedUsers.querySelectorAll('input[type=""checkbox""]:checked'))
          .map(function (item) { return item.value; });
        var body = 'billingReference=' + encodeURIComponent(billingReference.value || '');

        for (var i = 0; i < selectedUsers.length; i += 1) {
          body += '&windowsUsername=' + encodeURIComponent(selectedUsers[i]);
        }

        saveButton.disabled = true;
        status.textContent = 'Saving group details...';
        fetch('/api/admin/group-details?name=' + encodeURIComponent(groupName), {
          method: 'POST',
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8'
          },
          body: body
        })
          .then(function (response) { return response.json(); })
          .then(function (data) {
            if (!data.success) {
              throw new Error(data.message || 'The group changes could not be saved.');
            }

            status.textContent = data.message || 'Group saved.';
            saveButton.disabled = false;
            loadGroup();
          })
          .catch(function (error) {
            status.textContent = error && error.message ? error.message : 'The group changes could not be saved.';
            saveButton.disabled = false;
          });
      });

      loadGroup();
    }());
  </script>
</body>
</html>";
    }
}
