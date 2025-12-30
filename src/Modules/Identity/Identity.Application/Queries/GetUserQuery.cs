using FluentResults;
using Identity.Application.Dtos;
using MediatR;

namespace Identity.Application.Queries;

public record GetUserQuery(Guid UserId) : IRequest<Result<UserDto>>;

