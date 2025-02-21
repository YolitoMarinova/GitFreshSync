using GitFreshSync.API.Controllers;
using GitFreshSync.Application.Dtos.Sync;
using GitFreshSync.Application.Sync.Commands.SyncGitHubToFreshdeskCommand;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace GitFreshSync.Tests.API.Controllers
{
    public class SyncControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly SyncController _controller;

        public SyncControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new SyncController(_mediatorMock.Object);
        }

        [Fact]
        public async Task SyncGitHubToFreshdesk_ShouldReturnOk_WhenSyncIsSuccessful()
        {
            // Arrange
            var request = new SyncRequestDto
            {
                GitHubUsername = "testuser",
                FreshdeskSubdomain = "testdomain"
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<SyncGitHubToFreshdeskCommand>(), default))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.SyncGitHubToFreshdesk(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.True((bool)okResult.Value!);
        }

        [Fact]
        public async Task SyncGitHubToFreshdesk_ShouldReturnBadRequest_WhenSyncFails()
        {
            // Arrange
            var request = new SyncRequestDto
            {
                GitHubUsername = "testuser",
                FreshdeskSubdomain = "testdomain"
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<SyncGitHubToFreshdeskCommand>(), default))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.SyncGitHubToFreshdesk(request);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.False((bool)badRequestResult.Value!);
        }
    }
}
