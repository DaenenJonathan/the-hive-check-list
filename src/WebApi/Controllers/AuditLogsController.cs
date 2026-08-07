using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheHive.Application.Features.AuditLogs.Queries.GetAuditLogs;
using TheHive.Application.Features.AuditLogs.Queries.GetAuditLogsByAction;

namespace TheHive.WebApi.Controllers;

[ApiController]
[Route("api/audit-logs")]
[Authorize(Roles = "Admin,Manager")]
public class AuditLogsController : ControllerBase
{
    private readonly ISender _sender;

    public AuditLogsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? entityType,
        [FromQuery] Guid? entityId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAuditLogsQuery(entityType, entityId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("action/{actionId:guid}")]
    public async Task<IActionResult> GetByAction(Guid actionId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAuditLogsByActionQuery(actionId), cancellationToken);
        return Ok(result);
    }
}
