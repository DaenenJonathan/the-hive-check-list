using Microsoft.AspNetCore.Identity;
using TheHive.Infrastructure.Identity;

namespace TheHive.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Seed roles
        string[] roles = ["Admin", "Manager", "WarehouseUser", "Viewer"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed default admin user
        var adminEmail = "admin@thehive.local";
        if (await userManager.FindByEmailAsync(adminEmail) == null)
        {
            var admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "TheHive",
                Role = "Admin",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Seed a manager user
        var managerEmail = "manager@thehive.local";
        if (await userManager.FindByEmailAsync(managerEmail) == null)
        {
            var manager = new AppUser
            {
                UserName = managerEmail,
                Email = managerEmail,
                FirstName = "Manager",
                LastName = "Demo",
                Role = "Manager",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(manager, "Manager123!");
            await userManager.AddToRoleAsync(manager, "Manager");
        }

        // Seed a warehouse user
        var warehouseEmail = "warehouse@thehive.local";
        if (await userManager.FindByEmailAsync(warehouseEmail) == null)
        {
            var warehouse = new AppUser
            {
                UserName = warehouseEmail,
                Email = warehouseEmail,
                FirstName = "Jean",
                LastName = "Dupont",
                Role = "WarehouseUser",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(warehouse, "Warehouse123!");
            await userManager.AddToRoleAsync(warehouse, "WarehouseUser");
        }

        // Seed Daenen admin user
        var daenenEmail = "daenen@thehive.local";
        if (await userManager.FindByEmailAsync(daenenEmail) == null)
        {
            var daenen = new AppUser
            {
                UserName = daenenEmail,
                Email = daenenEmail,
                FirstName = "Jonathan",
                LastName = "Daenen",
                Role = "Admin",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(daenen, "Daenen@2026!");
            await userManager.AddToRoleAsync(daenen, "Admin");
        }
    }
}
