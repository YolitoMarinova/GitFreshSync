using GitFreshSync.Application.Constants.ErrorMessages;
using GitFreshSync.Application.Dtos.GitHub;
using GitFreshSync.Application.Exceptions.Github;
namespace GitFreshSync.Application.Validators
{
    public static class GitHubUserValidator
    {
        public static void Validate(GitHubUserDto? gitHubUser)
        {
            if (gitHubUser == null)
            {
                throw new GitHubUserNotFoundException(GithubErrorMessages.GitHubUserNotFouned);
            }

            if (string.IsNullOrEmpty(gitHubUser.Email))
            {
                throw new GitHubUserMissingPropertiesException(GithubErrorMessages.GitHubUserMissingEmail);
            }

            if (string.IsNullOrEmpty(gitHubUser.Name))
            {
                throw new GitHubUserMissingPropertiesException(GithubErrorMessages.GitHubUserMissingName);
            }
        }
    }
}
