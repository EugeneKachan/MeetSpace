using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MeetSpace.API.Controllers;
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

            _mediator
                .Setup(m => m.Send(It.IsAny<GetUsersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UserDto>)users);

            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(users, ok.Value);
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
    }
}
