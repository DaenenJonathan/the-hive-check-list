using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Features.ExcelImports.Commands.ImportChecklist;

namespace TheHive.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class ImportsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IExcelChecklistParser _parser;

    public ImportsController(ISender sender, IExcelChecklistParser parser)
    {
        _sender = sender;
        _parser = parser;
    }

    [HttpPost("preview")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Preview(IFormFile file, [FromForm] string? sheetName, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".xlsx" && ext != ".xlsm")
            return BadRequest("Only .xlsx and .xlsm files are supported.");

        using var stream = file.OpenReadStream();
        var preview = await _parser.ParseAsync(stream, file.FileName, sheetName, cancellationToken);
        return Ok(preview);
    }

    [HttpPost("confirm")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> Confirm(
        [FromForm] Guid? brandActionId,
        [FromForm] string? sheetName,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        using var stream = file.OpenReadStream();
        var command = new ImportChecklistCommand(stream, file.FileName, brandActionId, sheetName);
        var result = await _sender.Send(command, cancellationToken);

        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new
        {
            brandActionId = result.Value!.BrandActionId,
            checklistId = result.Value.ChecklistId
        });
    }
}
