using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheHive.Application.Features.Users.Commands.CreateUser;
using TheHive.Application.Features.Users.Commands.DeleteUser;
using TheHive.Application.Features.Users.Commands.ResetUserPassword;
using TheHive.Application.Features.Users.Commands.UpdateUserRole;
using TheHive.Application.Features.Users.Queries.GetUsers;

namespace TheHive.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly ISender _sender;

    public UsersController(ISender sender) => _sender = sender;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUsersQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id}/role")]
    public async Task<IActionResult> UpdateRole(string id, [FromBody] UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateUserRoleCommand(id, request.Role, request.AgencyId, request.BrandIds);
        var result = await _sender.Send(command, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand(request.Email, request.FirstName, request.LastName, request.Role, request.AgencyId, request.BrandIds);
        var result = await _sender.Send(command, cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return CreatedAtAction(nameof(GetAll), null, result.Value);
    }

    [HttpPost("{id}/reset-password")]
    public async Task<IActionResult> ResetPassword(string id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ResetUserPasswordCommand(id), cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return Ok(new { temporaryPassword = result.Value });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteUserCommand(id), cancellationToken);
        if (!result.Succeeded)
            return BadRequest(result.Errors);
        return NoContent();
    }
}

public record UpdateUserRoleRequest(string Role, Guid? AgencyId, IReadOnlyList<Guid>? BrandIds = null);
public record CreateUserRequest(string Email, string FirstName, string LastName, string Role, Guid? AgencyId, IReadOnlyList<Guid>? BrandIds = null);
