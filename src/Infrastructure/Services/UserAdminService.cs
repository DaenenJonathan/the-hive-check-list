using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Application.Features.Users.DTOs;
using TheHive.Domain.Entities;
using TheHive.Infrastructure.Common;
using TheHive.Infrastructure.Identity;
using TheHive.Infrastructure.Persistence;

namespace TheHive.Infrastructure.Services;

public class UserAdminService : IUserAdminService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly IEmailSender _emailSender;
    private readonly IConfiguration _configuration;

    public UserAdminService(
        UserManager<AppUser> userManager,
        ApplicationDbContext context,
        IEmailSender emailSender,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _context = context;
        _emailSender = emailSender;
        _configuration = configuration;
    }

    public async Task<List<UserDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userManager.Users.Include(u => u.ManagedBrands).ToListAsync(cancellationToken);
        var agencyNames = await _context.Agencies.ToDictionaryAsync(a => a.Id, a => a.Name, cancellationToken);

        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email ?? string.Empty,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Role = u.Role,
            AgencyId = u.AgencyId,
            AgencyName = u.AgencyId.HasValue && agencyNames.TryGetValue(u.AgencyId.Value, out var name) ? name : null,
            Brands = u.ManagedBrands.Select(b => new UserBrandDto { Id = b.Id, Name = b.Name }).ToList()
        }).ToList();
    }

    public async Task<Result> UpdateUserRoleAsync(string userId, string role, Guid? agencyId, IReadOnlyList<Guid> brandIds, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.Users.Include(u => u.ManagedBrands).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
            return Result.Failure("Utilisateur introuvable.");

        if (agencyId.HasValue)
        {
            var agencyExists = await _context.Agencies.AnyAsync(a => a.Id == agencyId.Value, cancellationToken);
            if (!agencyExists)
                return Result.Failure("Agence introuvable.");
        }

        var brands = await ResolveBrandsAsync(brandIds, cancellationToken);
        if (brands is null)
            return Result.Failure("Marque(s) introuvable(s).");
        if (brands.Any(b => b.AgencyId != agencyId))
            return Result.Failure("Toutes les marques doivent appartenir à l'agence sélectionnée.");

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, role);

        user.Role = role;
        user.AgencyId = agencyId;
        user.ManagedBrands = brands;
        await _userManager.UpdateAsync(user);

        return Result.Success();
    }

    public async Task<Result<CreateUserResult>> CreateUserAsync(
        string email, string firstName, string lastName, string role, Guid? agencyId, IReadOnlyList<Guid> brandIds, CancellationToken cancellationToken = default)
    {
        if (await _userManager.FindByEmailAsync(email) is not null)
            return Result<CreateUserResult>.Failure("Un compte existe déjà avec cet email.");

        if (agencyId.HasValue)
        {
            var agencyExists = await _context.Agencies.AnyAsync(a => a.Id == agencyId.Value, cancellationToken);
            if (!agencyExists)
                return Result<CreateUserResult>.Failure("Agence introuvable.");
        }

        var brands = await ResolveBrandsAsync(brandIds, cancellationToken);
        if (brands is null)
            return Result<CreateUserResult>.Failure("Marque(s) introuvable(s).");
        if (brands.Any(b => b.AgencyId != agencyId))
            return Result<CreateUserResult>.Failure("Toutes les marques doivent appartenir à l'agence sélectionnée.");

        var password = PasswordGenerator.Generate();
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            AgencyId = agencyId,
            ManagedBrands = brands,
            EmailConfirmed = true,
            MustChangePassword = true
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            return Result<CreateUserResult>.Failure(createResult.Errors.Select(e => e.Description).ToArray());

        await _userManager.AddToRoleAsync(user, role);

        var loginUrl = _configuration.GetSection("AllowedOrigins").Get<string[]>()?.FirstOrDefault() ?? string.Empty;
        var emailSent = await _emailSender.SendAsync(
            email,
            "Votre compte TheHive",
            $"""
            <p>Bonjour {firstName},</p>
            <p>Un compte TheHive a été créé pour vous.</p>
            <p><strong>Email :</strong> {email}<br/>
            <strong>Mot de passe temporaire :</strong> {password}</p>
            <p>Connectez-vous ici : <a href="{loginUrl}">{loginUrl}</a></p>
            """,
            cancellationToken);

        return Result<CreateUserResult>.Success(new CreateUserResult
        {
            UserId = user.Id,
            EmailSent = emailSent,
            TemporaryPassword = emailSent ? null : password
        });
    }

    public async Task<Result<string>> ResetPasswordAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result<string>.Failure("Utilisateur introuvable.");

        var password = PasswordGenerator.Generate();
        await _userManager.RemovePasswordAsync(user);
        var addResult = await _userManager.AddPasswordAsync(user, password);
        if (!addResult.Succeeded)
            return Result<string>.Failure(addResult.Errors.Select(e => e.Description).ToArray());

        user.MustChangePassword = true;
        await _userManager.UpdateAsync(user);

        return Result<string>.Success(password);
    }

    public async Task<Result> DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return Result.Failure("Utilisateur introuvable.");

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
            return Result.Failure(deleteResult.Errors.Select(e => e.Description).ToArray());

        return Result.Success();
    }

    // Null return signals "some requested brand id doesn't exist" to the caller, distinct from
    // the valid empty-list case (non-Manager roles always pass an empty brandIds list).
    private async Task<List<Brand>?> ResolveBrandsAsync(IReadOnlyList<Guid> brandIds, CancellationToken cancellationToken)
    {
        if (brandIds.Count == 0)
            return [];

        var brands = await _context.Brands.Where(b => brandIds.Contains(b.Id)).ToListAsync(cancellationToken);
        return brands.Count == brandIds.Count ? brands : null;
    }
}
