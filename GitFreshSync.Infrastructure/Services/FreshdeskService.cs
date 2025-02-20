using GitFreshSync.Application.Configurations;
using GitFreshSync.Application.Dtos.Freshdesk;
using GitFreshSync.Application.Interfaces;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

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
            var url = $"https://{subdomain}.freshdesk.com/api/v2/companies/autocomplete?name={companyName}";

            var response = await _httpClient.GetAsync(url);

            var responseData = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode || string.IsNullOrEmpty(responseData))
            {
                throw new Exception($"Failed to search companies. Status code: {response.StatusCode}");
            }

            return JsonSerializer.Deserialize<FreshdeskCompanySearchResultDto>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<FreshdeskCompanyDto> CreateCompany(string subdomain, FreshdeskCompanyCreateDto dto)
        {
            var url = $"https://{subdomain}.freshdesk.com/api/v2/companies";
            var jsonContent = JsonSerializer.Serialize(dto);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to create company. Status code: {response.StatusCode}");
            }

            var responseData = await response.Content.ReadAsStringAsync();
            var company = JsonSerializer.Deserialize<FreshdeskCompanyDto>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (company == null)
            {
                throw new Exception("Failed to create company.");
            }

            return company;
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
            var url = $"https://{subdomain}.freshdesk.com/api/v2/contacts";
            var jsonContent = JsonSerializer.Serialize(contact);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to create contact. Status code: {response.StatusCode}");
            }

            var responseData = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<FreshdeskContactDto>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private async Task<FreshdeskContactDto?> UpdateExistingContactAsync(string subdomain, long id, FreshdeskContactInputDto contact)
        {
            var contactId = id;
            var updateUrl = $"https://{subdomain}.freshdesk.com/api/v2/contacts/{contactId}";
            var jsonContent = JsonSerializer.Serialize(contact);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var updateResponse = await _httpClient.PutAsync(updateUrl, content);
            if (updateResponse.IsSuccessStatusCode)
            {
                var responseData = await updateResponse.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<FreshdeskContactDto>(responseData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            return null;
        }

        private async Task<FreshdeskContactDto[]?> SearchContactAsync(string subdomain, string email)
        {
            var searchUrl = $"https://{subdomain}.freshdesk.com/api/v2/contacts?email={email}";
            var searchResponse = await _httpClient.GetAsync(searchUrl);
            if (!searchResponse.IsSuccessStatusCode) return null;

            var searchData = await searchResponse.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<FreshdeskContactDto[]?>(searchData, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
