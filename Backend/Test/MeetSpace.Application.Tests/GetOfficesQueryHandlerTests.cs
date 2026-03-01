using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MeetSpace.Application.Common;
using MeetSpace.Application.Features.Offices;
using MeetSpace.Application.Features.Offices.GetOffices;
using MeetSpace.Application.Interfaces;
using MeetSpace.Domain.Entities;
using Moq;
using Xunit;

namespace MeetSpace.Application.Tests
{
    public class GetOfficesQueryHandlerTests
    {
        private readonly Mock<IOfficeRepository> _repo = new Mock<IOfficeRepository>();

        private static Office BuildOffice(string name = "HQ", string address = "1 Main St")
        {
            var officeId = Guid.NewGuid();
            return new Office
            {
                Id = officeId,
                Name = name,
                Address = address,
                IsActive = true,
                Rooms = new List<Room>
                {
                    new Room { Id = Guid.NewGuid(), Name = "Room A", Capacity = 5, Description = "Small", IsActive = true }
                },
                Assignments = new List<OfficeAssignment>
                {
                    new OfficeAssignment
                    {
                        OfficeId = officeId,
                        UserId = "mgr-1",
                        User = new ApplicationUser { Id = "mgr-1", FirstName = "Alice", LastName = "Smith", Email = "alice@test.com" }
                    }
                }
            };
        }

        [Fact]
        public async Task Handle_NoFilter_CallsGetAllAsync()
        {
            var offices = new List<Office> { BuildOffice() };
            _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(offices);

            var handler = new GetOfficesQueryHandler(_repo.Object);
            var result = await handler.Handle(new GetOfficesQuery(), CancellationToken.None);

            Assert.Single(result.Items);
            _repo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
            _repo.Verify(r => r.GetByAssignedUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_WithFilter_CallsGetByAssignedUserAsync()
        {
            var offices = new List<Office> { BuildOffice() };
            _repo.Setup(r => r.GetByAssignedUserAsync("user-1", It.IsAny<CancellationToken>()))
                .ReturnsAsync(offices);

            var handler = new GetOfficesQueryHandler(_repo.Object);
            var result = await handler.Handle(new GetOfficesQuery("user-1"), CancellationToken.None);

            Assert.Single(result.Items);
            _repo.Verify(r => r.GetByAssignedUserAsync("user-1", It.IsAny<CancellationToken>()), Times.Once);
            _repo.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_MapsOfficeDtoCorrectly()
        {
            var office = BuildOffice("Branch", "5 Oak Ave");
            _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Office> { office });

            var handler = new GetOfficesQueryHandler(_repo.Object);
            var result = await handler.Handle(new GetOfficesQuery(), CancellationToken.None);

            var dto = result.Items.Single();
            Assert.Equal(office.Id, dto.Id);
            Assert.Equal("Branch", dto.Name);
            Assert.Equal("5 Oak Ave", dto.Address);
            Assert.True(dto.IsActive);
        }

        [Fact]
        public async Task Handle_MapsRoomsAndManagersCorrectly()
        {
            var office = BuildOffice();
            _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Office> { office });

            var handler = new GetOfficesQueryHandler(_repo.Object);
            var result = await handler.Handle(new GetOfficesQuery(), CancellationToken.None);

            var dto = result.Items.Single();
            Assert.Single(dto.Rooms);
            Assert.Equal("Room A", dto.Rooms[0].Name);
            Assert.Equal(5, dto.Rooms[0].Capacity);

            Assert.Single(dto.Managers);
            Assert.Equal("mgr-1", dto.Managers[0].Id);
            Assert.Equal("Alice", dto.Managers[0].FirstName);
            Assert.Equal("alice@test.com", dto.Managers[0].Email);
        }

        [Fact]
        public async Task Handle_EmptyRepository_ReturnsEmptyList()
        {
            _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Office>());

            var handler = new GetOfficesQueryHandler(_repo.Object);
            var result = await handler.Handle(new GetOfficesQuery(), CancellationToken.None);

            Assert.Empty(result.Items);
        }

        // ── Search ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_SearchByName_FiltersCorrectly()
        {
            var offices = new List<Office>
            {
                BuildOffice("Alpha HQ",     "1 Street"),
                BuildOffice("Beta Office",  "2 Avenue"),
                BuildOffice("Alpha Branch", "3 Road"),
            };
            _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(offices);

            var handler = new GetOfficesQueryHandler(_repo.Object);
            var result = await handler.Handle(new GetOfficesQuery(Search: "alpha"), CancellationToken.None);

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
            Assert.All(result.Items, o =>
                Assert.Contains("alpha", o.Name, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Handle_SearchByAddress_FiltersCorrectly()
        {
            var offices = new List<Office>
            {
                BuildOffice("Office A", "1 Oak Lane"),
                BuildOffice("Office B", "2 Elm Road"),
                BuildOffice("Office C", "3 Oak Street"),
            };
            _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(offices);

            var handler = new GetOfficesQueryHandler(_repo.Object);
            var result = await handler.Handle(new GetOfficesQuery(Search: "oak"), CancellationToken.None);

            Assert.Equal(2, result.TotalCount);
            Assert.All(result.Items, o =>
                Assert.Contains("oak", o.Address, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task Handle_Search_IsCaseInsensitive()
        {
            var offices = new List<Office>
            {
                BuildOffice("hq office", "1 Street"),
                BuildOffice("Branch",    "2 Avenue"),
            };
            _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(offices);

            var handler = new GetOfficesQueryHandler(_repo.Object);
            var result = await handler.Handle(new GetOfficesQuery(Search: "HQ"), CancellationToken.None);

            Assert.Equal(1, result.TotalCount);
            Assert.Equal("hq office", result.Items[0].Name);
        }

        // ── Paging ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_Paging_ReturnsCorrectSliceAndTotalCount()
        {
            // Default sort is name asc → Alpha, Beta, Delta, Epsilon, Gamma
            var offices = new List<Office>
            {
                BuildOffice("Gamma",   "5 St"),
                BuildOffice("Alpha",   "1 St"),
                BuildOffice("Delta",   "3 St"),
                BuildOffice("Beta",    "2 St"),
                BuildOffice("Epsilon", "4 St"),
            };
            _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(offices);

            var handler = new GetOfficesQueryHandler(_repo.Object);
            // page 2, pageSize 2 → skip 2, take 2 → Delta, Epsilon
            var result = await handler.Handle(
                new GetOfficesQuery(Page: 2, PageSize: 2, SortBy: "name", SortDir: "asc"),
                CancellationToken.None);

            Assert.Equal(5,       result.TotalCount);
            Assert.Equal(2,       result.Items.Count);
            Assert.Equal(2,       result.Page);
            Assert.Equal("Delta", result.Items[0].Name);
            Assert.Equal("Epsilon", result.Items[1].Name);
        }

        [Fact]
        public async Task Handle_TotalCount_ReflectsFilteredCountBeforePaging()
        {
            var offices = new List<Office>
            {
                BuildOffice("London A", "1 High St"),
                BuildOffice("London B", "2 High St"),
                BuildOffice("Paris",    "3 Rue"),
                BuildOffice("London C", "4 High St"),
                BuildOffice("Berlin",   "5 Str"),
            };
            _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(offices);

            var handler = new GetOfficesQueryHandler(_repo.Object);
            // search matches 3 "London" offices, pageSize 2 → page 1 returns 2 items, TotalCount = 3
            var result = await handler.Handle(
                new GetOfficesQuery(Search: "london", Page: 1, PageSize: 2),
                CancellationToken.None);

            Assert.Equal(3, result.TotalCount);
            Assert.Equal(2, result.Items.Count);
        }

        // ── Sorting ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_SortByAddress_Ascending()
        {
            var offices = new List<Office>
            {
                BuildOffice("C", "3 Zebra Rd"),
                BuildOffice("A", "1 Apple St"),
                BuildOffice("B", "2 Mango Ave"),
            };
            _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(offices);

            var handler = new GetOfficesQueryHandler(_repo.Object);
            var result = await handler.Handle(
                new GetOfficesQuery(SortBy: "address", SortDir: "asc"),
                CancellationToken.None);

            Assert.Equal("1 Apple St",  result.Items[0].Address);
            Assert.Equal("2 Mango Ave", result.Items[1].Address);
            Assert.Equal("3 Zebra Rd",  result.Items[2].Address);
        }

        [Fact]
        public async Task Handle_SortByAddress_Descending()
        {
            var offices = new List<Office>
            {
                BuildOffice("A", "1 Apple St"),
                BuildOffice("C", "3 Zebra Rd"),
                BuildOffice("B", "2 Mango Ave"),
            };
            _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(offices);

            var handler = new GetOfficesQueryHandler(_repo.Object);
            var result = await handler.Handle(
                new GetOfficesQuery(SortBy: "address", SortDir: "desc"),
                CancellationToken.None);

            Assert.Equal("3 Zebra Rd",  result.Items[0].Address);
            Assert.Equal("2 Mango Ave", result.Items[1].Address);
            Assert.Equal("1 Apple St",  result.Items[2].Address);
        }

        [Fact]
        public async Task Handle_DefaultSort_ByNameAscending()
        {
            var offices = new List<Office>
            {
                BuildOffice("Zeta",  "3 St"),
                BuildOffice("Alpha", "1 St"),
                BuildOffice("Mu",    "2 St"),
            };
            _repo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(offices);

            var handler = new GetOfficesQueryHandler(_repo.Object);
            var result = await handler.Handle(new GetOfficesQuery(), CancellationToken.None);

            Assert.Equal("Alpha", result.Items[0].Name);
            Assert.Equal("Mu",    result.Items[1].Name);
            Assert.Equal("Zeta",  result.Items[2].Name);
        }
    }
}
