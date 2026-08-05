using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;

namespace TheHive.Application.Features.ChecklistItems.Commands.UploadChecklistItemImage;

public record UploadChecklistItemImageCommand(
    Guid ItemId,
    Stream ImageStream,
    string FileName
) : IRequest<Result<string>>;

public class UploadChecklistItemImageCommandHandler : IRequestHandler<UploadChecklistItemImageCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IImageStorageService _imageStorage;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public UploadChecklistItemImageCommandHandler(
        IApplicationDbContext context,
        IImageStorageService imageStorage,
        IAuditService auditService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _imageStorage = imageStorage;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result<string>> Handle(UploadChecklistItemImageCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.ChecklistItems
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken)
            ?? throw new NotFoundException("ChecklistItem", request.ItemId);

        if (item.ImagePath is not null)
            await _imageStorage.DeleteAsync(item.ImagePath, cancellationToken);

        var imagePath = await _imageStorage.SaveAsync(request.ImageStream, request.FileName, cancellationToken);
        item.SetImage(imagePath);

        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync("UploadImage", "ChecklistItem", item.Id,
            newValue: imagePath, cancellationToken: cancellationToken);

        return Result<string>.Success(imagePath);
    }
}
