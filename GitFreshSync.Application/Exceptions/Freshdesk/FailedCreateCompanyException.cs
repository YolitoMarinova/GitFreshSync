namespace GitFreshSync.Application.Exceptions.Freshdesk
{
    public class FailedCreateCompanyException : Exception
    {
        public FailedCreateCompanyException(string message)
            : base(message) { }
    }
}
