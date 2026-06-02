using Microsoft.AspNetCore.Identity;

namespace AgentFlow.API.Data
{
    public static class RoleSeeder
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                string[] roleNames = { "Admin", "User" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var role = new IdentityRole(roleName);
                        await roleManager.CreateAsync(role);
                    }
                }
                const string adminEmail = "admin@agentflow.io";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new IdentityUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail
                    };
                    await userManager.CreateAsync(adminUser, "Admin@123");
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}