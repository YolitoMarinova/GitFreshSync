using System.Text.Json.Serialization;

namespace GitFreshSync.Application.Dtos.Freshdesk
{
    public class FreshdeskCompanyCreateDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }
}
