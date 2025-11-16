using ABC_Retail.Data;
using ABC_Retail.Models;
using Microsoft.AspNetCore.Identity;

namespace ABC_Retail.SeedData
{
    public class SeedService
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Users>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<SeedService>>();

            try
            {
                //ensures the db is ready
                logger.LogInformation("Ensuring the db is created");
                await context.Database.EnsureCreatedAsync();

                //adding roles
                logger.LogInformation("Seeding roles");
                await addRoleAsync(roleManager, "Admin");
                await addRoleAsync(roleManager, "Customer");

                //adding an multiple users 
                logger.LogInformation("Seeding users");
                await SeedUserAsync(userManager, logger, "admin@abcretail.co.za", "John Doe", "#Admin123", "Admin");
                await SeedUserAsync(userManager, logger, "customer@abcretail.co.za", "Jane Smith", "#Customer123", "Customer");
            }
            catch (Exception ex)
            {
                logger.LogError("An error occured while seeding the database.");
            }
        }

        private static async Task addRoleAsync(RoleManager<IdentityRole> roleManager, string roleName)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create role '{roleName}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }

        private static async Task SeedUserAsync(UserManager<Users> userManager, ILogger logger, string email, string fullName, string password, string role)
        {
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var user = new Users
                {
                    FullName = fullName,
                    UserName = email,
                    NormalizedUserName = email.ToUpper(),
                    Email = email,
                    NormalizedEmail = email.ToUpper(),
                    EmailConfirmed = true,
                    SecurityStamp = Guid.NewGuid().ToString()
                };

                var result = await userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    logger.LogInformation($"Assigning role '{role}' to user '{email}'");
                    await userManager.AddToRoleAsync(user, role);
                }
                else
                {
                    logger.LogError("Failed to create user '{Email}': {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
