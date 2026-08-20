using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheHive.Application.Features.Actions.Commands.CancelAction;
using TheHive.Application.Features.Actions.Commands.CreateAction;
using TheHive.Application.Features.Actions.Commands.DeleteAction;
using TheHive.Application.Features.Actions.Commands.MarkActionAsSent;
using TheHive.Application.Features.Actions.Commands.ReactivateAction;
using TheHive.Application.Features.Actions.Commands.UpdateAction;
using TheHive.Application.Features.Actions.Commands.UploadActionPhoto;
using TheHive.Application.Features.Actions.Commands.ValidateActionReturn;
using TheHive.Application.Features.Actions.Queries.GetActionById;
using TheHive.Application.Features.Actions.Queries.GetActionReturns;
using TheHive.Application.Features.Actions.Queries.GetActions;
using TheHive.Domain.Enums;

namespace TheHive.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ActionsController : ControllerBase
{
    private readonly ISender _sender;

    public ActionsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetActionsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetActionByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,AgencyManager")]
    public async Task<IActionResult> Create([FromBody] CreateActionCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return CreatedAtAction(nameof(GetAll), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateActionCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { message = "L'identifiant de la route ne correspond pas à celui de la requête." });

        var result = await _sender.Send(command, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CancelActionCommand(id), cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpPost("{id:guid}/reactivate")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReactivateActionCommand(id), cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,AgencyManager")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteActionCommand(id), cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpPost("{id:guid}/send")]
    [Authorize(Roles = "Admin,Manager,WarehouseUser")]
    public async Task<IActionResult> MarkAsSent(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new MarkActionAsSentCommand(id), cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpGet("{id:guid}/returns")]
    public async Task<IActionResult> GetReturns(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetActionReturnsQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/returns/validate")]
    [Authorize(Roles = "Admin,Manager,WarehouseUser")]
    public async Task<IActionResult> ValidateReturns(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ValidateActionReturnCommand(id), cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpPost("{id:guid}/photos/{kind}")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [Authorize(Roles = "Admin,Manager,WarehouseUser")]
    public async Task<IActionResult> UploadPhoto(Guid id, ActionPhotoKind kind, IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) return BadRequest("No file provided.");
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".gif" or ".webp"))
            return BadRequest("Format non supporté. Utilisez JPG, PNG, GIF ou WebP.");

        using var stream = file.OpenReadStream();
        var result = await _sender.Send(new UploadActionPhotoCommand(id, kind, stream, file.FileName), cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(new { photoPath = result.Value });
    }
}
