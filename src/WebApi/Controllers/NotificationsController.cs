using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheHive.Application.Features.Notifications.Commands.MarkAllNotificationsRead;
using TheHive.Application.Features.Notifications.Commands.MarkNotificationRead;
using TheHive.Application.Features.Notifications.Queries.GetMyNotifications;

namespace TheHive.WebApi.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ISender _sender;

    public NotificationsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetMine(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMyNotificationsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new MarkNotificationReadCommand(id), cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new MarkAllNotificationsReadCommand(), cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return NoContent();
    }
}
