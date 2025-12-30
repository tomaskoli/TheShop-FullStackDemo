using FluentResults;
using MediatR;

namespace Identity.Application.Commands;

public record LogoutCommand(
    string Jti,
    bool LogoutAllDevices = false) : IRequest<Result>;

