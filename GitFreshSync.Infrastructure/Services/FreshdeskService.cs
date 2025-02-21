using GitFreshSync.Application.Configurations;
using GitFreshSync.Application.Constants.ErrorMessages;
using GitFreshSync.Application.Dtos.Freshdesk;
using GitFreshSync.Application.Exceptions.Freshdesk;
using GitFreshSync.Application.Interfaces;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace GitFreshSync.Infrastructure.Services
{
    public class FreshdeskService : IFreshdeskService
    {
        private readonly HttpClient _httpClient;
        private readonly FreshdeskSettings _freshdeskSettings;

        public FreshdeskService(HttpClient httpClient, IOptions<FreshdeskSettings> settings)
        {
            _httpClient = httpClient;
            _freshdeskSettings = settings.Value;
            var authHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_freshdeskSettings.ApiKey}:X"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeaderValue);
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<FreshdeskCompanySearchResultDto> SearchCompanies(string subdomain, string companyName)
        {
            var url = ConstructUrl(subdomain, $"companies/autocomplete?name={companyName}");

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                throw new FailedCompaniesSearchException(string.Format(FreshdeskErrorMessages.SearchCompaniesFailed, response.StatusCode));
            }

            var responseData = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize<FreshdeskCompanySearchResultDto>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null)
            {
                throw new SearchCompanyDeserializationException(FreshdeskErrorMessages.DeserializationFailed);
            }

            return result;
        }

        public async Task<FreshdeskCompanyDto> CreateCompany(string subdomain, FreshdeskCompanyCreateDto dto)
        {
            var url = ConstructUrl(subdomain, "companies");
            var jsonContent = JsonSerializer.Serialize(dto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new FailedCreateCompanyException(string.Format(FreshdeskErrorMessages.FailedToCreateCompany, response.StatusCode));
            }

            var responseData = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<FreshdeskCompanyDto>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null)
            {
                throw new CompanyDeserializationException(FreshdeskErrorMessages.DeserializationFailed);
            }

            return result;
        }

        public async Task<FreshdeskContactDto?> CreateOrUpdateContactAsync(string subdomain, FreshdeskContactInputDto contact)
        {
            var searchResult = await SearchContactAsync(subdomain, contact.Email);

            if (searchResult?.Length > 0)
            {
                //update
                return await UpdateExistingContactAsync(subdomain, searchResult[0].Id, contact);
            }

            //create
            return await CreateContactAsync(subdomain, contact);
        }

        private async Task<FreshdeskContactDto?> CreateContactAsync(string subdomain, FreshdeskContactInputDto contact)
        {
            var url = ConstructUrl(subdomain, "contacts");
            var jsonContent = JsonSerializer.Serialize(contact);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new CreateFreshdeskContactFailedException(string.Format(FreshdeskErrorMessages.FailedToCreateContact, response.StatusCode));
            }

            var responseData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<FreshdeskContactDto?>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private async Task<FreshdeskContactDto?> UpdateExistingContactAsync(string subdomain, long id, FreshdeskContactInputDto contact)
        {
            var updateUrl = ConstructUrl(subdomain, $"contacts/{id}");
            var jsonContent = JsonSerializer.Serialize(contact);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(updateUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                throw new UpdateFreshdeskContactFailedException(string.Format(FreshdeskErrorMessages.FailedToUpdateContact, response.StatusCode));
            }

            var responseData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<FreshdeskContactDto?>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private async Task<FreshdeskContactDto[]?> SearchContactAsync(string subdomain, string email)
        {
            var url = ConstructUrl(subdomain, $"contacts?email={email}");
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                throw new SearchContactFailedException(string.Format(FreshdeskErrorMessages.SearchContanctsFailed, response.StatusCode));
            }

            var searchData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<FreshdeskContactDto[]?>(searchData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private string ConstructUrl(string subdomain, string endpoint)
        {
            var baseUrl = string.Format(_freshdeskSettings.ApiUrl, subdomain);
            return $"{baseUrl}{endpoint}";
        }
    }
}
