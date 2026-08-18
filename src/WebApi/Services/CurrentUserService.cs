using System.Security.Claims;
using TheHive.Application.Common.Interfaces;

namespace TheHive.WebApi.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? UserName => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name);
    public string? Role => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role);
    public string? AgencyId
    {
        get
        {
            var value = _httpContextAccessor.HttpContext?.User.FindFirstValue("agencyId");
            return string.IsNullOrEmpty(value) ? null : value;
        }
    }
    public IReadOnlyList<Guid> BrandIds =>
        _httpContextAccessor.HttpContext?.User.FindAll("brandId")
            .Select(c => Guid.TryParse(c.Value, out var id) ? id : (Guid?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList()
        ?? [];

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
