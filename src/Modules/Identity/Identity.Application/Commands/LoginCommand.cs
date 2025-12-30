using FluentResults;
using Identity.Application.Dtos;
using MediatR;

namespace Identity.Application.Commands;

public record LoginCommand(
    string Email,
    string Password,
    string? DeviceInfo = null,
    string? IpAddress = null) : IRequest<Result<TokenResponse>>;

