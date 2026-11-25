using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using OpenGarden.Server.Data;
using OpenGarden.Server.Models;
using OpenGarden.Server.Utils;

namespace OpenGarden.Server.Services;

public class TokenProvider(
    UserManager<ApplicationUser> userManager,
    IConfiguration configuration,
    IClock clock,
    AppDbContext dbContext)
{
    public async Task<(string Token, Instant ExpiresAt)> CreateAccessToken(
        ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);

        var signingKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]!));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName!)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var expiresAt = clock.GetCurrentInstant().Plus(Duration.FromMinutes(
            configuration.GetValue<int>("Jwt:AccessTokenExpirationInMinutes")));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt.ToDateTimeUtc(),
            SigningCredentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256),
            Issuer = configuration["Jwt:Issuer"],
            Audience = configuration["Jwt:Audience"]
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);

        return (token, expiresAt);
    }

    // Adds a new refresh token to the DbContext (caller must SaveChanges).
    public (string Token, Instant ExpiresAt) CreateRefreshToken(string userId)
    {
        var (raw, expiresAt) = NewRefreshTokenValue();

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = RefreshTokenHasher.Hash(raw),
            Expires = expiresAt
        });

        return (raw, expiresAt);
    }

    // Replaces the token on an existing row (caller must SaveChanges).
    public (string Token, Instant ExpiresAt) RotateRefreshToken(RefreshToken stored)
    {
        var (raw, expiresAt) = NewRefreshTokenValue();

        stored.Token = RefreshTokenHasher.Hash(raw);
        stored.Expires = expiresAt;

        return (raw, expiresAt);
    }

    private (string Raw, Instant ExpiresAt) NewRefreshTokenValue()
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        var expiresAt = clock.GetCurrentInstant().Plus(Duration.FromDays(
            configuration.GetValue<int>("Jwt:RefreshTokenExpirationInDays")));

        return (raw, expiresAt);
    }
}
