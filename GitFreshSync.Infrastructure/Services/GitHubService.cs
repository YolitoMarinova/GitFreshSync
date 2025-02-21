using GitFreshSync.Application.Dtos.GitHub;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using GitFreshSync.Application.Configurations;
using GitFreshSync.Application.Interfaces;
using GitFreshSync.Application.Constants.ErrorMessages;
using GitFreshSync.Application.Exceptions.Github;


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
            throw new GetGitHubUserException(string.Format(GithubErrorMessages.GetGitHubUserFailed, response.StatusCode));
        }

        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GitHubUserDto?>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
}
