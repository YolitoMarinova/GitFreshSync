
namespace GitFreshSync.Application.Exceptions.Github
{
    public class GetGitHubUserException : Exception
    {
        public GetGitHubUserException(string message) 
            : base(message) {}
    }
}
