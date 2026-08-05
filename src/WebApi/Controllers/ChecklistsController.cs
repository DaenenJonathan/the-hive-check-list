using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheHive.Application.Features.Checklists.Commands.BatchUpdateItemStatus;
using TheHive.Application.Features.Checklists.Commands.CreateChecklist;
using TheHive.Application.Features.Checklists.Commands.ReorderChecklistItems;
using TheHive.Application.Features.Checklists.Commands.UpdateChecklist;
using TheHive.Application.Features.Checklists.Queries.GetChecklistById;
using TheHive.Application.Features.Checklists.Queries.GetChecklists;

namespace TheHive.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChecklistsController : ControllerBase
{
    private readonly ISender _sender;

    public ChecklistsController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? brandActionId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetChecklistsQuery(brandActionId), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetChecklistByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Create([FromBody] CreateChecklistCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    [HttpPatch("{id:guid}/reorder")]
    public async Task<IActionResult> Reorder(Guid id, [FromBody] List<ReorderItemDto> items, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReorderChecklistItemsCommand(id, items), cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpPatch("{id:guid}/items/status")]
    [Authorize(Roles = "Admin,Manager,WarehouseUser")]
    public async Task<IActionResult> BatchUpdateItemStatus(Guid id, [FromBody] List<ItemStatusUpdateDto> items, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new BatchUpdateItemStatusCommand(id, items), cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateChecklistCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("ID mismatch");
        var result = await _sender.Send(command, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return NoContent();
    }
}
