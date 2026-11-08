using FluentValidation;
using Microsoft.AspNetCore.Identity;
using OpenGarden.Server.Constants;
using OpenGarden.Server.Data;
using OpenGarden.Server.Dtos;

namespace OpenGarden.Server.Endpoints.Handlers;

public class RegisterHandler
{
    public static async Task<IResult> Handler(
            RegisterDto registerDto,
            AppDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IValidator<RegisterDto> validator)
    {
        var validationResult = await validator.ValidateAsync(registerDto);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = new ApplicationUser
        {
            UserName = registerDto.UserName
        };

        var createUserResult = await userManager.CreateAsync(user, registerDto.Password);

        if (!createUserResult.Succeeded)
        {
            var errors = createUserResult.Errors.ToArray();

            if (errors.Any(e => e.Code == "DuplicateUserName"))
                return Results.Conflict(errors);

            return Results.BadRequest(errors);
        }

        var addToRoleResult = await userManager.AddToRoleAsync(user, Roles.Member);
        if (!addToRoleResult.Succeeded)
        {
            return Results.BadRequest(addToRoleResult.Errors);
        }

        await transaction.CommitAsync();
        return Results.Ok();
    }
}
