namespace Chatbot.Application.Dtos;

public record ChatRequest(string Message, string? ConversationId = null);

public record ChatResponse(
    string Answer,
    string ConversationId,
    IReadOnlyList<SourceReference> Sources);

public record SourceReference(string Type, string Title, string? Url = null);

