namespace GitFreshSync.Application.Dtos.Sync
{
    public class SyncRequestDto
    {
        public string GitHubUsername { get; set; } = string.Empty;
        public string FreshdeskSubdomain { get; set; } = string.Empty;
    }
}
