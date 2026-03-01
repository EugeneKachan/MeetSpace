using System;
using System.Threading;
using System.Threading.Tasks;
using MeetSpace.Application.Features.Offices.UpdateOffice;
using MeetSpace.Application.Interfaces;
using MeetSpace.Domain.Entities;
using Moq;
using Xunit;

namespace MeetSpace.Application.Tests
{
    public class UpdateOfficeCommandHandlerTests
    {
        private readonly Mock<IOfficeRepository> _repo = new Mock<IOfficeRepository>();

        [Fact]
        public async Task Handle_OfficeNotFound_ThrowsKeyNotFoundException()
        {
            _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Office?)null);

            var handler = new UpdateOfficeCommandHandler(_repo.Object);
            var cmd = new UpdateOfficeCommand(Guid.NewGuid(), "New Name", "New Address");

            await Assert.ThrowsAsync<KeyNotFoundException>(() => handler.Handle(cmd, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Success_UpdatesNameAndAddress()
        {
            var officeId = Guid.NewGuid();
            var office = new Office { Id = officeId, Name = "Old Name", Address = "Old Address" };

            _repo.Setup(r => r.GetByIdAsync(officeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(office);
            _repo.Setup(r => r.UpdateAsync(It.IsAny<Office>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new UpdateOfficeCommandHandler(_repo.Object);
            var cmd = new UpdateOfficeCommand(officeId, "New Name", "New Address");

            await handler.Handle(cmd, CancellationToken.None);

            Assert.Equal("New Name", office.Name);
            Assert.Equal("New Address", office.Address);
            _repo.Verify(r => r.UpdateAsync(office, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
