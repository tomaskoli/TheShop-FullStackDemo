using FluentResults;
using Identity.Application.Dtos;
using Identity.Application.Services;
using Identity.Domain.Entities;
using MediatR;
using TheShop.SharedKernel;

namespace Identity.Application.Commands;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<UserDto>>
{
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(
        IRepository<ApplicationUser> userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        var emailExists = await _userRepository.ExistsAsync(u => u.Email == request.Email, ct);
        if (emailExists)
        {
            return Result.Fail<UserDto>("User with this email already exists");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = ApplicationUser.Create(
            request.Email,
            passwordHash,
            request.FirstName,
            request.LastName);

        await _userRepository.AddAsync(user, ct);
        await _userRepository.SaveChangesAsync(ct);

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

