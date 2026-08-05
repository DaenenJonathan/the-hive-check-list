using TheHive.Application.Common.Interfaces;
using TheHive.Domain.Entities;

namespace TheHive.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AuditService(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task LogAsync(string action, string entityType, Guid entityId,
        string? oldValue = null, string? newValue = null,
        CancellationToken cancellationToken = default)
    {
        var log = AuditLog.Create(
            _currentUser.UserId ?? "system",
            _currentUser.UserName ?? "system",
            action,
            entityType,
            entityId,
            oldValue,
            newValue);

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
