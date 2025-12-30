using Identity.Domain.Entities;

namespace Identity.Application.Services;

public interface IJwtService
{
    (string Token, string Jti) GenerateAccessToken(ApplicationUser user);
    string GenerateRefreshToken();
}

