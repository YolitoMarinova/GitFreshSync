namespace GitFreshSync.Application.Dtos.Freshdesk
{
    public class FreshdeskContactDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int CompanyId { get; set; }
        public string? Description { get; set; }
    }
}
