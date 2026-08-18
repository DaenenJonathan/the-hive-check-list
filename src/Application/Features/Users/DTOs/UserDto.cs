namespace TheHive.Application.Features.Users.DTOs;

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public Guid? AgencyId { get; set; }
    public string? AgencyName { get; set; }
    public List<UserBrandDto> Brands { get; set; } = [];
}

public class UserBrandDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
