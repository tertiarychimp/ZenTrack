namespace ZenTrackAgent
{
    internal static class AdminSettingsPage
    {
        public const string Html = @"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
  <title>ZenTrack Agent Settings</title>
  <style>
    :root {
      color-scheme: light;
      --bg: #edf4ff;
      --panel: rgba(255, 255, 255, 0.92);
      --ink: #14324f;
      --muted: #56718c;
      --line: #c4d8f2;
      --accent: #1f5da8;
      --accent-soft: #dceafd;
      --shadow: 0 18px 42px rgba(33, 79, 136, 0.14);
    }

    * {
      box-sizing: border-box;
    }

    body {
      margin: 0;
      font-family: ""Segoe UI"", Tahoma, sans-serif;
      color: var(--ink);
      background:
        radial-gradient(circle at top left, rgba(79, 137, 214, 0.18), transparent 28%),
        linear-gradient(180deg, #f8fbff 0%, var(--bg) 100%);
    }

    .shell {
      max-width: 1100px;
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
      max-width: 780px;
      line-height: 1.5;
    }

    .card {
      background: var(--panel);
      border: 1px solid rgba(196, 216, 242, 0.95);
      border-radius: 22px;
      box-shadow: var(--shadow);
      padding: 20px;
      margin-bottom: 18px;
      backdrop-filter: blur(8px);
    }

    .links {
      display: flex;
      flex-wrap: wrap;
      gap: 10px;
      margin-top: 16px;
    }

    .button-link {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-height: 40px;
      padding: 10px 16px;
      border-radius: 999px;
      text-decoration: none;
      font-size: 14px;
      font-weight: 700;
      color: #fff;
      background: var(--accent);
    }

    .button-link.secondary {
      color: var(--accent);
      background: var(--accent-soft);
    }

    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 16px;
    }

    .panel-title {
      margin: 0 0 6px;
      font-size: 18px;
    }

    .panel-copy {
      margin: 0;
      color: var(--muted);
      line-height: 1.5;
    }

    .field {
      display: grid;
      gap: 6px;
      margin-top: 16px;
    }

    .field label {
      font-size: 13px;
      font-weight: 700;
      color: var(--muted);
      text-transform: uppercase;
      letter-spacing: 0.06em;
    }

    .field input,
    .field textarea {
      width: 100%;
      border: 1px solid var(--line);
      border-radius: 14px;
      padding: 11px 13px;
      font: inherit;
      color: var(--ink);
      background: #fff;
    }

    .field textarea {
      min-height: 90px;
      resize: vertical;
    }

    .coming-soon {
      display: inline-flex;
      align-items: center;
      border-radius: 999px;
      padding: 6px 10px;
      font-size: 12px;
      font-weight: 700;
      background: var(--accent-soft);
      color: var(--accent);
    }

    .note {
      margin-top: 16px;
      color: var(--muted);
      line-height: 1.5;
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
        <div class=""eyebrow"">ZenTrack Administration</div>
        <h1>ZenTrack Agent Settings</h1>
        <p class=""subtitle"">This page is the administration landing area for future ZenTrackAgent configuration. It is linked from the reporting portal now so settings, sync controls, and billing outputs can live in one place as the master system grows.</p>
      </div>
      <span class=""coming-soon"">Settings save workflow coming next</span>
    </section>

    <section class=""card"">
      <div class=""grid"">
        <div>
          <h2 class=""panel-title"">Local Agent Tracking</h2>
          <p class=""panel-copy"">Manage monitored process names, polling interval, and local capture behavior.</p>
          <div class=""field"">
            <label for=""processNames"">Monitored Process Names</label>
            <textarea id=""processNames"" placeholder=""ZEN.exe""></textarea>
          </div>
          <div class=""field"">
            <label for=""pollInterval"">Poll Interval Seconds</label>
            <input id=""pollInterval"" type=""number"" min=""1"" value=""60"" />
          </div>
        </div>

        <div>
          <h2 class=""panel-title"">MySQL Master Sync</h2>
          <p class=""panel-copy"">Future sync settings for the master reporting database live here.</p>
          <div class=""field"">
            <label for=""mysqlServer"">MySQL Server</label>
            <input id=""mysqlServer"" type=""text"" placeholder=""mysql.example.local"" />
          </div>
          <div class=""field"">
            <label for=""mysqlDatabase"">Database Name</label>
            <input id=""mysqlDatabase"" type=""text"" placeholder=""zentrack_master"" />
          </div>
        </div>

        <div>
          <h2 class=""panel-title"">Groups And Billing</h2>
          <p class=""panel-copy"">Group assignment, billing rules, and PDF report configuration will be managed from the same administration space.</p>
          <div class=""field"">
            <label for=""billingPrefix"">Invoice Prefix</label>
            <input id=""billingPrefix"" type=""text"" placeholder=""ZEN-BILL"" />
          </div>
          <div class=""field"">
            <label for=""pdfNotes"">PDF Report Notes</label>
            <textarea id=""pdfNotes"" placeholder=""Internal notes for future PDF billing exports.""></textarea>
          </div>
        </div>
      </div>

      <p class=""note"">This first pass creates the HTML entry point and information architecture. Save actions and live configuration persistence are not wired yet.</p>

      <div class=""links"">
        <a class=""button-link"" href=""/admin"">Back To Admin Reporting</a>
        <a class=""button-link secondary"" href=""/"">Open Local Viewer</a>
      </div>
    </section>
  </div>
</body>
</html>";
    }
}
