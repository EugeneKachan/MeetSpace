#nullable enable
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MeetSpace.API.Controllers;
using MeetSpace.Application.Features.Bookings.CancelBooking;
using MeetSpace.Application.Features.Bookings.CreateBooking;
using MeetSpace.Application.Features.Bookings.GetUserBookings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace MeetSpace.API.Tests
{
    public class BookingsControllerTests
    {
        private readonly Mock<IMediator> _mediator = new Mock<IMediator>();
        private readonly BookingsController _controller;

        public BookingsControllerTests()
        {
            _controller = new BookingsController(_mediator.Object);
            SetUser("user-1", isManager: false);
        }

        private void SetUser(string userId, bool isManager)
        {
            var claims = new[] { new Claim("sub", userId) };
            var identity = new ClaimsIdentity(claims, "Test",
                ClaimTypes.NameIdentifier, isManager ? "OfficeManager" : "Employee");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        [Fact]
        public async Task Create_ValidRequest_ReturnsCreated()
        {
            var newId = Guid.NewGuid();
            var req = new BookingsController.CreateBookingRequest(
                Guid.NewGuid(), Guid.NewGuid(),
                new DateOnly(2026, 6, 1),
                new TimeOnly(9, 0), new TimeOnly(10, 0),
                "Planning");

            _mediator.Setup(m => m.Send(It.IsAny<CreateBookingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newId);

            var result = await _controller.Create(req, CancellationToken.None);

            var created = Assert.IsType<CreatedResult>(result);
            Assert.Equal(newId, created.Value);
        }

        [Fact]
        public async Task Create_NullRequest_ReturnsBadRequest()
        {
            var result = await _controller.Create(null!, CancellationToken.None);

            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task Cancel_ValidId_ReturnsNoContent()
        {
            var id = Guid.NewGuid();
            _mediator.Setup(m => m.Send(It.IsAny<CancelBookingCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(MediatR.Unit.Value);

            var result = await _controller.Cancel(id, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Cancel_SendsCommandWithCorrectUserId()
        {
            var id = Guid.NewGuid();
            CancelBookingCommand? captured = null;
            _mediator.Setup(m => m.Send(It.IsAny<CancelBookingCommand>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<MediatR.Unit>, CancellationToken>((cmd, _) => captured = (CancelBookingCommand)cmd)
                .ReturnsAsync(MediatR.Unit.Value);

            SetUser("test-user", isManager: false);
            await _controller.Cancel(id, CancellationToken.None);

            Assert.NotNull(captured);
            Assert.Equal(id, captured!.Id);
            Assert.Equal("test-user", captured.RequestingUserId);
            Assert.False(captured.IsManagerOrAdmin);
        }

        // ── GetMine ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetMine_ReturnsOkWithBookings()
        {
            var dtos = new List<BookingSummaryDto>
            {
                new BookingSummaryDto(Guid.NewGuid(), Guid.NewGuid(),
                    "Room A", "HQ", "2026-06-01", "09:00", "10:00", "Stand-up", false)
            };
            _mediator.Setup(m => m.Send(It.IsAny<GetUserBookingsQuery>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(dtos);

            var result = await _controller.GetMine(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dtos, ok.Value);
        }

        [Fact]
        public async Task GetMine_SendsQueryWithCurrentUserId()
        {
            SetUser("current-user", isManager: false);
            GetUserBookingsQuery? captured = null;
            _mediator
                .Setup(m => m.Send(It.IsAny<GetUserBookingsQuery>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<List<BookingSummaryDto>>, CancellationToken>(
                    (q, _) => captured = (GetUserBookingsQuery)q)
                .ReturnsAsync(new List<BookingSummaryDto>());

            await _controller.GetMine(CancellationToken.None);

            Assert.NotNull(captured);
            Assert.Equal("current-user", captured!.UserId);
        }
    }
}
