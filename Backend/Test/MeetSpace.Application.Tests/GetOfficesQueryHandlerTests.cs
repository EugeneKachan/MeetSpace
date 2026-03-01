using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

            Assert.Single(result);
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

            Assert.Single(result);
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

            var dto = result.Single();
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

            var dto = result.Single();
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

            Assert.Empty(result);
        }
    }
}
