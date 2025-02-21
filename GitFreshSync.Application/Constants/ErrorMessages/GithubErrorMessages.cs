namespace GitFreshSync.Application.Constants.ErrorMessages
{
    public static class GithubErrorMessages
    {
        public const string GetGitHubUserFailed = "Failed to get GitHub user. Status Code: {0}.";
        public const string GitHubUserNotFouned = "GitHub user not found.";
        public const string GitHubUserMissingEmail = "GitHub user does not have an email address.";
        public const string GitHubUserMissingName = "GitHub user does not have a name.";
    }
}
