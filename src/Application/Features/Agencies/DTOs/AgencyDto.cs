namespace TheHive.Application.Features.Agencies.DTOs;

public class AgencyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int BrandCount { get; set; }
}
