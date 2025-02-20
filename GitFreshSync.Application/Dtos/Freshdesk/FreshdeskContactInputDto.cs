using System.Text.Json.Serialization;

namespace GitFreshSync.Application.Dtos.Freshdesk
{
    public class FreshdeskContactInputDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("company_id")]
        public long? CompanyId { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}
