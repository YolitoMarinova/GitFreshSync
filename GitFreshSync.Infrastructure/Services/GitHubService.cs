using GitFreshSync.Application.Dtos.GitHub;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using GitFreshSync.Application.Configurations;
using GitFreshSync.Application.Interfaces;


namespace GitFreshSync.Infrastructure.Services;

public class GitHubService : IGitHubService
{
    private readonly HttpClient _httpClient;
    private readonly GitHubSettings _gitHubSettings;

    public GitHubService(HttpClient httpClient, IOptions<GitHubSettings> settings)
    {
        _httpClient = httpClient;
        _gitHubSettings = settings.Value;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _gitHubSettings.Token);
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("GitFreshSync", "1.0"));
    }

    public async Task<GitHubUserDto?> GetGitHubUserAsync(string username)
    {
        
        var response = await _httpClient.GetAsync($"{_gitHubSettings.ApiUrl}{username}");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"GitHub API returned {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GitHubUserDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
