using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheHive.Application.Features.ChecklistItems.Commands.AddChecklistItem;
using TheHive.Application.Features.ChecklistItems.Commands.DeleteChecklistItem;
using TheHive.Application.Features.ChecklistItems.Commands.UpdateChecklistItemDetails;
using TheHive.Application.Features.ChecklistItems.Commands.UpdateChecklistItemReturn;
using TheHive.Application.Features.ChecklistItems.Commands.UpdateChecklistItemStatus;
using TheHive.Application.Features.ChecklistItems.Commands.UploadChecklistItemImage;

namespace TheHive.WebApi.Controllers;

[ApiController]
[Route("api/checklist-items")]
[Authorize]
public class ChecklistItemsController : ControllerBase
{
    private readonly ISender _sender;

    public ChecklistItemsController(ISender sender) => _sender = sender;

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddItem([FromBody] AddChecklistItemCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return Ok(new { id = result.Value });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateDetails(Guid id, [FromBody] UpdateChecklistItemDetailsCommand command, CancellationToken cancellationToken)
    {
        if (id != command.ItemId) return BadRequest("ID mismatch");
        var result = await _sender.Send(command, cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteItem(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteChecklistItemCommand(id), cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = "Admin,Manager,WarehouseUser")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateChecklistItemStatusCommand command, CancellationToken cancellationToken)
    {
        if (id != command.ItemId) return BadRequest("ID mismatch");
        var result = await _sender.Send(command, cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpPatch("{id:guid}/return")]
    [Authorize(Roles = "Admin,Manager,WarehouseUser")]
    public async Task<IActionResult> UpdateReturn(Guid id, [FromBody] UpdateChecklistItemReturnCommand command, CancellationToken cancellationToken)
    {
        if (id != command.ItemId) return BadRequest("ID mismatch");
        var result = await _sender.Send(command, cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpPost("{id:guid}/image")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    [Authorize(Roles = "Admin,Manager,WarehouseUser")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0) return BadRequest("No file provided.");
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".gif" or ".webp"))
            return BadRequest("Format non supporté. Utilisez JPG, PNG, GIF ou WebP.");

        using var stream = file.OpenReadStream();
        var result = await _sender.Send(new UploadChecklistItemImageCommand(id, stream, file.FileName), cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);

        return Ok(new { imagePath = result.Value });
    }
}
