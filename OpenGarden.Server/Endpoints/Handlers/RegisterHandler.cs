using FluentValidation;
using Microsoft.AspNetCore.Identity;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;
using OpenGarden.Server.Extensions;
using OpenGarden.Server.Services;

namespace OpenGarden.Server.Endpoints.Handlers;

public class RegisterHandler
{
    public static async Task<IResult> Handler(
        RegisterDto registerDto,
        AppDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        IValidator<RegisterDto> validator,
        TokenProvider tokenProvider,
        HttpContext httpContext)
    {
        var validationResult = await validator.ValidateAsync(registerDto);

        if (!validationResult.IsValid)
            return Results.ValidationProblem(validationResult.ToDictionary());

        var user = await userManager.GetUserAsync(httpContext.User);

        if (user is null)
            return Results.Unauthorized();

        if (!user.IsGuest)
            return Results.Conflict("Account is already registered.");

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync();

        var usernameResult =
            await userManager.SetUserNameAsync(user, registerDto.UserName);

        if (!usernameResult.Succeeded)
        {
            var errors = usernameResult.Errors.ToArray();

            if (errors.Any(e => e.Code == "DuplicateUserName"))
                return Results.Conflict(errors);

            return Results.BadRequest(errors);
        }

        var passwordResult =
            await userManager.AddPasswordAsync(user, registerDto.Password);

        if (!passwordResult.Succeeded)
            return Results.BadRequest(passwordResult.Errors);

        var addRoleResult =
            await userManager.AddToRoleAsync(user, Roles.Member);

        if (!addRoleResult.Succeeded)
            return Results.BadRequest(addRoleResult.Errors);

        var removeRoleResult =
            await userManager.RemoveFromRoleAsync(user, Roles.Guest);

        if (!removeRoleResult.Succeeded)
            return Results.BadRequest(removeRoleResult.Errors);

        user.IsGuest = false;

        await dbContext.SaveChangesAsync();

        var accessToken = await tokenProvider.CreateAccessToken(user);

        await transaction.CommitAsync();

        httpContext.AppendAccessTokenCookie(accessToken);

        return Results.Ok();
    }
}
