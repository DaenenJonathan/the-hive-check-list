using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Features.AuditLogs.DTOs;

namespace TheHive.Application.Features.AuditLogs.Queries.GetAuditLogs;

public record GetAuditLogsQuery(string? EntityType = null, Guid? EntityId = null) : IRequest<List<AuditLogDto>>;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, List<AuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(request.EntityType))
            query = query.Where(l => l.EntityType == request.EntityType);

        if (request.EntityId.HasValue)
            query = query.Where(l => l.EntityId == request.EntityId.Value);

        return await query
            .OrderByDescending(l => l.OccurredAt)
            .Take(500)
            .Select(l => new AuditLogDto
            {
                Id = l.Id,
                UserId = l.UserId,
                UserName = l.UserName,
                Action = l.Action,
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                OldValue = l.OldValue,
                NewValue = l.NewValue,
                OccurredAt = l.OccurredAt
            })
            .ToListAsync(cancellationToken);
    }
}
