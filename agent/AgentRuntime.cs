using System.Text;
using System.Text.Json;

public class AgentRuntime
{
    private readonly HttpClient _httpClient = new();

    public async Task<string> RunAsync()
    {
        string openAIEndpoint = "https://testdocumentationwithai.openai.azure.com/openai/deployments/gpt-4.1-mini/chat/completions?api-version=2025-01-01-preview";
        string openAIapiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");

        var projectRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

        var agentPromptPath = Path.Combine(projectRoot, "Github-Jira.agent.md");

        var agentPrompt = await File.ReadAllTextAsync(agentPromptPath);

        var messages = new List<Dictionary<string, object?>>
        {
            new()
            {
                ["role"] = "system",
                ["content"] = "You are a github and Confluence page reader. Use the instructions provided by the user."
            },
            new()
            {
                ["role"] = "user",
                ["content"] = $"{agentPrompt}\n\nConfluence pageId = \"753665\".\nList all public repositories I have in github."
            }
        };

        while (true)
        {
            var requestBody = new Dictionary<string, object?>
            {
                ["messages"] = messages,
                ["tools"] = ToolSchemas.All,
                ["tool_choice"] = "auto",
                ["max_tokens"] = 2000,
                ["temperature"] = 0.2
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, openAIEndpoint)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json")
            };
            
            request.Headers.Add("api-key", openAIapiKey);

            using var response = await _httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Azure OpenAI call failed: {(int)response.StatusCode} {response.ReasonPhrase}\n{responseContent}");
            }

            using var jsonDoc = JsonDocument.Parse(responseContent);
            var assistantMessage = jsonDoc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message");

            if (assistantMessage.TryGetProperty("tool_calls", out var toolCalls) &&
                toolCalls.ValueKind == JsonValueKind.Array &&
                toolCalls.GetArrayLength() > 0)
            {
                messages.Add(new Dictionary<string, object?>
                {
                    ["role"] = "assistant",
                    ["content"] = assistantMessage.TryGetProperty("content", out var assistantContent) &&
                                  assistantContent.ValueKind != JsonValueKind.Null
                        ? assistantContent.GetString()
                        : null,
                    ["tool_calls"] = toolCalls.Clone()
                });

                foreach (var call in toolCalls.EnumerateArray())
                {
                    var toolCallId = call.GetProperty("id").GetString() ?? string.Empty;
                    var function = call.GetProperty("function");
                    var name = function.GetProperty("name").GetString() ?? string.Empty;
                    var args = function.TryGetProperty("arguments", out var argsElement)
                        ? (argsElement.GetString() ?? "{}")
                        : "{}";

                    var result = await Tools.Execute(name, args);

                    messages.Add(new Dictionary<string, object?>
                    {
                        ["role"] = "tool",
                        ["tool_call_id"] = toolCallId,
                        ["content"] = result
                    });
                }

                continue;
            }

            if (assistantMessage.TryGetProperty("content", out var finalContent) &&
                finalContent.ValueKind != JsonValueKind.Null)
            {   
                return finalContent.GetString() ?? string.Empty;
            }

            throw new InvalidOperationException("Model returned no content and no tool calls.");
        }

      }

    }

