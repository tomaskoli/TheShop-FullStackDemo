using Chatbot.Application.Dtos;
using Chatbot.Application.Services;
using Chatbot.Infrastructure.Settings;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Concurrent;

namespace Chatbot.Infrastructure.Services;

public class ChatbotService : IChatbotService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chatCompletion;
    private readonly string _systemPrompt;
    private static readonly ConcurrentDictionary<string, ChatHistory> _conversations = new();

    public ChatbotService(Kernel kernel, IOptions<ChatbotSettings> settings)
    {
        _kernel = kernel;
        _chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
        _systemPrompt = settings.Value.SystemPrompt;
    }

    public async Task<ChatResponse> ChatAsync(string message, string? conversationId, CancellationToken ct)
    {
        conversationId ??= Guid.NewGuid().ToString();
        
        var history = _conversations.GetOrAdd(conversationId, _ => new ChatHistory(_systemPrompt));

        history.AddUserMessage(message);

        var settings = new PromptExecutionSettings
        {
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };

        var response = await _chatCompletion.GetChatMessageContentAsync(history, settings, _kernel, ct);
        
        var responseContent = response.Content ?? string.Empty;
        history.AddAssistantMessage(responseContent);

        var sources = ExtractSources(history);

        return new ChatResponse(responseContent, conversationId, sources);
    }

    private static List<SourceReference> ExtractSources(ChatHistory history)
    {
        var sources = new List<SourceReference>();
        
        foreach (var msg in history.Where(m => m.Role == AuthorRole.Tool))
        {
            if (msg.Metadata?.TryGetValue("ChatCompletionsFunctionToolCall.Name", out var toolName) == true)
            {
                var name = toolName?.ToString() ?? "Unknown";
                if (name.Contains("DocumentSearch"))
                {
                    sources.Add(new SourceReference("Document", "Shop Knowledge Base"));
                }
                else if (name.Contains("ProductGraph"))
                {
                    sources.Add(new SourceReference("Graph", "Product Database"));
                }
            }
        }

        return sources.DistinctBy(s => s.Title).ToList();
    }
}

