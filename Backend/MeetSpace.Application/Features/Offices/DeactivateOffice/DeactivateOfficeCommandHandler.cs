using MediatR;
using MeetSpace.Application.Interfaces;

namespace MeetSpace.Application.Features.Offices.DeactivateOffice;

public class DeactivateOfficeCommandHandler : IRequestHandler<DeactivateOfficeCommand>
{
    private readonly IOfficeRepository _repo;

    public DeactivateOfficeCommandHandler(IOfficeRepository repo) => _repo = repo;

    public async Task Handle(DeactivateOfficeCommand command, CancellationToken ct)
    {
        var office = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"Office '{command.Id}' not found.");

        office.IsActive = false;

        await _repo.UpdateAsync(office, ct);
    }
}
