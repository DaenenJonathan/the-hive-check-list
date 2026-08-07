using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Features.AuditLogs.DTOs;

namespace TheHive.Application.Features.AuditLogs.Queries.GetAuditLogsByAction;

public record GetAuditLogsByActionQuery(Guid ActionId) : IRequest<List<AuditLogDto>>;

public class GetAuditLogsByActionQueryHandler : IRequestHandler<GetAuditLogsByActionQuery, List<AuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogsByActionQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<AuditLogDto>> Handle(GetAuditLogsByActionQuery request, CancellationToken cancellationToken)
    {
        var checklistIds = await _context.Checklists
            .Where(c => c.BrandActionId == request.ActionId)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var itemIds = await _context.ChecklistItems
            .Where(i => checklistIds.Contains(i.ChecklistId))
            .Select(i => i.Id)
            .ToListAsync(cancellationToken);

        var relevantIds = new HashSet<Guid>(checklistIds) { request.ActionId };
        relevantIds.UnionWith(itemIds);

        return await _context.AuditLogs
            .Where(l => relevantIds.Contains(l.EntityId))
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
