using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;

namespace TheHive.Application.Features.Checklists.Commands.ReorderChecklistItems;

public record ReorderItemDto(Guid ItemId, int SortOrder, string? Category);

public record ReorderChecklistItemsCommand(Guid ChecklistId, List<ReorderItemDto> Items)
    : IRequest<Result<bool>>;

public class ReorderChecklistItemsCommandHandler : IRequestHandler<ReorderChecklistItemsCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;

    public ReorderChecklistItemsCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result<bool>> Handle(ReorderChecklistItemsCommand request, CancellationToken cancellationToken)
    {
        var items = await _context.ChecklistItems
            .Where(i => i.ChecklistId == request.ChecklistId)
            .ToListAsync(cancellationToken);

        foreach (var dto in request.Items)
        {
            var item = items.FirstOrDefault(i => i.Id == dto.ItemId);
            item?.UpdateOrder(dto.SortOrder, dto.Category);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Success(true);
    }
}
