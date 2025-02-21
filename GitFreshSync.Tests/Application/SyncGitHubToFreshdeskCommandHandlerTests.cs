using GitFreshSync.Application.Dtos.Freshdesk;
using GitFreshSync.Application.Dtos.GitHub;
using GitFreshSync.Application.Exceptions.Github;
using GitFreshSync.Application.Interfaces;
using GitFreshSync.Application.Sync.Commands.SyncGitHubToFreshdeskCommand;
using MediatR;
using Moq;
using Xunit;

namespace GitFreshSync.Application.Tests
{
    public class SyncGitHubToFreshdeskCommandHandlerTests
    {
        private readonly Mock<IGitHubService> _gitHubServiceMock;
        private readonly Mock<IFreshdeskService> _freshdeskServiceMock;
        private readonly IRequestHandler<SyncGitHubToFreshdeskCommand, bool> _handler;

        public SyncGitHubToFreshdeskCommandHandlerTests()
        {
            _gitHubServiceMock = new Mock<IGitHubService>();
            _freshdeskServiceMock = new Mock<IFreshdeskService>();

            _handler = new SyncGitHubToFreshdeskCommandHandler(_gitHubServiceMock.Object, _freshdeskServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldSyncGitHubUserToFreshdesk_WhenUserExists()
        {
            // Arrange
            var command = new SyncGitHubToFreshdeskCommand("testuser", "testdomain");

            var gitHubUser = new GitHubUserDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Company = "TestCompany",
                Bio = "Software Engineer"
            };

            var freshdeskCompany = new FreshdeskCompanyDto { Id = 123, Name = "TestCompany" };
            var freshdeskContact = new FreshdeskContactDto { Id = 456, Email = "test@example.com" };

            _gitHubServiceMock.Setup(s => s.GetGitHubUserAsync(command.GitHubUsername))
                .ReturnsAsync(gitHubUser);

            _freshdeskServiceMock.Setup(s => s.SearchCompanies(command.FreshdeskSubdomain, gitHubUser.Company))
                .ReturnsAsync(new FreshdeskCompanySearchResultDto { Companies = new[] { freshdeskCompany } });

            _freshdeskServiceMock.Setup(s => s.CreateOrUpdateContactAsync(command.FreshdeskSubdomain, It.IsAny<FreshdeskContactInputDto>()))
                .ReturnsAsync(freshdeskContact);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _gitHubServiceMock.Verify(s => s.GetGitHubUserAsync(command.GitHubUsername), Times.Once);
            _freshdeskServiceMock.Verify(s => s.CreateOrUpdateContactAsync(command.FreshdeskSubdomain, It.IsAny<FreshdeskContactInputDto>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenGitHubUserNotFound()
        {
            // Arrange
            var command = new SyncGitHubToFreshdeskCommand("nonexistentuser", "testdomain");

            _gitHubServiceMock.Setup(s => s.GetGitHubUserAsync(command.GitHubUsername))
                .ReturnsAsync((GitHubUserDto?)null);

            // Act & Assert
            await Assert.ThrowsAsync<GitHubUserNotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenGitHubUserHasNoEmail()
        {
            // Arrange
            var command = new SyncGitHubToFreshdeskCommand("nonexistentuser", "testdomain");

            var gitHubUser = new GitHubUserDto
            {
                Name = "Test User",
                Email = null,
                Company = "TestCompany",
                Bio = "Software Engineer"
            };

            _gitHubServiceMock.Setup(s => s.GetGitHubUserAsync(command.GitHubUsername))
                .ReturnsAsync(gitHubUser);

            // Act & Assert
            await Assert.ThrowsAsync<GitHubUserMissingPropertiesException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenGitHubUserHasNoName()
        {
            // Arrange
            var command = new SyncGitHubToFreshdeskCommand("nonexistentuser", "testdomain");

            var gitHubUser = new GitHubUserDto
            {
                Name = null,
                Email = "test@test.com",
                Company = "TestCompany",
                Bio = "Software Engineer"
            };

            _gitHubServiceMock.Setup(s => s.GetGitHubUserAsync(command.GitHubUsername))
                .ReturnsAsync(gitHubUser);

            // Act & Assert
            await Assert.ThrowsAsync<GitHubUserMissingPropertiesException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ShouldCreateCompany_WhenCompanyDoesNotExist()
        {
            // Arrange
            var command = new SyncGitHubToFreshdeskCommand("testuser", "testdomain");

            var gitHubUser = new GitHubUserDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Company = "NewCompany",
                Bio = "Software Developer"
            };

            var freshdeskNewCompany = new FreshdeskCompanyDto { Id = 999, Name = "NewCompany" };
            var freshdeskContact = new FreshdeskContactDto { Id = 456, Email = "test@example.com" };

            _gitHubServiceMock.Setup(s => s.GetGitHubUserAsync(command.GitHubUsername))
                .ReturnsAsync(gitHubUser);

            _freshdeskServiceMock.Setup(s => s.SearchCompanies(command.FreshdeskSubdomain, gitHubUser.Company))
                .ReturnsAsync(new FreshdeskCompanySearchResultDto { Companies = new FreshdeskCompanyDto[] { } });

            _freshdeskServiceMock.Setup(s => s.CreateCompany(command.FreshdeskSubdomain, It.IsAny<FreshdeskCompanyCreateDto>()))
                .ReturnsAsync(freshdeskNewCompany);

            _freshdeskServiceMock.Setup(s => s.CreateOrUpdateContactAsync(command.FreshdeskSubdomain, It.IsAny<FreshdeskContactInputDto>()))
                .ReturnsAsync(freshdeskContact);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _freshdeskServiceMock.Verify(s => s.CreateCompany(command.FreshdeskSubdomain, It.IsAny<FreshdeskCompanyCreateDto>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldSyncContactWithoutCompany_WhenCompanyIsNull()
        {
            // Arrange
            var command = new SyncGitHubToFreshdeskCommand("testuser", "testdomain");

            var gitHubUser = new GitHubUserDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Company = null,
                Bio = "Software Developer"
            };

            var freshdeskContact = new FreshdeskContactDto { Id = 456, Email = "test@example.com" };

            _gitHubServiceMock.Setup(s => s.GetGitHubUserAsync(command.GitHubUsername))
                .ReturnsAsync(gitHubUser);

            _freshdeskServiceMock.Setup(s => s.CreateOrUpdateContactAsync(command.FreshdeskSubdomain, It.Is<FreshdeskContactInputDto>(c => c.CompanyId == null)))
                .ReturnsAsync(freshdeskContact);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);
            _freshdeskServiceMock.Verify(s => s.CreateOrUpdateContactAsync(command.FreshdeskSubdomain, It.Is<FreshdeskContactInputDto>(c => c.CompanyId == null)), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnFalse_WhenContactCreationFails()
        {
            // Arrange
            var command = new SyncGitHubToFreshdeskCommand("testuser", "testdomain");

            var gitHubUser = new GitHubUserDto
            {
                Name = "Test User",
                Email = "test@example.com",
                Company = "TestCompany",
                Bio = "Software Developer"
            };

            var freshdeskCompany = new FreshdeskCompanyDto { Id = 123, Name = "TestCompany" };

            _gitHubServiceMock.Setup(s => s.GetGitHubUserAsync(command.GitHubUsername))
                .ReturnsAsync(gitHubUser);

            _freshdeskServiceMock.Setup(s => s.SearchCompanies(command.FreshdeskSubdomain, gitHubUser.Company))
                .ReturnsAsync(new FreshdeskCompanySearchResultDto { Companies = new[] { freshdeskCompany } });

            _freshdeskServiceMock.Setup(s => s.CreateOrUpdateContactAsync(command.FreshdeskSubdomain, It.IsAny<FreshdeskContactInputDto>()))
                .ReturnsAsync((FreshdeskContactDto?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
        }
    }
}
