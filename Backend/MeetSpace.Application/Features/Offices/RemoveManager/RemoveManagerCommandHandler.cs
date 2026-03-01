using MediatR;
using MeetSpace.Application.Interfaces;

namespace MeetSpace.Application.Features.Offices.RemoveManager;

public class RemoveManagerCommandHandler : IRequestHandler<RemoveManagerCommand>
{
    private readonly IOfficeRepository _officeRepo;

    public RemoveManagerCommandHandler(IOfficeRepository officeRepo) => _officeRepo = officeRepo;

    public async Task Handle(RemoveManagerCommand request, CancellationToken ct)
    {
        var office = await _officeRepo.GetByIdAsync(request.OfficeId, ct)
            ?? throw new KeyNotFoundException($"Office {request.OfficeId} not found.");

        await _officeRepo.RemoveAssignmentAsync(office.Id, request.UserId, ct);
    }
}
