using Chatbot.Application.Dtos;

namespace Chatbot.Application.Services;

public interface IChatbotService
{
    Task<ChatResponse> ChatAsync(string message, string? conversationId, CancellationToken ct = default);
}

