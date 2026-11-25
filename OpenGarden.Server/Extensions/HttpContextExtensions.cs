using NodaTime;

namespace OpenGarden.Server.Extensions;

public static class HttpContextExtensions
{
    private const string AccessTokenCookieName = "accessToken";
    private const string RefreshTokenCookieName = "refreshToken";

    private static CookieOptions CookieOptions(Instant expiresAt) => new()
    {
        Secure = true,
        SameSite = SameSiteMode.Strict,
        Path = "/",
        HttpOnly = true,
        Expires = expiresAt.ToDateTimeOffset()
    };

    public static void AppendAccessTokenCookie(
        this HttpContext httpContext,
        string token,
        Instant expiresAt)
    {
        httpContext.Response.Cookies.Append(
            AccessTokenCookieName,
            token,
            CookieOptions(expiresAt));
    }

    public static void AppendRefreshTokenCookie(
        this HttpContext httpContext,
        string token,
        Instant expiresAt)
    {
        httpContext.Response.Cookies.Append(
            RefreshTokenCookieName,
            token,
            CookieOptions(expiresAt));
    }

    public static void AppendAuthCookies(
        this HttpContext httpContext,
        (string Token, Instant ExpiresAt) access,
        (string Token, Instant ExpiresAt) refresh)
    {
        httpContext.AppendAccessTokenCookie(access.Token, access.ExpiresAt);
        httpContext.AppendRefreshTokenCookie(refresh.Token, refresh.ExpiresAt);
    }

    public static void DeleteAuthCookies(this HttpContext httpContext)
    {
        var options = new CookieOptions
        {
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            HttpOnly = true
        };

        httpContext.Response.Cookies.Delete(AccessTokenCookieName, options);
        httpContext.Response.Cookies.Delete(RefreshTokenCookieName, options);
    }

    public static string? GetRefreshToken(this HttpContext httpContext)
    {
        return httpContext.Request.Cookies[RefreshTokenCookieName];
    }
}
