using Microsoft.AspNetCore.Identity;

public static class DbInitializer
{
    public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "Admin", "Accountant", "Viewer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
}

