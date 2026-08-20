using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;
using TheHive.Domain.Enums;

namespace TheHive.Application.Features.Actions.Commands.UploadActionPhoto;

public record UploadActionPhotoCommand(
    Guid ActionId,
    ActionPhotoKind Kind,
    Stream ImageStream,
    string FileName
) : IRequest<Result<string>>;

public class UploadActionPhotoCommandHandler : IRequestHandler<UploadActionPhotoCommand, Result<string>>
{
    private readonly IApplicationDbContext _context;
    private readonly IImageStorageService _imageStorage;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public UploadActionPhotoCommandHandler(
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

    public async Task<Result<string>> Handle(UploadActionPhotoCommand request, CancellationToken cancellationToken)
    {
        var action = await _context.BrandActions
            .FirstOrDefaultAsync(a => a.Id == request.ActionId, cancellationToken)
            ?? throw new NotFoundException("BrandAction", request.ActionId);

        var existingPath = request.Kind == ActionPhotoKind.Material ? action.MaterialPhotoPath : action.ConsumablesPhotoPath;
        if (existingPath is not null)
            await _imageStorage.DeleteAsync(existingPath, cancellationToken);

        var photoPath = await _imageStorage.SaveAsync(request.ImageStream, request.FileName, cancellationToken);
        if (request.Kind == ActionPhotoKind.Material) action.SetMaterialPhoto(photoPath);
        else action.SetConsumablesPhoto(photoPath);

        await _context.SaveChangesAsync(cancellationToken);
        await _auditService.LogAsync($"UploadActionPhoto:{request.Kind}", "BrandAction", action.Id,
            newValue: photoPath, cancellationToken: cancellationToken);

        return Result<string>.Success(photoPath);
    }
}
