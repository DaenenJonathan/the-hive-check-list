using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheHive.Application.Features.AuditLogs.Queries.GetAuditLogs;

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
}
