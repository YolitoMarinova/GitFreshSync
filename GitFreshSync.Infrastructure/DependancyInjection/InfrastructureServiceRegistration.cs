using GitFreshSync.Application.Interfaces;
using GitFreshSync.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GitFreshSync.Infrastructure.DependancyInjection
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddHttpClient<IGitHubService, GitHubService>();
            services.AddHttpClient<IFreshdeskService, FreshdeskService>();

            return services;
        }
    }
}
