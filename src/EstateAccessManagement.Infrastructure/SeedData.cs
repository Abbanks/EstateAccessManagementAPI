using EstateAccessManagement.Common.Enums;
using EstateAccessManagement.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EstateAccessManagement.Infrastructure
{
    public static class SeedData
    {
        public static async Task InitializeAsync(this IServiceProvider serviceProvider, IConfiguration configuration)
        {
            using var scope = serviceProvider.CreateScope();
            var scopedServices = scope.ServiceProvider;

            var loggerFactory = scopedServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("SeedData");
            var userManager = scopedServices.GetRequiredService<UserManager<AppUser>>();
            var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            logger.LogInformation("Seeding database started...");

            var roles = new[] { "Admin", "Resident", "Security" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var roleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                    if (roleResult.Succeeded)
                    {
                        logger.LogInformation("'{Role}' role created successfully.", role);
                    }
                    else
                    {
                        logger.LogError("Failed to create '{Role}' role: {Errors}",
                                        role, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                        return;
                    }
                }
            }

            const string adminRole = "Admin";
            var adminEmail = configuration["AdminUser:Email"];
            var adminPassword = configuration["AdminUser:Password"];

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                logger.LogError("Admin user credentials are not configured in appsettings.json.");
                return;
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new AppUser
                {
                    FirstName = "System",
                    LastName = "Admin",
                    UserName = adminEmail,
                    Email = adminEmail,
                    UserType = UserType.Admin
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                    logger.LogInformation("Initial Admin user created successfully.");
                }
                else
                {
                    logger.LogError("Error creating initial Admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
