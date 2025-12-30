using FluentResults;
using Identity.Application.Dtos;
using MediatR;

namespace Identity.Application.Commands;

public record RefreshTokenCommand(
    string RefreshToken,
    string? OldJti = null) : IRequest<Result<TokenResponse>>;

