namespace GitFreshSync.Application.Exceptions.Github
{
    public class GitHubUserNotFoundException : Exception
    {
        public GitHubUserNotFoundException(string message)
            : base(message) { }
    }
}
