using GitFreshSync.Application.Interfaces;
using GitFreshSync.Infrastructure.DependancyInjection;
using GitFreshSync.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GitFreshSync.Tests.Infrastructure
{
    public class DependencyInjectionTests
    {
        [Fact]
        public void AddInfrastructureServices_ShouldRegisterServicesCorrectly()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddInfrastructureServices();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var gitHubService = serviceProvider.GetService<IGitHubService>();
            var freshdeskService = serviceProvider.GetService<IFreshdeskService>();

            Assert.NotNull(gitHubService);
            Assert.NotNull(freshdeskService);
            Assert.IsType<GitHubService>(gitHubService);
            Assert.IsType<FreshdeskService>(freshdeskService);
        }
    }
}
