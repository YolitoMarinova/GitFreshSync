namespace GitFreshSync.Application.Exceptions.Freshdesk
{
    public class FailedCompaniesSearchException : Exception
    {
        public FailedCompaniesSearchException(string message)
            : base(message) { }
    }
}
