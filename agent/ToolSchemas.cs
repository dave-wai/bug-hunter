public static class ToolSchemas
{
    public static object[] All =>
    [
        GitHubListRepos,
        JiraGetPageContent
    ];

    private static object GitHubListRepos => new
    {
        type = "function",
        function = new
        {
            name = "github.list_repos",
            description = "List repositories for the authenticated user, including public and private repositories.",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    visibility = new
                    {
                        type = "string",
                        @enum = new[] { "all", "public", "private" },
                        description = "Use 'all' to include both public and private repositories."
                    },
                    per_page = new
                    {
                        type = "integer",
                        minimum = 1,
                        maximum = 100,
                        description = "Items per page."
                    },
                    page = new
                    {
                        type = "integer",
                        minimum = 1,
                        description = "Page number."
                    }
                }
            }
        }
    };

    private static object JiraGetPageContent => new
    {
        type = "function",
        function = new
        {
            name = "jira.get_page_content",
            description = "Get content for a Jira page.",
            parameters = new
            {
                type = "object",
                properties = new
                {
                    page_id = new
                    {
                        type = "string",
                        description = "Jira page ID."
                    },
                    format = new
                    {
                        type = "string",
                        @enum = new[] { "storage", "rendered", "text" },
                        description = "Preferred content format."
                    }
                },
                required = new[] { "page_id" }
            }
        }
    };
}