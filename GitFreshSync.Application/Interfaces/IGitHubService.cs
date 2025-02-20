using GitFreshSync.Application.Dtos.GitHub;

namespace GitFreshSync.Application.Interfaces
{
    public interface IGitHubService
    {
        Task<GitHubUserDto?> GetGitHubUserAsync(string username);
    }
}
