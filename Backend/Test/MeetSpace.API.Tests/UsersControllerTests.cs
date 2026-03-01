using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MeetSpace.API.Controllers;
using MeetSpace.Application.Common;
using MeetSpace.Application.Features.Users.CreateUser;
using MeetSpace.Application.Features.Users.GetUsers;
using MeetSpace.Application.Features.Users.UpdateUser;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace MeetSpace.API.Tests
{
    public class UsersControllerTests
    {
        private readonly Mock<IMediator> _mediator = new Mock<IMediator>();
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _controller = new UsersController(_mediator.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsOkWithUsers()
        {
            var users = new List<UserDto>
            {
                new UserDto("u1", "Alice", "Smith", "alice@example.com", "Admin", true, DateTime.UtcNow)
            };
            var paged = new PagedResult<UserDto>(users, 1, 1, 10);

            _mediator
                .Setup(m => m.Send(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(paged);

            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, ok.Value);
        }

        [Fact]
        public async Task Create_ReturnsCreatedAtAction()
        {
            var cmd = new CreateUserCommand("A", "B", "a@b.com", "P@ssw0rd1", "Employee", true);
            var resp = new CreateUserResponse("id-1", "A", "B", "a@b.com", "Employee", true, DateTime.UtcNow);

            _mediator
                .Setup(m => m.Send(It.IsAny<CreateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resp);

            var result = await _controller.Create(cmd);

            var created = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(resp, created.Value);
            Assert.Equal("Create", created.ActionName);
            Assert.True(created.RouteValues.ContainsKey("id"));
            Assert.Equal(resp.Id, created.RouteValues["id"]);
        }

        [Fact]
        public async Task Update_IdMismatch_ReturnsBadRequest()
        {
            var cmd = new UpdateUserCommand("id-1", "A", "B", "a@b.com", "Employee", true);

            var result = await _controller.Update("different-id", cmd);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task Update_ReturnsOkOnSuccess()
        {
            var cmd = new UpdateUserCommand("id-1", "A", "B", "a@b.com", "Employee", true);
            var resp = new UpdateUserResponse("id-1", "A", "B", "a@b.com", "Employee", true, DateTime.UtcNow);

            _mediator
                .Setup(m => m.Send(It.IsAny<UpdateUserCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(resp);

            var result = await _controller.Update("id-1", cmd);

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(resp, ok.Value);
        }

        // ── Pagination param forwarding ───────────────────────────────────────────

        [Fact]
        public async Task GetAll_ForwardsPaginationParamsToQuery()
        {
            var paged = new PagedResult<UserDto>(new List<UserDto>(), 0, 2, 5);
            _mediator
                .Setup(m => m.Send(
                    It.Is<GetUsersQuery>(q =>
                        q.Page     == 2      &&
                        q.PageSize == 5      &&
                        q.Search   == "alice" &&
                        q.SortBy   == "email" &&
                        q.SortDir  == "desc"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(paged);

            var result = await _controller.GetAll(
                page: 2, pageSize: 5, search: "alice", sortBy: "email", sortDir: "desc");

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, ok.Value);
            _mediator.Verify(
                m => m.Send(
                    It.Is<GetUsersQuery>(q =>
                        q.Page == 2 && q.PageSize == 5 &&
                        q.Search == "alice" && q.SortBy == "email" && q.SortDir == "desc"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
