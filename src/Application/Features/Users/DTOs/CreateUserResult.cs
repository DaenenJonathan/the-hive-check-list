namespace TheHive.Application.Features.Users.DTOs;

public class CreateUserResult
{
    public string UserId { get; set; } = string.Empty;
    public bool EmailSent { get; set; }
    public string? TemporaryPassword { get; set; }
}
