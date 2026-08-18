namespace TheHive.Application.Common.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    string? UserName { get; }
    string? Role { get; }
    string? AgencyId { get; }
    IReadOnlyList<Guid> BrandIds { get; }
    bool IsAuthenticated { get; }
}
