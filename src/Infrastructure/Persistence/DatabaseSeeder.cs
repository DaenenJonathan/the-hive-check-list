using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheHive.Domain.Entities;
using TheHive.Infrastructure.Identity;

namespace TheHive.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(
        UserManager<AppUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext dbContext,
        bool seedDemoUsers)
    {
        // Seed roles
        string[] roles = ["Admin", "Manager", "WarehouseUser", "Viewer", "AgencyManager"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        if (!seedDemoUsers)
            return;

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

        // Seed demo agencies and brands
        var hbs = await dbContext.Agencies.FirstOrDefaultAsync(a => a.Name == "HBS");
        if (hbs is null)
        {
            hbs = Agency.Create("HBS", "#2563EB");
            dbContext.Agencies.Add(hbs);
            dbContext.Brands.Add(Brand.Create("Pepsi", hbs.Id));
            dbContext.Brands.Add(Brand.Create("Dr Pepper", hbs.Id));
        }

        var butik = await dbContext.Agencies.FirstOrDefaultAsync(a => a.Name == "Butik");
        if (butik is null)
        {
            butik = Agency.Create("Butik", "#EA580C");
            dbContext.Agencies.Add(butik);
            dbContext.Brands.Add(Brand.Create("Aperol", butik.Id));
            dbContext.Brands.Add(Brand.Create("Campari", butik.Id));
        }

        await dbContext.SaveChangesAsync();

        // Seed an agency manager user
        var agencyEmail = "agency@thehive.local";
        if (await userManager.FindByEmailAsync(agencyEmail) == null)
        {
            var agencyManager = new AppUser
            {
                UserName = agencyEmail,
                Email = agencyEmail,
                FirstName = "Agence",
                LastName = "HBS",
                Role = "AgencyManager",
                AgencyId = hbs.Id,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(agencyManager, "Agency123!");
            await userManager.AddToRoleAsync(agencyManager, "AgencyManager");
        }
    }
}
