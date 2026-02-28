using System.Text.Json;

public static class ToolSchemas
{
    public static object[] All => new object[]
    {
        GitHubCreatePr,
        JiraCreateTicket
    };

    private static object GitHubCreatePr => new
    {
        type = "function",
        function = new
        {
            name = "github.create_pr",
            description = "Create a pull request in GitHub",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    title = new { type = "string" },
                    branch = new { type = "string" }
                },
                required = new[] { "title", "branch" }
            }
        }
    };

    private static object JiraCreateTicket => new
    {
        type = "function",
        function = new
        {
            name = "jira.create_ticket",
            description = "Create a Jira ticket",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    summary = new { type = "string" }
                },
                required = new[] { "summary" }
            }
        }
    };
}
