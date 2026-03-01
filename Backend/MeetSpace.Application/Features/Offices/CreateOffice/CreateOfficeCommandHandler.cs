using MediatR;
using MeetSpace.Application.Interfaces;
using MeetSpace.Domain.Entities;

namespace MeetSpace.Application.Features.Offices.CreateOffice;

public class CreateOfficeCommandHandler : IRequestHandler<CreateOfficeCommand, Guid>
{
    private readonly IOfficeRepository _repo;

    public CreateOfficeCommandHandler(IOfficeRepository repo) => _repo = repo;

    public async Task<Guid> Handle(CreateOfficeCommand command, CancellationToken ct)
    {
        var office = new Office
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Address = command.Address,
            IsActive = true,
            Rooms = command.Rooms.Select(r => new Room
            {
                Id = Guid.NewGuid(),
                Name = r.Name,
                Capacity = r.Capacity,
                Description = r.Description,
                IsActive = true
            }).ToList()
        };

        await _repo.AddAsync(office, ct);
        return office.Id;
    }
}
