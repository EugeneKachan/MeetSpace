using MediatR;
using MeetSpace.Application.Interfaces;
using MeetSpace.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MeetSpace.Application.Features.Offices.AssignManager;

public class AssignManagerCommandHandler : IRequestHandler<AssignManagerCommand>
{
    private readonly IOfficeRepository _officeRepo;
    private readonly UserManager<ApplicationUser> _userManager;

    public AssignManagerCommandHandler(IOfficeRepository officeRepo, UserManager<ApplicationUser> userManager)
    {
        _officeRepo = officeRepo;
        _userManager = userManager;
    }

    public async Task Handle(AssignManagerCommand request, CancellationToken ct)
    {
        var office = await _officeRepo.GetByIdAsync(request.OfficeId, ct)
            ?? throw new KeyNotFoundException($"Office {request.OfficeId} not found.");

        var user = await _userManager.FindByIdAsync(request.UserId)
            ?? throw new KeyNotFoundException($"User {request.UserId} not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(UserRoles.OfficeManager))
            throw new InvalidOperationException("Only users with the OfficeManager role can be assigned to offices.");

        var alreadyAssigned = await _officeRepo.AssignmentExistsAsync(office.Id, user.Id, ct);
        if (alreadyAssigned)
            throw new InvalidOperationException($"User {user.Email} is already assigned to this office.");

        var assignment = new OfficeAssignment { OfficeId = office.Id, UserId = user.Id };
        await _officeRepo.AddAssignmentAsync(assignment, ct);
    }
}
