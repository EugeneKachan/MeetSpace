using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MeetSpace.Application.Features.Offices.RemoveManager;
using MeetSpace.Application.Interfaces;
using MeetSpace.Domain.Entities;
using Moq;
using Xunit;

namespace MeetSpace.Application.Tests
{
    public class RemoveManagerCommandHandlerTests
    {
        private readonly Mock<IOfficeRepository> _repo = new Mock<IOfficeRepository>();

        [Fact]
        public async Task Handle_OfficeNotFound_ThrowsKeyNotFoundException()
        {
            _repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Office?)null);

            var handler = new RemoveManagerCommandHandler(_repo.Object);

            await Assert.ThrowsAsync<KeyNotFoundException>(
                () => handler.Handle(new RemoveManagerCommand(Guid.NewGuid(), "user-1"), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_Success_CallsRemoveAssignment()
        {
            var officeId = Guid.NewGuid();
            _repo.Setup(r => r.GetByIdAsync(officeId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Office { Id = officeId });
            _repo.Setup(r => r.RemoveAssignmentAsync(officeId, "user-1", It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var handler = new RemoveManagerCommandHandler(_repo.Object);
            await handler.Handle(new RemoveManagerCommand(officeId, "user-1"), CancellationToken.None);

            _repo.Verify(r => r.RemoveAssignmentAsync(officeId, "user-1", It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
