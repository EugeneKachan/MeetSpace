using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using MeetSpace.Application.Common;
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

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
            Assert.Contains(result.Items, u => u.Id == "1" && u.Role == "Admin");
            Assert.Contains(result.Items, u => u.Id == "2" && u.Role == "");
        }

        // ── Search ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_Search_FiltersByFirstName()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", FirstName = "Alice",  LastName = "Brown", Email = "a@x.com" },
                new ApplicationUser { Id = "2", FirstName = "Bob",    LastName = "Green", Email = "b@x.com" },
                new ApplicationUser { Id = "3", FirstName = "Alicia", LastName = "White", Email = "c@x.com" },
            };
            var userManager = CreateUserManagerMock();
            userManager.SetupGet(m => m.Users).Returns(users.AsQueryable());
            foreach (var u in users)
                userManager.Setup(m => m.GetRolesAsync(u)).ReturnsAsync(new List<string>());

            var handler = new GetUsersQueryHandler(userManager.Object);
            var result = await handler.Handle(new GetUsersQuery(Search: "ali"), CancellationToken.None);

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
            Assert.All(result.Items, u =>
                Assert.Contains("ali", u.FirstName, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Handle_Search_FiltersByEmail()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", FirstName = "Alice", LastName = "A", Email = "alice@company.com" },
                new ApplicationUser { Id = "2", FirstName = "Bob",   LastName = "B", Email = "bob@other.com" },
            };
            var userManager = CreateUserManagerMock();
            userManager.SetupGet(m => m.Users).Returns(users.AsQueryable());
            foreach (var u in users)
                userManager.Setup(m => m.GetRolesAsync(u)).ReturnsAsync(new List<string>());

            var handler = new GetUsersQueryHandler(userManager.Object);
            var result = await handler.Handle(new GetUsersQuery(Search: "company.com"), CancellationToken.None);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal("1", result.Items[0].Id);
        }

        // ── Paging ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_Paging_ReturnsCorrectPageAndTotalCount()
        {
            var users = Enumerable.Range(1, 5).Select(i => new ApplicationUser
            {
                Id        = i.ToString(),
                FirstName = $"User{i:D2}",
                LastName  = "Z",
                Email     = $"u{i}@x.com",
                IsActive  = true
            }).ToList();

            var userManager = CreateUserManagerMock();
            userManager.SetupGet(m => m.Users).Returns(users.AsQueryable());
            foreach (var u in users)
                userManager.Setup(m => m.GetRolesAsync(u)).ReturnsAsync(new List<string>());

            var handler = new GetUsersQueryHandler(userManager.Object);
            // page 2, pageSize 2 → items at index 2 and 3
            var result = await handler.Handle(
                new GetUsersQuery(Page: 2, PageSize: 2, SortBy: "firstName", SortDir: "asc"),
                CancellationToken.None);

            Assert.Equal(5, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.Page);
            Assert.Equal(2, result.PageSize);
        }

        [Fact]
        public async Task Handle_TotalCount_ReflectsFilteredCountBeforePaging()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", FirstName = "Smith",   LastName = "A", Email = "s1@x.com" },
                new ApplicationUser { Id = "2", FirstName = "Jones",   LastName = "B", Email = "j@x.com"  },
                new ApplicationUser { Id = "3", FirstName = "Smith",   LastName = "C", Email = "s2@x.com" },
                new ApplicationUser { Id = "4", FirstName = "Smithson",LastName = "D", Email = "s3@x.com" },
                new ApplicationUser { Id = "5", FirstName = "Brown",   LastName = "E", Email = "b@x.com"  },
            };
            var userManager = CreateUserManagerMock();
            userManager.SetupGet(m => m.Users).Returns(users.AsQueryable());
            foreach (var u in users)
                userManager.Setup(m => m.GetRolesAsync(u)).ReturnsAsync(new List<string>());

            var handler = new GetUsersQueryHandler(userManager.Object);
            // "smith" matches ids 1, 3, 4 → TotalCount=3; page 1, pageSize 2 → 2 items
            var result = await handler.Handle(
                new GetUsersQuery(Search: "smith", Page: 1, PageSize: 2),
                CancellationToken.None);

            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
        }

        // ── Sorting ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_SortByFirstName_Ascending()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", FirstName = "Charlie", LastName = "X", Email = "c@x.com" },
                new ApplicationUser { Id = "2", FirstName = "Alice",   LastName = "X", Email = "a@x.com" },
                new ApplicationUser { Id = "3", FirstName = "Bob",     LastName = "X", Email = "b@x.com" },
            };
            var userManager = CreateUserManagerMock();
            userManager.SetupGet(m => m.Users).Returns(users.AsQueryable());
            foreach (var u in users)
                userManager.Setup(m => m.GetRolesAsync(u)).ReturnsAsync(new List<string>());

            var handler = new GetUsersQueryHandler(userManager.Object);
            var result = await handler.Handle(
                new GetUsersQuery(SortBy: "firstName", SortDir: "asc"),
                CancellationToken.None);

            Assert.Equal("Alice",   result.Items[0].FirstName);
            Assert.Equal("Bob",     result.Items[1].FirstName);
            Assert.Equal("Charlie", result.Items[2].FirstName);
        }

        [Fact]
        public async Task Handle_SortByFirstName_Descending()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", FirstName = "Charlie", LastName = "X", Email = "c@x.com" },
                new ApplicationUser { Id = "2", FirstName = "Alice",   LastName = "X", Email = "a@x.com" },
                new ApplicationUser { Id = "3", FirstName = "Bob",     LastName = "X", Email = "b@x.com" },
            };
            var userManager = CreateUserManagerMock();
            userManager.SetupGet(m => m.Users).Returns(users.AsQueryable());
            foreach (var u in users)
                userManager.Setup(m => m.GetRolesAsync(u)).ReturnsAsync(new List<string>());

            var handler = new GetUsersQueryHandler(userManager.Object);
            var result = await handler.Handle(
                new GetUsersQuery(SortBy: "firstName", SortDir: "desc"),
                CancellationToken.None);

            Assert.Equal("Charlie", result.Items[0].FirstName);
            Assert.Equal("Bob",     result.Items[1].FirstName);
            Assert.Equal("Alice",   result.Items[2].FirstName);
        }

        [Fact]
        public async Task Handle_DefaultSort_ByLastNameThenFirstName_Ascending()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", FirstName = "Charlie", LastName = "Brown", Email = "c@x.com" },
                new ApplicationUser { Id = "2", FirstName = "Alice",   LastName = "Zebra", Email = "a@x.com" },
                new ApplicationUser { Id = "3", FirstName = "Bob",     LastName = "Brown", Email = "b@x.com" },
            };
            var userManager = CreateUserManagerMock();
            userManager.SetupGet(m => m.Users).Returns(users.AsQueryable());
            foreach (var u in users)
                userManager.Setup(m => m.GetRolesAsync(u)).ReturnsAsync(new List<string>());

            var handler = new GetUsersQueryHandler(userManager.Object);
            // Default sort: lastName asc then firstName asc
            // Expected: Brown/Bob (Id=3), Brown/Charlie (Id=1), Zebra/Alice (Id=2)
            var result = await handler.Handle(new GetUsersQuery(), CancellationToken.None);

            Assert.Equal("3", result.Items[0].Id);
            Assert.Equal("1", result.Items[1].Id);
            Assert.Equal("2", result.Items[2].Id);
        }
    }
}
