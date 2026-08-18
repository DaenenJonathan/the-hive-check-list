namespace TheHive.Application.Features.Brands.DTOs;

public class BrandDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid AgencyId { get; set; }
    public string AgencyName { get; set; } = string.Empty;
    public string AgencyColor { get; set; } = string.Empty;
}
