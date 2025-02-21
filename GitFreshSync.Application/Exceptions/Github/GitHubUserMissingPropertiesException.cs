namespace GitFreshSync.Application.Exceptions.Github
{
    public class GitHubUserMissingPropertiesException : Exception
    {
        public GitHubUserMissingPropertiesException(string message)
            : base(message) { }
    }
}
