using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TheHive.Infrastructure.Identity;
using ValidationException = TheHive.Application.Common.Exceptions.ValidationException;

namespace TheHive.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly TokenService _tokenService;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RegisterRequest> _registerValidator;

    public AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        TokenService tokenService,
        IValidator<LoginRequest> loginValidator,
        IValidator<RegisterRequest> registerValidator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _loginValidator = loginValidator;
        _registerValidator = registerValidator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var validation = await _loginValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
            return Unauthorized(new { message = "Email ou mot de passe incorrect." });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
            return Unauthorized(new { message = "Compte temporairement verrouillé suite à plusieurs échecs de connexion. Réessayez dans quelques minutes." });

        if (!result.Succeeded)
            return Unauthorized(new { message = "Email ou mot de passe incorrect." });

        var token = _tokenService.GenerateToken(user);

        return Ok(new
        {
            token,
            expiresAt = DateTime.UtcNow.AddHours(8),
            user = new
            {
                id = user.Id,
                userName = user.UserName,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role
            }
        });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var validation = await _registerValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var email = request.Email.Trim().ToLowerInvariant();

        if (await _userManager.FindByEmailAsync(email) != null)
            return BadRequest(new { status = 400, message = "Un compte existe déjà avec cet email.", errors = Array.Empty<string>() });

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = "WarehouseUser",
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { status = 400, message = "Impossible de créer le compte.", errors = result.Errors.Select(e => e.Description).ToArray() });

        await _userManager.AddToRoleAsync(user, user.Role);

        var token = _tokenService.GenerateToken(user);

        return Ok(new
        {
            token,
            expiresAt = DateTime.UtcNow.AddHours(8),
            user = new
            {
                id = user.Id,
                userName = user.UserName,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role
            }
        });
    }
}

public record LoginRequest(string Email, string Password);
public record RegisterRequest(string Email, string Password, string ConfirmPassword, string FirstName, string LastName);
