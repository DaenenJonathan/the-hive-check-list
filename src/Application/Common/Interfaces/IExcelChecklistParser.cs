using TheHive.Application.Features.ExcelImports.DTOs;

namespace TheHive.Application.Common.Interfaces;

public interface IExcelChecklistParser
{
    Task<ExcelImportPreviewDto> ParseAsync(Stream fileStream, string fileName, string? sheetName = null, CancellationToken cancellationToken = default);
}
