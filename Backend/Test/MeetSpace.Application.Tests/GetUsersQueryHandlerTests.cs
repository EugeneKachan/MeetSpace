using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using MeetSpace.Application.Features.Users.GetUsers;
using MeetSpace.Domain.Entities;

namespace MeetSpace.Application.Tests
{
    public class GetUsersQueryHandlerTests
    {
        private static Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task Handle_ReturnsUsersWithRoles()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", FirstName = "A", LastName = "Z", Email = "a@example.com", IsActive = true },
                new ApplicationUser { Id = "2", FirstName = "B", LastName = "Y", Email = "b@example.com", IsActive = false }
            };

            var userManager = CreateUserManagerMock();
            userManager.SetupGet(m => m.Users).Returns(users.AsQueryable());
            userManager.Setup(m => m.GetRolesAsync(It.Is<ApplicationUser>(u => u.Id == "1")))
                .ReturnsAsync(new List<string> { "Admin" });
            userManager.Setup(m => m.GetRolesAsync(It.Is<ApplicationUser>(u => u.Id == "2")))
                .ReturnsAsync(new List<string>());

            var handler = new GetUsersQueryHandler(userManager.Object);

            var result = await handler.Handle(new GetUsersQuery(), CancellationToken.None);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, u => u.Id == "1" && u.Role == "Admin");
            Assert.Contains(result, u => u.Id == "2" && u.Role == "");
        }
    }
}
