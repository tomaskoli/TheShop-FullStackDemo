using FluentResults;
using Identity.Application.Dtos;
using Identity.Domain.Entities;
using MediatR;
using TheShop.SharedKernel;

namespace Identity.Application.Queries;

public class GetUserQueryHandler : IRequestHandler<GetUserQuery, Result<UserDto>>
{
    private readonly IRepository<ApplicationUser> _userRepository;

    public GetUserQueryHandler(IRepository<ApplicationUser> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<Result<UserDto>> Handle(GetUserQuery request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct);

        if (user is null)
        {
            return Result.Fail<UserDto>("User not found");
        }

        return Result.Ok(new UserDto(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role,
            user.CreatedAt,
            user.IsActive));
    }
}

