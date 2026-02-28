using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

public static class Tools
{
   private static readonly HttpClient _http = new HttpClient();

   public static async Task<string> Execute(string toolName, string args)
   {
    switch (toolName)
    {
        case "github.create_pr":
            var githubToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
            var payload = new StringContent(args, Encoding.UTF8, "application/json");
            var req = new HttpRequestMessage(HttpMethod.Post,
                "https://api.github.com/repos/OWNER/REPO/pulls");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", githubToken);
            req.Headers.Add("User-Agent", "AgentRunner");
            req.Content = payload;
            var resp = await _http.SendAsync(req);
            return await resp.Content.ReadAsStringAsync();

        case "jira.create_ticket":
            var jiraToken = Environment.GetEnvironmentVariable("JIRA_API_TOKEN");
            var jiraBase  = Environment.GetEnvironmentVariable("JIRA_BASE_URL");
            var jiraPayload = new StringContent(args, Encoding.UTF8, "application/json");
            var jiraReq = new HttpRequestMessage(HttpMethod.Post, $"{jiraBase}/rest/api/3/issue");
            jiraReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Basic", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"email:{jiraToken}"))
            );
            jiraReq.Content = jiraPayload;
            var jiraResp = await _http.SendAsync(jiraReq);
            return await jiraResp.Content.ReadAsStringAsync();

        default:
            return $"Unknown tool {toolName}";
    }
  }	

}
