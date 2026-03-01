using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetSpace.Application.Features.Offices.CreateOffice;
using MeetSpace.Application.Interfaces;
using MeetSpace.Domain.Entities;
using Moq;
using Xunit;

namespace MeetSpace.Application.Tests
{
    public class CreateOfficeCommandHandlerTests
    {
        private readonly Mock<IOfficeRepository> _repo = new Mock<IOfficeRepository>();

        [Fact]
        public async Task Handle_NoRooms_AddsOfficeAndReturnsId()
        {
            _repo.Setup(r => r.AddAsync(It.IsAny<Office>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new CreateOfficeCommandHandler(_repo.Object);
            var cmd = new CreateOfficeCommand("HQ", "123 Main St", true, new List<CreateRoomRequest>());

            var id = await handler.Handle(cmd, CancellationToken.None);

            Assert.NotEqual(Guid.Empty, id);
            _repo.Verify(r => r.AddAsync(
                It.Is<Office>(o => o.Name == "HQ" && o.Address == "123 Main St" && o.IsActive),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_WithRooms_MapsRoomsCorrectly()
        {
            Office? captured = null;
            _repo.Setup(r => r.AddAsync(It.IsAny<Office>(), It.IsAny<CancellationToken>()))
                .Callback<Office, CancellationToken>((o, _) => captured = o)
                .Returns(Task.CompletedTask);

            var handler = new CreateOfficeCommandHandler(_repo.Object);
            var cmd = new CreateOfficeCommand("Branch", "456 Side Ave", true, new List<CreateRoomRequest>
            {
                new CreateRoomRequest("Room A", 10, "Small meeting room", true),
                new CreateRoomRequest("Room B", 20, "Large conference room", true)
            });

            var id = await handler.Handle(cmd, CancellationToken.None);

            Assert.NotNull(captured);
            Assert.Equal(2, captured!.Rooms.Count);
            Assert.Contains(captured.Rooms, r => r.Name == "Room A" && r.Capacity == 10 && r.IsActive);
            Assert.Contains(captured.Rooms, r => r.Name == "Room B" && r.Capacity == 20 && r.IsActive);
        }

        [Fact]
        public async Task Handle_ReturnedId_MatchesCreatedOfficeId()
        {
            Office? captured = null;
            _repo.Setup(r => r.AddAsync(It.IsAny<Office>(), It.IsAny<CancellationToken>()))
                .Callback<Office, CancellationToken>((o, _) => captured = o)
                .Returns(Task.CompletedTask);

            var handler = new CreateOfficeCommandHandler(_repo.Object);
            var cmd = new CreateOfficeCommand("Office", "1 Street", true, new List<CreateRoomRequest>());

            var returnedId = await handler.Handle(cmd, CancellationToken.None);

            Assert.Equal(captured!.Id, returnedId);
        }
    }
}
