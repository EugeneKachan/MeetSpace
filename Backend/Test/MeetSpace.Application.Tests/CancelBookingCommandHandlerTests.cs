using System;
using System.Threading;
using System.Threading.Tasks;
using MeetSpace.Application.Features.Bookings.CancelBooking;
using MeetSpace.Application.Interfaces;
using MeetSpace.Domain.Entities;
using Moq;
using Xunit;

namespace MeetSpace.Application.Tests
{
    public class CancelBookingCommandHandlerTests
    {
        private readonly Mock<IBookingRepository> _bookingRepo = new Mock<IBookingRepository>();

        private CancelBookingCommandHandler CreateHandler() =>
            new CancelBookingCommandHandler(_bookingRepo.Object);

        [Fact]
        public async Task Handle_OwnerCancels_SetsIsCancelledAndCallsUpdate()
        {
            var id = Guid.NewGuid();
            var booking = new Booking { Id = id, UserId = "user-1", IsCancelled = false };
            _bookingRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
            _bookingRepo.Setup(r => r.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateHandler().Handle(new CancelBookingCommand(id, "user-1", false), CancellationToken.None);

            Assert.True(booking.IsCancelled);
            _bookingRepo.Verify(r => r.UpdateAsync(It.Is<Booking>(b => b.IsCancelled), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ManagerCancelsOtherUserBooking_Succeeds()
        {
            var id = Guid.NewGuid();
            var booking = new Booking { Id = id, UserId = "owner", IsCancelled = false };
            _bookingRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);
            _bookingRepo.Setup(r => r.UpdateAsync(It.IsAny<Booking>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await CreateHandler().Handle(new CancelBookingCommand(id, "manager-1", true), CancellationToken.None);

            Assert.True(booking.IsCancelled);
        }

        [Fact]
        public async Task Handle_NonOwnerNonManager_ThrowsUnauthorizedAccess()
        {
            var id = Guid.NewGuid();
            var booking = new Booking { Id = id, UserId = "owner", IsCancelled = false };
            _bookingRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(booking);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                CreateHandler().Handle(new CancelBookingCommand(id, "someone-else", false), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_BookingNotFound_ThrowsInvalidOperation()
        {
            var id = Guid.NewGuid();
            _bookingRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Booking?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                CreateHandler().Handle(new CancelBookingCommand(id, "user-1", false), CancellationToken.None));
        }
    }
}
