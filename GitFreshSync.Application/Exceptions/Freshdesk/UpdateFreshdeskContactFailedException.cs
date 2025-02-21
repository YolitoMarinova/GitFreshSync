namespace GitFreshSync.Application.Exceptions.Freshdesk
{
    public class UpdateFreshdeskContactFailedException : Exception
    {
        public UpdateFreshdeskContactFailedException(string message)
            : base(message) { }
    }
}
