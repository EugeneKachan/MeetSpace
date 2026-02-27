using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using MeetSpace.Application.Features.Users.UpdateUser;
using MeetSpace.Domain.Entities;

namespace MeetSpace.Application.Tests
{
    public class UpdateUserCommandHandlerTests
    {
        private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task Handle_UserNotFound_ThrowsKeyNotFoundException()
        {
            var userManager = CreateUserManagerMock();
            userManager.Setup(m => m.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);

            var handler = new UpdateUserCommandHandler(userManager.Object);
            var cmd = new UpdateUserCommand("id","F","L","e@x.com","User", true);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_EmailTakenByOther_ThrowsInvalidOperationException()
        {
            var existingUser = new ApplicationUser { Id = "other", Email = "taken@example.com" };
            var user = new ApplicationUser { Id = "id", Email = "original@example.com" };

            var userManager = CreateUserManagerMock();
            userManager.Setup(m => m.FindByIdAsync("id")).ReturnsAsync(user);
            userManager.Setup(m => m.FindByEmailAsync("taken@example.com")).ReturnsAsync(existingUser);

            var handler = new UpdateUserCommandHandler(userManager.Object);
            var cmd = new UpdateUserCommand("id","F","L","taken@example.com","User", true);

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Success_UpdatesAndReturnsResponse()
        {
            var user = new ApplicationUser { Id = "id", Email = "old@example.com", FirstName = "Old", LastName = "Name", CreatedAt = DateTime.UtcNow };

            var userManager = CreateUserManagerMock();
            userManager.Setup(m => m.FindByIdAsync("id")).ReturnsAsync(user);
            userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            userManager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<string> { "OldRole" });
            userManager.Setup(m => m.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            var handler = new UpdateUserCommandHandler(userManager.Object);
            var cmd = new UpdateUserCommand("id","NewFirst","NewLast","new@example.com","NewRole", false);

            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.Equal("id", result.Id);
            Assert.Equal("NewFirst", result.FirstName);
            Assert.Equal("NewLast", result.LastName);
            Assert.Equal("new@example.com", result.Email);
            Assert.Equal("NewRole", result.Role);
            Assert.False(result.IsActive);
        }
    }
}
