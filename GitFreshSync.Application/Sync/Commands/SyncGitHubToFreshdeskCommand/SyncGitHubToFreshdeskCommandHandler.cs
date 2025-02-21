using GitFreshSync.Application.Dtos.Freshdesk;
using GitFreshSync.Application.Interfaces;
using GitFreshSync.Application.Validators;
using MediatR;

namespace GitFreshSync.Application.Sync.Commands.SyncGitHubToFreshdeskCommand
{
    public class SyncGitHubToFreshdeskCommandHandler : IRequestHandler<SyncGitHubToFreshdeskCommand, bool>
    {
        private readonly IGitHubService _gitHubService;
        private readonly IFreshdeskService _freshdeskService;

        public SyncGitHubToFreshdeskCommandHandler(IGitHubService gitHubService, IFreshdeskService freshdeskService)
        {
            _gitHubService = gitHubService;
            _freshdeskService = freshdeskService;
        }

        public async Task<bool> Handle(SyncGitHubToFreshdeskCommand request, CancellationToken cancellationToken)
        {
            // Fetch user from GitHub
            var gitHubUser = await _gitHubService.GetGitHubUserAsync(request.GitHubUsername);

            GitHubUserValidator.Validate(gitHubUser);

            var contact = new FreshdeskContactInputDto
            {
                Name = gitHubUser!.Name,
                Email = gitHubUser.Email,
                CompanyId = await GetCompanyId(request.FreshdeskSubdomain, gitHubUser.Company),
                Description = gitHubUser.Bio
            };

            // Sync with Freshdesk
            var result = await _freshdeskService.CreateOrUpdateContactAsync(request.FreshdeskSubdomain, contact);
            return result == null ? false : true;
        }

        private async Task<long?> GetCompanyId(string subdomain, string companyName)
        {
            if(string.IsNullOrEmpty(companyName))
            {
                return null;
            }

            var searchResult = await _freshdeskService.SearchCompanies(subdomain, companyName);

            if (searchResult.Companies == null || !searchResult.Companies.Any(c => c.Name == companyName))
            {
                var companyDto = new FreshdeskCompanyCreateDto
                {
                    Name = companyName
                };
                var newCompany = await _freshdeskService.CreateCompany(subdomain, companyDto);
                searchResult.Companies = new FreshdeskCompanyDto[] { newCompany };
            }

            return searchResult.Companies.First(c => c.Name == companyName).Id;
        }
    }
}
