namespace GitFreshSync.Application.Exceptions.Freshdesk
{
    public class SearchContactFailedException : Exception
    {
        public SearchContactFailedException(string message)
            : base(message) { }
    }
}
