using MediatR;
using MeetSpace.Application.Interfaces;

namespace MeetSpace.Application.Features.Offices.UpdateOffice;

public class UpdateOfficeCommandHandler : IRequestHandler<UpdateOfficeCommand>
{
    private readonly IOfficeRepository _repo;

    public UpdateOfficeCommandHandler(IOfficeRepository repo) => _repo = repo;

    public async Task Handle(UpdateOfficeCommand command, CancellationToken ct)
    {
        var office = await _repo.GetByIdAsync(command.Id, ct)
            ?? throw new KeyNotFoundException($"Office '{command.Id}' not found.");

        office.Name = command.Name;
        office.Address = command.Address;

        await _repo.UpdateAsync(office, ct);
    }
}
