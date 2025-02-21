using GitFreshSync.Application.Configurations;
using GitFreshSync.Application.Dtos.GitHub;
using GitFreshSync.Application.Exceptions.Github;
using GitFreshSync.Application.Interfaces;
using GitFreshSync.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Xunit;

namespace GitFreshSync.Tests.Infrastructure
{
    public class GitHubServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly IGitHubService _gitHubService;

        public GitHubServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://api.github.com/")
            };

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "fake-token");
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GitFreshSync", "1.0"));

            var gitHubSettings = Options.Create(new GitHubSettings
            {
                ApiUrl = "https://api.github.com/users/",
                Token = "fake-token"
            });

            _gitHubService = new GitHubService(httpClient, gitHubSettings);
        }

        [Fact]
        public async Task GetGitHubUserAsync_ShouldReturnUser_WhenApiReturnsSuccess()
        {
            // Arrange
            var username = "testuser";
            var expectedUser = new GitHubUserDto { Login = username, Name = "Test User" };
            var responseContent = JsonSerializer.Serialize(expectedUser);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });

            // Act
            var result = await _gitHubService.GetGitHubUserAsync(username);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(username, result.Login);
            Assert.Equal("Test User", result.Name);
        }

        [Fact]
        public async Task GetGitHubUserAsync_ShouldThrowException_WhenApiReturnsNonSuccess()
        {
            // Arrange
            var username = "invaliduser";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound
                });

            // Act & Assert
            await Assert.ThrowsAsync<GetGitHubUserException>(() => _gitHubService.GetGitHubUserAsync(username));
        }
    }
}
