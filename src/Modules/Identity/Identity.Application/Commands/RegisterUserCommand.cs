using FluentResults;
using Identity.Application.Dtos;
using MediatR;

namespace Identity.Application.Commands;

public record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName) : IRequest<Result<UserDto>>;

