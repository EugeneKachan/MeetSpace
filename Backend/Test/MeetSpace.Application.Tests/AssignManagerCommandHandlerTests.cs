using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetSpace.Application.Features.Offices.AssignManager;
using MeetSpace.Application.Interfaces;
using MeetSpace.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace MeetSpace.Application.Tests
{
    public class AssignManagerCommandHandlerTests
    {
        private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private static AssignManagerCommandHandler BuildHandler(
            Mock<IOfficeRepository> repo,
            Mock<UserManager<ApplicationUser>> userManager)
            => new AssignManagerCommandHandler(repo.Object, userManager.Object);

        [Fact]
        public async Task Handle_OfficeNotFound_ThrowsKeyNotFoundException()
        {
            var repo = new Mock<IOfficeRepository>();
            repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Office?)null);

            var userManager = CreateUserManagerMock();
            var handler = BuildHandler(repo, userManager);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => handler.Handle(new AssignManagerCommand(Guid.NewGuid(), "user-1"), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
        {
            var officeId = Guid.NewGuid();
            var repo = new Mock<IOfficeRepository>();
            repo.Setup(r => r.GetByIdAsync(officeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Office { Id = officeId });

            var userManager = CreateUserManagerMock();
            userManager.Setup(m => m.FindByIdAsync("user-99"))
                .ReturnsAsync((ApplicationUser?)null);

            var handler = BuildHandler(repo, userManager);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => handler.Handle(new AssignManagerCommand(officeId, "user-99"), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_UserNotOfficeManager_ThrowsInvalidOperationException()
        {
            var officeId = Guid.NewGuid();
            var repo = new Mock<IOfficeRepository>();
            repo.Setup(r => r.GetByIdAsync(officeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Office { Id = officeId });

            var user = new ApplicationUser { Id = "user-1", Email = "user@test.com" };
            var userManager = CreateUserManagerMock();
            userManager.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);
            userManager.Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Employee" });

            var handler = BuildHandler(repo, userManager);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.Handle(new AssignManagerCommand(officeId, "user-1"), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_AlreadyAssigned_ThrowsInvalidOperationException()
        {
            var officeId = Guid.NewGuid();
            var repo = new Mock<IOfficeRepository>();
            repo.Setup(r => r.GetByIdAsync(officeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Office { Id = officeId });
            repo.Setup(r => r.AssignmentExistsAsync(officeId, "user-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var user = new ApplicationUser { Id = "user-1", Email = "mgr@test.com" };
            var userManager = CreateUserManagerMock();
            userManager.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);
            userManager.Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "OfficeManager" });

            var handler = BuildHandler(repo, userManager);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => handler.Handle(new AssignManagerCommand(officeId, "user-1"), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Success_AddsAssignment()
        {
            var officeId = Guid.NewGuid();
            var repo = new Mock<IOfficeRepository>();
            repo.Setup(r => r.GetByIdAsync(officeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Office { Id = officeId });
            repo.Setup(r => r.AssignmentExistsAsync(officeId, "user-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            repo.Setup(r => r.AddAssignmentAsync(It.IsAny<OfficeAssignment>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var user = new ApplicationUser { Id = "user-1", Email = "mgr@test.com" };
            var userManager = CreateUserManagerMock();
            userManager.Setup(m => m.FindByIdAsync("user-1")).ReturnsAsync(user);
            userManager.Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "OfficeManager" });

            var handler = BuildHandler(repo, userManager);
            await handler.Handle(new AssignManagerCommand(officeId, "user-1"), CancellationToken.None);

            repo.Verify(r => r.AddAssignmentAsync(
                It.Is<OfficeAssignment>(a => a.OfficeId == officeId && a.UserId == "user-1"),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
