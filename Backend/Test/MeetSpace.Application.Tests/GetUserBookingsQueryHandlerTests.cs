using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetSpace.Application.Features.Bookings.GetUserBookings;
using MeetSpace.Application.Interfaces;
using MeetSpace.Domain.Entities;
using Moq;
using Xunit;

namespace MeetSpace.Application.Tests
{
    public class GetUserBookingsQueryHandlerTests
    {
        private readonly Mock<IBookingRepository> _repo = new Mock<IBookingRepository>();

        private static Booking BuildBooking(
            string userId = "user-1",
            string title = "Stand-up",
            string roomName = "Room A",
            string officeName = "HQ",
            DateTime? start = null,
            DateTime? end = null,
            bool isCancelled = false)
        {
            var s = start ?? new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc);
            var e = end   ?? new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
            return new Booking
            {
                Id = Guid.NewGuid(),
                RoomId = Guid.NewGuid(),
                UserId = userId,
                StartTime = s,
                EndTime = e,
                Title = title,
                IsCancelled = isCancelled,
                Room = new Room
                {
                    Name = roomName,
                    Office = new Office { Name = officeName }
                }
            };
        }

        // ── Handle_ReturnsMappedDtos ──────────────────────────────────────────────

        [Fact]
        public async Task Handle_ReturnsMappedDtos()
        {
            var booking = BuildBooking();
            _repo.Setup(r => r.GetByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Booking> { booking });

            var handler = new GetUserBookingsQueryHandler(_repo.Object);
            var result = await handler.Handle(new GetUserBookingsQuery("user-1"), CancellationToken.None);

            Assert.Single(result);
            var dto = result[0];
            Assert.Equal(booking.Id,     dto.Id);
            Assert.Equal(booking.RoomId, dto.RoomId);
            Assert.Equal("Room A",      dto.RoomName);
            Assert.Equal("HQ",          dto.OfficeName);
            Assert.Equal("Stand-up",    dto.Title);
            Assert.False(dto.IsCancelled);
        }

        // ── Handle_MapsDateAndTimeStrings_Correctly ───────────────────────────────

        [Fact]
        public async Task Handle_MapsDateAndTimeStrings_Correctly()
        {
            var start   = new DateTime(2026, 6, 15, 9, 30, 0, DateTimeKind.Utc);
            var end     = new DateTime(2026, 6, 15, 10, 45, 0, DateTimeKind.Utc);
            var booking = BuildBooking(start: start, end: end);

            _repo.Setup(r => r.GetByUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Booking> { booking });

            var handler = new GetUserBookingsQueryHandler(_repo.Object);
            var result  = await handler.Handle(new GetUserBookingsQuery("user-1"), CancellationToken.None);

            var dto = result[0];
            Assert.Equal("2026-06-15", dto.Date);
            Assert.Equal("09:30",      dto.StartTime);
            Assert.Equal("10:45",      dto.EndTime);
        }

        // ── Handle_EmptyBookings_ReturnsEmptyList ─────────────────────────────────

        [Fact]
        public async Task Handle_EmptyBookings_ReturnsEmptyList()
        {
            _repo.Setup(r => r.GetByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Booking>());

            var handler = new GetUserBookingsQueryHandler(_repo.Object);
            var result  = await handler.Handle(new GetUserBookingsQuery("user-1"), CancellationToken.None);

            Assert.Empty(result);
        }

        // ── Handle_BookingWithNullRoom_UsesEmptyStrings ───────────────────────────

        [Fact]
        public async Task Handle_BookingWithNullRoom_UsesEmptyStrings()
        {
            var booking = new Booking
            {
                Id        = Guid.NewGuid(),
                RoomId    = Guid.NewGuid(),
                UserId    = "user-1",
                StartTime = new DateTime(2026, 6, 1, 9, 0, 0, DateTimeKind.Utc),
                EndTime   = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc),
                Title     = "Orphan booking",
                Room      = null
            };

            _repo.Setup(r => r.GetByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Booking> { booking });

            var handler = new GetUserBookingsQueryHandler(_repo.Object);
            var result  = await handler.Handle(new GetUserBookingsQuery("user-1"), CancellationToken.None);

            Assert.Equal(string.Empty, result[0].RoomName);
            Assert.Equal(string.Empty, result[0].OfficeName);
        }

        // ── Handle_CancelledBooking_MapsIsCancelledTrue ───────────────────────────

        [Fact]
        public async Task Handle_CancelledBooking_MapsIsCancelledTrue()
        {
            var booking = BuildBooking(isCancelled: true);
            _repo.Setup(r => r.GetByUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(new List<Booking> { booking });

            var handler = new GetUserBookingsQueryHandler(_repo.Object);
            var result  = await handler.Handle(new GetUserBookingsQuery("user-1"), CancellationToken.None);

            Assert.True(result[0].IsCancelled);
        }

        // ── Handle_MultipleBookings_ReturnsAllMapped ──────────────────────────────

        [Fact]
        public async Task Handle_MultipleBookings_ReturnsAllMapped()
        {
            var bookings = new List<Booking>
            {
                BuildBooking(title: "Morning sync"),
                BuildBooking(title: "Design review"),
                BuildBooking(title: "Retro", isCancelled: true),
            };

            _repo.Setup(r => r.GetByUserIdAsync("user-1", It.IsAny<CancellationToken>()))
                 .ReturnsAsync(bookings);

            var handler = new GetUserBookingsQueryHandler(_repo.Object);
            var result  = await handler.Handle(new GetUserBookingsQuery("user-1"), CancellationToken.None);

            Assert.Equal(3, result.Count);
        }
    }
}
