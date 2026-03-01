using System;
using System.Threading;
using System.Threading.Tasks;
using MeetSpace.Application.Features.Bookings.CreateBooking;
using MeetSpace.Application.Interfaces;
using MeetSpace.Domain.Entities;
using Moq;
using Xunit;

namespace MeetSpace.Application.Tests
{
    public class CreateBookingCommandHandlerTests
    {
        private readonly Mock<IOfficeRepository> _officeRepo = new Mock<IOfficeRepository>();
        private readonly Mock<IRoomRepository> _roomRepo = new Mock<IRoomRepository>();
        private readonly Mock<IBookingRepository> _bookingRepo = new Mock<IBookingRepository>();

        private CreateBookingCommandHandler CreateHandler() =>
            new CreateBookingCommandHandler(_officeRepo.Object, _roomRepo.Object, _bookingRepo.Object);

        private static CreateBookingCommand ValidCommand(Guid officeId, Guid roomId) =>
            new CreateBookingCommand(
                officeId, roomId,
                new DateOnly(2026, 6, 1),
                new TimeOnly(9, 0), new TimeOnly(10, 0),
                "Planning", "user-1");

        [Fact]
        public async Task Handle_ValidInput_CreatesBookingAndReturnsId()
        {
            var officeId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            var office = new Office { Id = officeId, Name = "HQ", IsActive = true };
            var room = new Room { Id = roomId, OfficeId = officeId, Name = "Room A", IsActive = true };

            _officeRepo.Setup(r => r.GetByIdAsync(officeId, It.IsAny<CancellationToken>())).ReturnsAsync(office);
            _roomRepo.Setup(r => r.GetByIdAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync(room);
            _bookingRepo.Setup(r => r.HasConflictingAsync(roomId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            _bookingRepo.Setup(r => r.AddAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var id = await CreateHandler().Handle(ValidCommand(officeId, roomId), CancellationToken.None);

            Assert.NotEqual(Guid.Empty, id);
            _bookingRepo.Verify(r => r.AddAsync(
                It.Is<Booking>(b => b.RoomId == roomId && b.UserId == "user-1" && !b.IsCancelled),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_OfficeNotFound_ThrowsInvalidOperation()
        {
            var officeId = Guid.NewGuid();
            _officeRepo.Setup(r => r.GetByIdAsync(officeId, It.IsAny<CancellationToken>())).ReturnsAsync((Office?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateHandler().Handle(ValidCommand(officeId, Guid.NewGuid()), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_OfficeInactive_ThrowsInvalidOperation()
        {
            var officeId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            _officeRepo.Setup(r => r.GetByIdAsync(officeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Office { Id = officeId, IsActive = false });

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateHandler().Handle(ValidCommand(officeId, roomId), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_RoomNotFound_ThrowsInvalidOperation()
        {
            var officeId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            _officeRepo.Setup(r => r.GetByIdAsync(officeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Office { Id = officeId, IsActive = true });
            _roomRepo.Setup(r => r.GetByIdAsync(roomId, It.IsAny<CancellationToken>())).ReturnsAsync((Room?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateHandler().Handle(ValidCommand(officeId, roomId), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_RoomInactive_ThrowsInvalidOperation()
        {
            var officeId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            _officeRepo.Setup(r => r.GetByIdAsync(officeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Office { Id = officeId, IsActive = true });
            _roomRepo.Setup(r => r.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Room { Id = roomId, OfficeId = officeId, IsActive = false });

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateHandler().Handle(ValidCommand(officeId, roomId), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_RoomBelongsToDifferentOffice_ThrowsInvalidOperation()
        {
            var officeId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            _officeRepo.Setup(r => r.GetByIdAsync(officeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Office { Id = officeId, IsActive = true });
            _roomRepo.Setup(r => r.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Room { Id = roomId, OfficeId = Guid.NewGuid(), IsActive = true });

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateHandler().Handle(ValidCommand(officeId, roomId), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ConflictingBooking_ThrowsInvalidOperation()
        {
            var officeId = Guid.NewGuid();
            var roomId = Guid.NewGuid();
            _officeRepo.Setup(r => r.GetByIdAsync(officeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Office { Id = officeId, IsActive = true });
            _roomRepo.Setup(r => r.GetByIdAsync(roomId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Room { Id = roomId, OfficeId = officeId, IsActive = true });
            _bookingRepo.Setup(r => r.HasConflictingAsync(roomId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateHandler().Handle(ValidCommand(officeId, roomId), CancellationToken.None));
        }
    }
}
