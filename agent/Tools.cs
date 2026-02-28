using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public static class Tools
{
    private static readonly HttpClient _http = new HttpClient();

    public static async Task<string> Execute(string name, string args)
    {
        switch (name)
        {
            case "github.create_pr":
                // TODO: replace URL and payload with actual values
                var prResp = await _http.PostAsync(
                    "https://api.github.com/repos/OWNER/REPO/pulls",
                    new StringContent(args, Encoding.UTF8, "application/json")
                );
                return await prResp.Content.ReadAsStringAsync();

            case "jira.create_ticket":
                // TODO: replace URL and payload with actual values
                var jiraResp = await _http.PostAsync(
                    "https://your-domain.atlassian.net/rest/api/3/issue",
                    new StringContent(args, Encoding.UTF8, "application/json")
                );
                return await jiraResp.Content.ReadAsStringAsync();

            default:
                return "Unknown tool: " + name;
        }
    }
}
