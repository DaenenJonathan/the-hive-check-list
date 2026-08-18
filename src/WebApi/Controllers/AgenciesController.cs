using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheHive.Application.Features.Agencies.Commands.CreateAgency;
using TheHive.Application.Features.Agencies.Commands.DeleteAgency;
using TheHive.Application.Features.Agencies.Commands.UpdateAgency;
using TheHive.Application.Features.Agencies.Queries.GetAgencies;

namespace TheHive.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgenciesController : ControllerBase
{
    private readonly ISender _sender;

    public AgenciesController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetAgenciesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateAgencyCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return CreatedAtAction(nameof(GetAll), new { id = result.Value }, result.Value);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAgencyCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { message = "L'identifiant de la route ne correspond pas à celui de la requête." });

        var result = await _sender.Send(command, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteAgencyCommand(id), cancellationToken);
        if (!result.Succeeded) return BadRequest(result.Errors);
        return NoContent();
    }
}
