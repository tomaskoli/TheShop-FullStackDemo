using FluentAssertions;
using Identity.Application.Commands;
using Identity.Application.Models;
using Identity.Application.Services;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using NSubstitute;
using TheShop.SharedKernel;
using Xunit;

namespace TheShop.UnitTests.Identity;

public class LoginCommandHandlerTests
{
    private readonly IRepository<ApplicationUser> _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly ISessionService _sessionService;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userRepository = Substitute.For<IRepository<ApplicationUser>>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtService = Substitute.For<IJwtService>();
        _sessionService = Substitute.For<ISessionService>();
        _handler = new LoginCommandHandler(_userRepository, _passwordHasher, _jwtService, _sessionService);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var user = ApplicationUser.Create(
            "test@example.com",
            "hashedPassword",
            "John",
            "Doe");

        _userRepository.FindOneAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ApplicationUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher.Verify("password123", "hashedPassword").Returns(true);
        _jwtService.GenerateAccessToken(user).Returns(("access_token", "test-jti"));
        _jwtService.GenerateRefreshToken().Returns("refresh_token");
        _sessionService.CreateSessionAsync(
            Arg.Any<Guid>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<UserRole>(),
            Arg.Any<string>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<CancellationToken>())
            .Returns(new UserSession());

        var command = new LoginCommand("test@example.com", "password123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access_token");
        result.Value.RefreshToken.Should().Be("refresh_token");
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ReturnsFail()
    {
        // Arrange
        _userRepository.FindOneAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ApplicationUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((ApplicationUser?)null);

        var command = new LoginCommand("invalid@example.com", "password123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("Invalid"));
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ReturnsFail()
    {
        // Arrange
        var user = ApplicationUser.Create(
            "test@example.com",
            "hashedPassword",
            "John",
            "Doe");

        _userRepository.FindOneAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ApplicationUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(user);

        _passwordHasher.Verify("wrongpassword", "hashedPassword").Returns(false);

        var command = new LoginCommand("test@example.com", "wrongpassword");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("Invalid"));
    }
}

