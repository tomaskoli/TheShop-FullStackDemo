using Chatbot.Application.Dtos;
using FluentResults;
using MediatR;

namespace Chatbot.Application.Commands;

public record ChatCommand(string Message, string? ConversationId) : IRequest<Result<ChatResponse>>;

