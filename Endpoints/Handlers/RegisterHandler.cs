using Microsoft.AspNetCore.Identity;
using StarterKit.Api.Constants;
using StarterKit.Api.Data;
using StarterKit.Api.Dtos;

namespace StarterKit.Api.Endpoints.Handlers;

public class RegisterHandler
{
    public static async Task<IResult> Handler(
            RegisterDto registerDto,
            AppDbContext dbContext,
            UserManager<ApplicationUser> UserManager)
    {
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email
        };

        var createUserResult = await UserManager.CreateAsync(user, registerDto.Password);

        if (!createUserResult.Succeeded)
        {
            return Results.BadRequest(createUserResult.Errors);
        }

        var addToRoleResult = await UserManager.AddToRoleAsync(user, Roles.Member);
        if (!addToRoleResult.Succeeded)
        {
            return Results.BadRequest(addToRoleResult.Errors);
        }

        await transaction.CommitAsync();
        return Results.Ok();
    }
}
