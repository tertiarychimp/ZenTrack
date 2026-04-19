using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ZenTrackAgent
{
    internal sealed class WebUiServer : IDisposable
    {
        private readonly TrackerDatabase _database;
        private readonly AdminReportingService _adminReportingService;
        private readonly int _port;
        private readonly TcpListener _listener;
        private bool _listenerStarted;
        private bool _stopRequested;

        public WebUiServer(TrackerDatabase database, GroupDatabase groupDatabase, int port)
        {
            _database = database;
            _adminReportingService = new AdminReportingService(database, groupDatabase);
            _port = port;
            _listener = new TcpListener(IPAddress.Loopback, port);
        }

        public void Run(TimeSpan? maxRuntime)
        {
            Console.CancelKeyPress += OnCancelKeyPress;
            _stopRequested = false;
            _listener.Start();
            _listenerStarted = true;

            Console.WriteLine("ZenTrack web UI started at http://localhost:" + _port + "/");

            System.Threading.Timer shutdownTimer = null;
            if (maxRuntime.HasValue)
            {
                shutdownTimer = new System.Threading.Timer(
                    delegate(object state)
                    {
                        _stopRequested = true;
                        StopListener();
                    },
                    null,
                    maxRuntime.Value,
                    System.Threading.Timeout.InfiniteTimeSpan);
            }

            try
            {
                while (!_stopRequested)
                {
                    if (!_listener.Pending())
                    {
                        System.Threading.Thread.Sleep(100);
                        continue;
                    }

                    TcpClient client = null;
                    try
                    {
                        client = _listener.AcceptTcpClient();
                        HandleClient(client);
                    }
                    catch (SocketException)
                    {
                        if (_stopRequested)
                        {
                            break;
                        }
                    }
                    finally
                    {
                        if (client != null)
                        {
                            client.Close();
                        }
                    }
                }
            }
            finally
            {
                if (shutdownTimer != null)
                {
                    shutdownTimer.Dispose();
                }

                StopListener();
                Console.CancelKeyPress -= OnCancelKeyPress;
                Console.WriteLine("ZenTrack web UI stopped.");
            }
        }

        private void HandleClient(TcpClient client)
        {
            using (NetworkStream stream = client.GetStream())
            using (StreamReader reader = new StreamReader(stream, new UTF8Encoding(false), false, 4096, true))
            using (StreamWriter writer = new StreamWriter(stream, new UTF8Encoding(false), 4096, true))
            {
                writer.NewLine = "\r\n";

                string requestLine = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(requestLine))
                {
                    return;
                }

                string[] requestParts = requestLine.Split(' ');
                if (requestParts.Length < 2)
                {
                    WriteResponse(writer, "400 Bad Request", "text/plain; charset=utf-8", "Bad request");
                    return;
                }

                string method = requestParts[0];
                string rawTarget = requestParts[1];
                Dictionary<string, string> headers = ReadHeaders(reader);

                if (!string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(method, "POST", StringComparison.OrdinalIgnoreCase))
                {
                    WriteResponse(writer, "405 Method Not Allowed", "text/plain; charset=utf-8", "Method not allowed");
                    return;
                }

                string requestBody = ReadBody(reader, headers);
                Uri requestUri = new Uri("http://localhost:" + _port + rawTarget);
                string path = requestUri.AbsolutePath;
                if (path.Length > 1 && path.EndsWith("/", StringComparison.Ordinal))
                {
                    path = path.TrimEnd('/');
                }

                try
                {
                    if (string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
                    {
                        HandleGet(writer, requestUri, path);
                        return;
                    }

                    HandlePost(writer, requestUri, path, requestBody);
                }
                catch (Exception ex)
                {
                    WriteResponse(writer, "500 Internal Server Error", "application/json; charset=utf-8", "{\"error\":\"" + EscapeJson(ex.Message) + "\"}");
                }
            }
        }

        private void HandleGet(StreamWriter writer, Uri requestUri, string path)
        {
            if (string.Equals(path, "/", StringComparison.OrdinalIgnoreCase))
            {
                WriteResponse(writer, "200 OK", "text/html; charset=utf-8", WebUiPage.Html);
                return;
            }

            if (string.Equals(path, "/admin", StringComparison.OrdinalIgnoreCase))
            {
                WriteResponse(writer, "200 OK", "text/html; charset=utf-8", AdminWebUiPage.Html);
                return;
            }

            if (string.Equals(path, "/admin/settings", StringComparison.OrdinalIgnoreCase))
            {
                WriteResponse(writer, "200 OK", "text/html; charset=utf-8", AdminSettingsPage.Html);
                return;
            }

            if (string.Equals(path, "/admin/groups", StringComparison.OrdinalIgnoreCase))
            {
                WriteResponse(writer, "200 OK", "text/html; charset=utf-8", GroupDetailsPage.Html);
                return;
            }

            if (string.Equals(path, "/api/sessions", StringComparison.OrdinalIgnoreCase))
            {
                int page = ParsePositiveOrDefault(GetQueryValue(requestUri.Query, "page"), 1);
                int pageSize = ParsePositiveOrDefault(GetQueryValue(requestUri.Query, "pageSize"), 10);
                UsageSessionPage results = _database.GetUsageSessionsPage(page, pageSize);
                WriteResponse(writer, "200 OK", "application/json; charset=utf-8", JsonResponseWriter.Serialize(results));
                return;
            }

            if (string.Equals(path, "/api/admin/filter-options", StringComparison.OrdinalIgnoreCase))
            {
                AdminFilterOptionsResponse results = _adminReportingService.GetFilterOptions();
                WriteResponse(writer, "200 OK", "application/json; charset=utf-8", JsonResponseWriter.Serialize(results));
                return;
            }

            if (string.Equals(path, "/api/admin/sessions", StringComparison.OrdinalIgnoreCase))
            {
                int page = ParsePositiveOrDefault(GetQueryValue(requestUri.Query, "page"), 1);
                int pageSize = ParsePositiveOrDefault(GetQueryValue(requestUri.Query, "pageSize"), 20);
                string userFilter = GetQueryValue(requestUri.Query, "user");
                string groupFilter = GetQueryValue(requestUri.Query, "group");
                string startDate = GetQueryValue(requestUri.Query, "startDate");
                string endDate = GetQueryValue(requestUri.Query, "endDate");
                AdminUsageSessionPage results = _adminReportingService.GetSessionsPage(page, pageSize, userFilter, groupFilter, startDate, endDate);
                WriteResponse(writer, "200 OK", "application/json; charset=utf-8", JsonResponseWriter.Serialize(results));
                return;
            }

            if (string.Equals(path, "/api/admin/group-summary", StringComparison.OrdinalIgnoreCase))
            {
                string userFilter = GetQueryValue(requestUri.Query, "user");
                string groupFilter = GetQueryValue(requestUri.Query, "group");
                string startDate = GetQueryValue(requestUri.Query, "startDate");
                string endDate = GetQueryValue(requestUri.Query, "endDate");
                AdminGroupSummaryResponse results = _adminReportingService.GetGroupSummary(userFilter, groupFilter, startDate, endDate);
                WriteResponse(writer, "200 OK", "application/json; charset=utf-8", JsonResponseWriter.Serialize(results));
                return;
            }

            if (string.Equals(path, "/api/admin/group-details", StringComparison.OrdinalIgnoreCase))
            {
                string groupName = GetQueryValue(requestUri.Query, "name");
                AdminGroupDetailsResponse result = _adminReportingService.GetGroupDetails(groupName);
                if (result == null)
                {
                    WriteResponse(writer, "404 Not Found", "application/json; charset=utf-8", "{\"error\":\"Group not found.\"}");
                    return;
                }

                WriteResponse(writer, "200 OK", "application/json; charset=utf-8", JsonResponseWriter.Serialize(result));
                return;
            }

            WriteResponse(writer, "404 Not Found", "text/plain; charset=utf-8", "Not found");
        }

        private void HandlePost(StreamWriter writer, Uri requestUri, string path, string requestBody)
        {
            Dictionary<string, List<string>> formValues = ParseFormBody(requestBody);

            if (string.Equals(path, "/api/admin/groups", StringComparison.OrdinalIgnoreCase))
            {
                string groupName = GetFormValue(formValues, "groupName");
                _adminReportingService.CreateGroup(groupName);
                WriteResponse(writer, "200 OK", "application/json; charset=utf-8", JsonResponseWriter.Serialize(new ApiStatusResponse
                {
                    Success = true,
                    Message = "Group saved."
                }));
                return;
            }

            if (string.Equals(path, "/api/admin/group-details", StringComparison.OrdinalIgnoreCase))
            {
                string groupName = GetQueryValue(requestUri.Query, "name");
                string billingReference = GetFormValue(formValues, "billingReference");
                List<string> windowsUsernames = GetFormValues(formValues, "windowsUsername");
                _adminReportingService.SaveGroupDetails(groupName, billingReference, windowsUsernames);
                WriteResponse(writer, "200 OK", "application/json; charset=utf-8", JsonResponseWriter.Serialize(new ApiStatusResponse
                {
                    Success = true,
                    Message = "Group details saved."
                }));
                return;
            }

            WriteResponse(writer, "404 Not Found", "text/plain; charset=utf-8", "Not found");
        }

        private static Dictionary<string, string> ReadHeaders(StreamReader reader)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            while (true)
            {
                string headerLine = reader.ReadLine();
                if (string.IsNullOrEmpty(headerLine))
                {
                    break;
                }

                int separatorIndex = headerLine.IndexOf(':');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string headerName = headerLine.Substring(0, separatorIndex).Trim();
                string headerValue = headerLine.Substring(separatorIndex + 1).Trim();
                headers[headerName] = headerValue;
            }

            return headers;
        }

        private static string ReadBody(StreamReader reader, Dictionary<string, string> headers)
        {
            string contentLengthHeader;
            if (!headers.TryGetValue("Content-Length", out contentLengthHeader))
            {
                return string.Empty;
            }

            int contentLength;
            if (!int.TryParse(contentLengthHeader, out contentLength) || contentLength <= 0)
            {
                return string.Empty;
            }

            char[] buffer = new char[contentLength];
            int totalRead = 0;
            while (totalRead < contentLength)
            {
                int read = reader.Read(buffer, totalRead, contentLength - totalRead);
                if (read <= 0)
                {
                    break;
                }

                totalRead += read;
            }

            return new string(buffer, 0, totalRead);
        }

        private static Dictionary<string, List<string>> ParseFormBody(string body)
        {
            Dictionary<string, List<string>> values = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(body))
            {
                return values;
            }

            string[] pairs = body.Split('&');
            foreach (string pair in pairs)
            {
                if (string.IsNullOrEmpty(pair))
                {
                    continue;
                }

                string[] parts = pair.Split(new[] { '=' }, 2);
                string key = Uri.UnescapeDataString(parts[0].Replace("+", " "));
                string value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1].Replace("+", " ")) : string.Empty;

                List<string> list;
                if (!values.TryGetValue(key, out list))
                {
                    list = new List<string>();
                    values[key] = list;
                }

                list.Add(value);
            }

            return values;
        }

        private static string GetFormValue(Dictionary<string, List<string>> values, string key)
        {
            List<string> matches;
            if (!values.TryGetValue(key, out matches) || matches.Count == 0)
            {
                return null;
            }

            return matches[0];
        }

        private static List<string> GetFormValues(Dictionary<string, List<string>> values, string key)
        {
            List<string> matches;
            if (!values.TryGetValue(key, out matches))
            {
                return new List<string>();
            }

            return matches;
        }

        private static string GetQueryValue(string query, string key)
        {
            if (string.IsNullOrEmpty(query))
            {
                return null;
            }

            string trimmed = query.TrimStart('?');
            string[] pairs = trimmed.Split('&');
            foreach (string pair in pairs)
            {
                string[] parts = pair.Split(new[] { '=' }, 2);
                if (parts.Length == 2 && string.Equals(parts[0], key, StringComparison.OrdinalIgnoreCase))
                {
                    return Uri.UnescapeDataString(parts[1]);
                }
            }

            return null;
        }

        private static int ParsePositiveOrDefault(string value, int fallback)
        {
            int parsedValue;
            if (!int.TryParse(value, out parsedValue) || parsedValue <= 0)
            {
                return fallback;
            }

            return parsedValue;
        }

        private static void WriteResponse(StreamWriter writer, string status, string contentType, string body)
        {
            byte[] bodyBytes = Encoding.UTF8.GetBytes(body ?? string.Empty);

            writer.WriteLine("HTTP/1.1 " + status);
            writer.WriteLine("Content-Type: " + contentType);
            writer.WriteLine("Content-Length: " + bodyBytes.Length);
            writer.WriteLine("Cache-Control: no-store");
            writer.WriteLine("Connection: close");
            writer.WriteLine();
            writer.Flush();

            Stream baseStream = writer.BaseStream;
            baseStream.Write(bodyBytes, 0, bodyBytes.Length);
            baseStream.Flush();
        }

        private static string EscapeJson(string value)
        {
            return (value ?? string.Empty)
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n");
        }

        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;
            _stopRequested = true;
            StopListener();
        }

        private void StopListener()
        {
            if (_listenerStarted)
            {
                _listener.Stop();
                _listenerStarted = false;
            }
        }

        public void Dispose()
        {
            StopListener();
        }
    }
}
