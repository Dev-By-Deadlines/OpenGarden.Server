using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using OpenGarden.Server.Data;

namespace OpenGarden.Server.Services;

public class TokenProvider(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    IClock clock)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly IClock _clock = clock;

    public async Task<(string Token, Instant ExpiresAt)> CreateAccessToken(
        ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var signingKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]!));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName!)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var expiresAt = _clock.GetCurrentInstant()
            .Plus(Duration.FromMinutes(
                _configuration.GetValue<int>("Jwt:ExpirationInMinutes")));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt.ToDateTimeUtc(),
            SigningCredentials = credentials,
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var token = new JsonWebTokenHandler().CreateToken(tokenDescriptor);

        return (token, expiresAt);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
