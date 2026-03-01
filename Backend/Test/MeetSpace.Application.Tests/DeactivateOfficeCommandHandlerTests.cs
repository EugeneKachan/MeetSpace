using System;
using System.Threading;
using System.Threading.Tasks;
using MeetSpace.Application.Features.Offices.DeactivateOffice;
using MeetSpace.Application.Interfaces;
using MeetSpace.Domain.Entities;
using Moq;
using Xunit;

namespace MeetSpace.Application.Tests
{
    public class DeactivateOfficeCommandHandlerTests
    {
        private readonly Mock<IOfficeRepository> _repo = new Mock<IOfficeRepository>();

        [Fact]
        public async Task Handle_OfficeNotFound_ThrowsKeyNotFoundException()
        {
            _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Office?)null);

            var handler = new DeactivateOfficeCommandHandler(_repo.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => handler.Handle(new DeactivateOfficeCommand(Guid.NewGuid()), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Success_SetsIsActiveFalseAndSaves()
        {
            var officeId = Guid.NewGuid();
            var office = new Office { Id = officeId, Name = "HQ", IsActive = true };

            _repo.Setup(r => r.GetByIdAsync(officeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(office);
            _repo.Setup(r => r.UpdateAsync(It.IsAny<Office>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new DeactivateOfficeCommandHandler(_repo.Object);
            await handler.Handle(new DeactivateOfficeCommand(officeId), CancellationToken.None);

            Assert.False(office.IsActive);
            _repo.Verify(r => r.UpdateAsync(office, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
