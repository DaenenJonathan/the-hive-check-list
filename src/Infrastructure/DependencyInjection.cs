using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheHive.Application.Common.Interfaces;
using TheHive.Infrastructure.Common.Email;
using TheHive.Infrastructure.Excel;
using TheHive.Infrastructure.Identity;
using TheHive.Infrastructure.Persistence;
using TheHive.Infrastructure.Services;
using TheHive.Infrastructure.Storage;

namespace TheHive.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseProvider = configuration["Database:Provider"] ?? "SqlServer";
        if (databaseProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<ApplicationDbContext, PostgresApplicationDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        }
        else
        {
            services.AddDbContext<ApplicationDbContext, SqlServerApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        }

        services.AddIdentityCore<AppUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = true;

            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        })
        .AddRoles<IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders()
        .AddSignInManager<SignInManager<AppUser>>();

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IUserDirectoryService, UserDirectoryService>();
        services.AddScoped<IUserAdminService, UserAdminService>();
        services.Configure<SmtpSettings>(configuration.GetSection("Smtp"));
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();
        services.AddScoped<IExcelChecklistParser, ExcelChecklistParser>();
        services.AddScoped<IImageStorageService, LocalImageStorageService>();
        services.AddScoped<TokenService>();

        return services;
    }
}
