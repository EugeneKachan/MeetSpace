#nullable enable
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MeetSpace.API.Controllers;
using MeetSpace.Application.Features.Offices;
using MeetSpace.Application.Features.Offices.AssignManager;
using MeetSpace.Application.Features.Offices.CreateOffice;
using MeetSpace.Application.Features.Offices.DeactivateOffice;
using MeetSpace.Application.Features.Offices.GetOffices;
using MeetSpace.Application.Features.Offices.RemoveManager;
using MeetSpace.Application.Features.Offices.UpdateOffice;
using MeetSpace.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace MeetSpace.API.Tests
{
    public class OfficesControllerTests
    {
        private readonly Mock<IMediator> _mediator = new Mock<IMediator>();
        private readonly OfficesController _controller;

        public OfficesControllerTests()
        {
            _controller = new OfficesController(_mediator.Object);
            SetUserRole("Admin");
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private void SetUserRole(string role, string? userId = null)
        {
            var claims = new List<Claim> { new Claim(ClaimTypes.Role, role) };
            if (userId is not null)
                claims.Add(new Claim("sub", userId));

            var identity = new ClaimsIdentity(claims, "Test");
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
            };
        }

        private static IReadOnlyList<OfficeDto> BuildOfficeDtos(int count = 1)
        {
            var list = new List<OfficeDto>();
            for (var i = 0; i < count; i++)
                list.Add(new OfficeDto(Guid.NewGuid(), $"Office {i}", $"Addr {i}", true,
                    new List<RoomDto>(), new List<ManagerDto>()));
            return list;
        }

        // ── GetAll ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_Admin_PassesNullFilterAndReturnsOk()
        {
            SetUserRole(UserRoles.Admin);
            var dtos = BuildOfficeDtos(2);
            _mediator.Setup(m => m.Send(It.Is<GetOfficesQuery>(q => q.FilterByUserId == null), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dtos);

            var result = await _controller.GetAll(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dtos, ok.Value);
        }

        [Fact]
        public async Task GetAll_OfficeManager_FiltersById()
        {
            SetUserRole(UserRoles.OfficeManager, userId: "mgr-42");
            var dtos = BuildOfficeDtos(1);
            _mediator.Setup(m => m.Send(It.Is<GetOfficesQuery>(q => q.FilterByUserId == "mgr-42"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(dtos);

            var result = await _controller.GetAll(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(dtos, ok.Value);
        }

        // ── Create ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Create_ReturnsCreated()
        {
            var newId = Guid.NewGuid();
            var cmd = new CreateOfficeCommand("New Office", "1 Street", new List<CreateRoomRequest>());
            _mediator.Setup(m => m.Send(It.IsAny<CreateOfficeCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(newId);

            var result = await _controller.Create(cmd, CancellationToken.None);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(newId, created.Value);
            Assert.Equal(nameof(_controller.GetAll), created.ActionName);
        }

        // ── Update ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            var cmd = new UpdateOfficeCommand(Guid.NewGuid(), "Name", "Addr");

            var result = await _controller.Update(Guid.NewGuid(), cmd, CancellationToken.None);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Update_Success_ReturnsNoContent()
        {
            var id = Guid.NewGuid();
            var cmd = new UpdateOfficeCommand(id, "Updated", "New Addr");
            _mediator.Setup(m => m.Send(It.IsAny<UpdateOfficeCommand>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(default(Unit)));

            var result = await _controller.Update(id, cmd, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        // ── Deactivate ───────────────────────────────────────────────────────────

        [Fact]
        public async Task Deactivate_ReturnsNoContent()
        {
            var id = Guid.NewGuid();
            _mediator.Setup(m => m.Send(It.Is<DeactivateOfficeCommand>(c => c.Id == id), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(default(Unit)));

            var result = await _controller.Deactivate(id, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        // ── AssignManager ────────────────────────────────────────────────────────

        [Fact]
        public async Task AssignManager_ReturnsNoContent()
        {
            var officeId = Guid.NewGuid();
            _mediator.Setup(m => m.Send(
                    It.Is<AssignManagerCommand>(c => c.OfficeId == officeId && c.UserId == "user-1"),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(default(Unit)));

            var result = await _controller.AssignManager(officeId, new AssignManagerRequest("user-1"), CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        // ── RemoveManager ────────────────────────────────────────────────────────

        [Fact]
        public async Task RemoveManager_ReturnsNoContent()
        {
            var officeId = Guid.NewGuid();
            _mediator.Setup(m => m.Send(
                    It.Is<RemoveManagerCommand>(c => c.OfficeId == officeId && c.UserId == "user-1"),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(default(Unit)));

            var result = await _controller.RemoveManager(officeId, "user-1", CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }
    }
}
