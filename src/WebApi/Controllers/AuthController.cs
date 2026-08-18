using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TheHive.Application.Common.Interfaces;
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
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RequestAccountRequest> _requestAccountValidator;

    public AuthController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        TokenService tokenService,
        INotificationDispatcher notificationDispatcher,
        IValidator<LoginRequest> loginValidator,
        IValidator<RequestAccountRequest> requestAccountValidator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _notificationDispatcher = notificationDispatcher;
        _loginValidator = loginValidator;
        _requestAccountValidator = requestAccountValidator;
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

        var token = await _tokenService.GenerateTokenAsync(user);

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

    [HttpPost("request-account")]
    public async Task<IActionResult> RequestAccount([FromBody] RequestAccountRequest request)
    {
        var validation = await _requestAccountValidator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var requesterName = $"{request.FirstName.Trim()} {request.LastName.Trim()}";
        await _notificationDispatcher.DispatchAccountRequestAsync(
            requesterName, request.Email.Trim().ToLowerInvariant(), request.Message?.Trim());

        return NoContent();
    }
}

public record LoginRequest(string Email, string Password);
public record RequestAccountRequest(string FirstName, string LastName, string Email, string? Message);
