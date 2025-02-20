using MediatR;

namespace GitFreshSync.Application.Sync.Commands.SyncGitHubToFreshdeskCommand
{
    public record SyncGitHubToFreshdeskCommand(string GitHubUsername, string FreshdeskSubdomain) : IRequest<bool>;
}
