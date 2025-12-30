using Chatbot.Application.Dtos;
using Chatbot.Application.Services;
using FluentResults;
using MediatR;

namespace Chatbot.Application.Commands;

public class ChatCommandHandler : IRequestHandler<ChatCommand, Result<ChatResponse>>
{
    private readonly IChatbotService _chatbotService;

    public ChatCommandHandler(IChatbotService chatbotService)
    {
        _chatbotService = chatbotService;
    }

    public async Task<Result<ChatResponse>> Handle(ChatCommand request, CancellationToken ct)
    {
        try
        {
            var response = await _chatbotService.ChatAsync(request.Message, request.ConversationId, ct);
            return Result.Ok(response);
        }
        catch (Exception ex)
        {
            return Result.Fail<ChatResponse>($"Chat service error: {ex.Message}");
        }
    }
}

