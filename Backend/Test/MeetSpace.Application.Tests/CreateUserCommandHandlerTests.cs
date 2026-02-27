using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using MeetSpace.Application.Features.Users.CreateUser;
using MeetSpace.Domain.Entities;

namespace MeetSpace.Application.Tests
{
    public class CreateUserCommandHandlerTests
    {
        private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task Handle_EmailAlreadyExists_ThrowsInvalidOperationException()
        {
            var userManager = CreateUserManagerMock();
            userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(new ApplicationUser { Email = "existing@example.com" });

            var handler = new CreateUserCommandHandler(userManager.Object);

            var cmd = new CreateUserCommand("A","B","existing@example.com","P@ssw0rd","Admin");

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_CreateFails_ThrowsInvalidOperationException()
        {
            var userManager = CreateUserManagerMock();
            userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "create failed" }));

            var handler = new CreateUserCommandHandler(userManager.Object);
            var cmd = new CreateUserCommand("A","B","new@example.com","P@ss","User");

            await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Success_ReturnsCreateUserResponse()
        {
            var userManager = CreateUserManagerMock();
            userManager.Setup(m => m.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((ApplicationUser?)null);
            userManager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // When CreateAsync is called, ensure the user.Id is set
            userManager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .Callback<ApplicationUser, string>((u, p) => u.Id = "generated-id")
                .ReturnsAsync(IdentityResult.Success);

            var handler = new CreateUserCommandHandler(userManager.Object);
            var cmd = new CreateUserCommand("First","Last","new@example.com","P@ssword","User", true);

            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.Equal("generated-id", result.Id);
            Assert.Equal("First", result.FirstName);
            Assert.Equal("Last", result.LastName);
            Assert.Equal("new@example.com", result.Email);
            Assert.Equal("User", result.Role);
        }
    }
}
