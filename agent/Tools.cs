using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

public static class Tools
{
    private static readonly HttpClient _http = new HttpClient();

    public static async Task<string> Execute(string toolName, string args)
    {
        switch (toolName)
        {
            case "github.list_repos":
                var githubToken = Environment.GetEnvironmentVariable("MYGITHUB_TOKEN");
                var payload = new StringContent(args, Encoding.UTF8, "application/json");
                var req = new HttpRequestMessage(HttpMethod.Get,
                    "https://api.github.com/users/dave-wai/repos");
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", githubToken);
                req.Headers.Add("User-Agent", "AgentRunner");
                req.Content = payload;
                var resp = await _http.SendAsync(req);
                return await resp.Content.ReadAsStringAsync();

            case "jira.get_page_content":
            case "confluence.get_page_content":

                var confluenceBase = "https://davewai.atlassian.net";
                var confluenceEmail = Environment.GetEnvironmentVariable("CONFLUENCE_EMAIL");
                var confluenceApiToken = Environment.GetEnvironmentVariable("CONFLUENCE_API_TOKEN");

                var pageId = 753665.ToString();
                if (string.IsNullOrWhiteSpace(pageId))
                {
                    return "Missing pageId. Pass args as plain page id or JSON: {\"pageId\":\"753665\"}.";
                }

                var confluenceUrl =
                    $"{confluenceBase}/wiki/rest/api/content/{Uri.EscapeDataString(pageId)}?expand=body.storage";

                var confluenceReq = new HttpRequestMessage(HttpMethod.Get, confluenceUrl);
                var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{confluenceEmail}:{confluenceApiToken}"));
                confluenceReq.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
                confluenceReq.Headers.Accept.ParseAdd("application/json");
                confluenceReq.Headers.Add("User-Agent", "AgentRunner");

                var confluenceResp = await _http.SendAsync(confluenceReq);
                var confluenceJson = await confluenceResp.Content.ReadAsStringAsync();

                if (!confluenceResp.IsSuccessStatusCode)
                {
                    return confluenceJson;
                }

                return ExtractConfluenceTitleAndStorageValue(confluenceJson);

            default:
                return $"Unknown tool {toolName}";
        }
      
    }


    //private static string ExtractPageId(string args)
    //{
    //    if (string.IsNullOrWhiteSpace(args)) return string.Empty;

    //    var raw = args.Trim().Trim('"');

    //    if (!raw.StartsWith("{"))
    //    {
    //        return raw;
    //    }

    //    try
    //    {
    //        using var doc = JsonDocument.Parse(raw);
    //        if (doc.RootElement.TryGetProperty("pageId", out var pageIdProp))
    //        {
    //            return pageIdProp.GetString() ?? string.Empty;
    //        }

    //        if (doc.RootElement.TryGetProperty("id", out var idProp))
    //        {
    //            return idProp.GetString() ?? string.Empty;
    //        }
    //    }
    //    catch
    //    {
    //        // Fall back to raw value if JSON parsing fails.
    //    }

    //    return raw;
    //}

    private static string ExtractConfluenceTitleAndStorageValue(string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return json;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var title = root.TryGetProperty("title", out var titleProp)
                ? titleProp.GetString() ?? string.Empty
                : string.Empty;

            string value = string.Empty;
            if (root.TryGetProperty("body", out var bodyProp) &&
                bodyProp.ValueKind == JsonValueKind.Object &&
                bodyProp.TryGetProperty("storage", out var storageProp) &&
                storageProp.ValueKind == JsonValueKind.Object &&
                storageProp.TryGetProperty("value", out var valueProp))
            {
                value = valueProp.GetString() ?? string.Empty;
            }

            return JsonSerializer.Serialize(new
            {
                title,
                value
            });
        }
        catch
        {
            return json;
        }
    }

}