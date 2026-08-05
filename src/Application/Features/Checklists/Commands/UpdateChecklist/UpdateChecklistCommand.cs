using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TheHive.Application.Common.Exceptions;
using TheHive.Application.Common.Interfaces;
using TheHive.Application.Common.Models;

namespace TheHive.Application.Features.Checklists.Commands.UpdateChecklist;

public record UpdateChecklistCommand(Guid Id, string Name) : IRequest<Result>;

public class UpdateChecklistCommandValidator : AbstractValidator<UpdateChecklistCommand>
{
    public UpdateChecklistCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class UpdateChecklistCommandHandler : IRequestHandler<UpdateChecklistCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IAuditService _auditService;
    private readonly ICurrentUserService _currentUser;

    public UpdateChecklistCommandHandler(
        IApplicationDbContext context,
        IAuditService auditService,
        ICurrentUserService currentUser)
    {
        _context = context;
        _auditService = auditService;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdateChecklistCommand request, CancellationToken cancellationToken)
    {
        var checklist = await _context.Checklists
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Checklist", request.Id);

        var oldName = checklist.Name;
        checklist.Update(request.Name);
        checklist.SetUpdated(_currentUser.UserId!);

        await _context.SaveChangesAsync(cancellationToken);

        await _auditService.LogAsync("Update", "Checklist", checklist.Id,
            oldValue: oldName, newValue: request.Name, cancellationToken: cancellationToken);

        return Result.Success();
    }
}
