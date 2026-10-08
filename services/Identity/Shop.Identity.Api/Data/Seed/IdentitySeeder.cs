using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Shop.Identity.Api.Options;
using Shop.Identity.Api.Entities;
using Shop.Shared.Constants;

using System.Net;
using Microsoft.IdentityModel.Tokens;

namespace Shop.Identity.Api.Data.Seed;
public static class IdentitySeeder
{
    public static async Task SeedIdentityAsync(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var seed = scope.ServiceProvider.GetRequiredService<IOptions<SeedOptions>>().Value;
        var logger = app.Logger;

        foreach(var roleName in Roles.All)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                {
                continue;
                }

            var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            EnsureSucceeded(roleResult, $"Create role {roleName}");
            logger.LogInformation("Seed: created role {Role}", roleName);
        }

        var admin = await userManager.FindByEmailAsync(seed.AdminEmail);
        if (admin is null)
        {
            admin = new AppUser
            {
                UserName = seed.AdminEmail,
                Email = seed.AdminEmail,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(admin, seed.AdminPassword);
            EnsureSucceeded(createResult, "create admin");
            logger.LogInformation("Seed: created admin {Email}", seed.AdminEmail);
        }

        if (!await userManager.IsInRoleAsync(admin, Roles.Admin))
        {
            var AddRoleResult = await userManager.AddToRoleAsync(admin, Roles.Admin);
            EnsureSucceeded(AddRoleResult, "add admin role");
            logger.LogInformation("Seed: added role {Role} to {Email}", Roles.Admin, seed.AdminEmail);
        }
    }


        
    private static void EnsureSucceeded(IdentityResult result, string action)
    {
        if (result.Succeeded)
        {
            return;
        }
        var errors = string.Join(", ", result.Errors.Select(e=>e.Description));
        throw new InvalidOperationException($"Seed failes: {action} : {errors}");
    }
}


