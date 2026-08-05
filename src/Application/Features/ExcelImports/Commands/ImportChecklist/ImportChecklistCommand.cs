using MediatR;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Domain.Entities;

namespace TheHive.Application.Features.ExcelImports.Commands.ImportChecklist;

public record ImportChecklistCommand(
    Stream FileStream,
    string FileName,
    Guid? BrandActionId = null
) : IRequest<Result<ImportChecklistResult>>;

public record ImportChecklistResult(Guid BrandActionId, Guid ChecklistId);

public class ImportChecklistCommandHandler : IRequestHandler<ImportChecklistCommand, Result<ImportChecklistResult>>
{
    private readonly IApplicationDbContext _context;
    private readonly IExcelChecklistParser _parser;
    private readonly IImageStorageService _imageStorage;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public ImportChecklistCommandHandler(
        IApplicationDbContext context,
        IExcelChecklistParser parser,
        IImageStorageService imageStorage,
        IAuditService auditService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _parser = parser;
        _imageStorage = imageStorage;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result<ImportChecklistResult>> Handle(ImportChecklistCommand request, CancellationToken cancellationToken)
    {
        var preview = await _parser.ParseAsync(request.FileStream, request.FileName, cancellationToken);
        if (!preview.IsValid)
            return Result<ImportChecklistResult>.Failure(preview.Errors.ToArray());

        var brandActionId = request.BrandActionId;

        if (brandActionId == null)
        {
            var eventDate = preview.EventDate ?? DateTime.UtcNow;
            var action = BrandAction.Create(
                preview.ProjectName.Length > 0 ? preview.ProjectName : preview.SuggestedChecklistName,
                preview.Client.Length > 0 ? preview.Client : "Unknown",
                eventDate,
                $"{preview.ActionType} | {preview.Brand}".Trim(' ', '|'),
                address: preview.AddressAction,
                city: preview.City);
            action.SetCreated(_currentUser.UserId!);
            _context.BrandActions.Add(action);
            brandActionId = action.Id;
        }

        var checklist = Checklist.Create(preview.SuggestedChecklistName, brandActionId.Value, request.FileName, preview.EventDate);
        checklist.SetCreated(_currentUser.UserId!);

        int sortOrder = 0;
        foreach (var item in preview.Items)
        {
            var checklistItem = ChecklistItem.Create(
                checklist.Id,
                item.MaterialName,
                item.QuantityRequested,
                category: item.Category,
                notes: item.Notes,
                sortOrder: sortOrder++);
            checklistItem.SetCreated(_currentUser.UserId!);

            if (item.ImageData is not null)
            {
                var imagePath = await _imageStorage.SaveAsync(new MemoryStream(item.ImageData), "item.jpg", cancellationToken);
                checklistItem.SetImage(imagePath);
            }

            checklist.AddItem(checklistItem);
        }

        _context.Checklists.Add(checklist);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Import", "Checklist", checklist.Id,
            newValue: $"Imported from {request.FileName} with {preview.Items.Count} items",
            cancellationToken: cancellationToken);

        return Result<ImportChecklistResult>.Success(new ImportChecklistResult(brandActionId.Value, checklist.Id));
    }
}
