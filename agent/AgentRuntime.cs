using System.Text;
using System.Text.Json;

public class AgentRuntime
{
    private readonly HttpClient _http = new();

    public async Task<string> RunAsync()
    {
        var agentPrompt = await File.ReadAllTextAsync("agent/agent.md");

        var messages = new List<object>
        {
            new { role = "system", content = agentPrompt }
        };

        while (true)
        {
            var request = new
            {
                model = "gpt-4.1",
                messages,
                tools = ToolSchemas.All
            };

            var resp = await _http.PostAsync(
                "https://api.openai.com/v1/chat/completions",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
            );

            var json = await resp.Content.ReadAsStringAsync();
            var msg = JsonDocument.Parse(json)
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message");

            if (msg.TryGetProperty("tool_calls", out var calls))
            {
                foreach (var call in calls.EnumerateArray())
                {
                    var name = call.GetProperty("function").GetProperty("name").GetString();
                    var args = call.GetProperty("function").GetProperty("arguments").GetString();

                    var result = await Tools.Execute(name!, args!);

                    messages.Add(msg);
                    messages.Add(new
                    {
                        role = "tool",
                        content = result
                    });
                }
            }
            else
            {
                return msg.GetProperty("content").GetString()!;
            }
        }
    }
}

