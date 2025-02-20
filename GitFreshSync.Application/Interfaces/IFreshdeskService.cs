using GitFreshSync.Application.Dtos.Freshdesk;

namespace GitFreshSync.Application.Interfaces
{
    public interface IFreshdeskService
    {
        Task<FreshdeskCompanySearchResultDto> SearchCompanies(string subdomain, string companyName);
        Task<FreshdeskCompanyDto> CreateCompany(string subdomain, FreshdeskCompanyCreateDto company);
        Task<FreshdeskContactDto?> CreateOrUpdateContactAsync(string subdomain, FreshdeskContactInputDto contact);
    }
}
