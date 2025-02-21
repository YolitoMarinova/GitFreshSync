namespace GitFreshSync.Application.Exceptions.Freshdesk
{
    public class CompanyDeserializationException : Exception
    {
        public CompanyDeserializationException(string message)
            : base(message) { }
    }
}
