namespace GitFreshSync.Application.Exceptions.Freshdesk
{
    public class CreateFreshdeskContactFailedException : Exception
    {
        public CreateFreshdeskContactFailedException(string message)
            : base(message) { }
    }
}
