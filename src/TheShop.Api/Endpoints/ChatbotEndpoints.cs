using Chatbot.Application.Commands;
using Chatbot.Application.Dtos;
using MediatR;

namespace TheShop.Api.Endpoints;

public static class ChatbotEndpoints
{
    public static void MapChatbotEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/chatbot")
            .WithTags("Chatbot")
            .RequireRateLimiting("Global");

        group.MapPost("/chat", Chat)
            .AllowAnonymous()
            .Produces<ChatResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapDelete("/conversations/{conversationId}", ClearConversation)
            .AllowAnonymous()
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> Chat(
        ChatRequest request,
        ISender mediator,
        CancellationToken ct)
    {
        var command = new ChatCommand(request.Message, request.ConversationId);
        var result = await mediator.Send(command, ct);

        return result.IsSuccess
            ? Results.Ok(result.Value)
            : Results.BadRequest(result.Errors.Select(e => e.Message));
    }

    private static IResult ClearConversation(string conversationId)
    {
        // Conversation cleanup handled by service
        return Results.NoContent();
    }
}

