using GitFreshSync.Application.Configurations;
using GitFreshSync.Application.Dtos.Freshdesk;
using GitFreshSync.Application.Exceptions.Freshdesk;
using GitFreshSync.Application.Interfaces;
using GitFreshSync.Infrastructure.Services;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Xunit;

namespace GitFreshSync.Tests.Infrastructure
{
    public class FreshdeskServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly IFreshdeskService _freshdeskService;

        public FreshdeskServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("https://fake.freshdesk.com/")
            };

            var apiKey = "fake-api-key";
            var authHeaderValue = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{apiKey}:X"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authHeaderValue);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var freshdeskSettings = Options.Create(new FreshdeskSettings
            {
                ApiUrl = "https://{0}.freshdesk.com/api/v2/",
                ApiKey = apiKey
            });

            _freshdeskService = new FreshdeskService(httpClient, freshdeskSettings);
        }

        [Fact]
        public async Task SearchCompanies_ShouldReturnResult_WhenApiReturnsSuccess()
        {
            // Arrange
            var subdomain = "testdomain";
            var companyName = "TestCompany";
            var expectedResult = new FreshdeskCompanySearchResultDto { Companies = new[] { new FreshdeskCompanyDto { Id = 123, Name = companyName } } };
            var responseContent = JsonSerializer.Serialize(expectedResult);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _freshdeskService.SearchCompanies(subdomain, companyName);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Companies);
            Assert.Equal(companyName, result.Companies[0].Name);
        }

        [Fact]
        public async Task SearchCompanies_ShouldThrowException_WhenApiReturnsError()
        {
            // Arrange
            var subdomain = "testdomain";
            var companyName = "InvalidCompany";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest });

            // Act & Assert
            await Assert.ThrowsAsync<FailedCompaniesSearchException>(() => _freshdeskService.SearchCompanies(subdomain, companyName));
        }

        [Fact]
        public async Task CreateCompany_ShouldReturnCompany_WhenApiReturnsSuccess()
        {
            // Arrange
            var subdomain = "testdomain";
            var companyDto = new FreshdeskCompanyCreateDto { Name = "New Company" };
            var expectedCompany = new FreshdeskCompanyDto { Id = 456, Name = "New Company" };
            var responseContent = JsonSerializer.Serialize(expectedCompany);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _freshdeskService.CreateCompany(subdomain, companyDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedCompany.Id, result.Id);
            Assert.Equal(expectedCompany.Name, result.Name);
        }

        [Fact]
        public async Task CreateCompany_ShouldThrowException_WhenApiReturnsError()
        {
            // Arrange
            var subdomain = "testdomain";
            var companyDto = new FreshdeskCompanyCreateDto { Name = "Invalid Company" };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.BadRequest });

            // Act & Assert
            await Assert.ThrowsAsync<FailedCreateCompanyException>(() => _freshdeskService.CreateCompany(subdomain, companyDto));
        }

        [Fact]
        public async Task CreateOrUpdateContactAsync_ShouldCreateContact_WhenNoExistingContactFound()
        {
            // Arrange
            var subdomain = "testdomain";
            var contactDto = new FreshdeskContactInputDto { Email = "test@example.com", Name = "Test User" };
            var expectedContact = new FreshdeskContactDto { Id = 789, Email = "test@example.com", Name = "Test User" };
            var responseContent = JsonSerializer.Serialize(expectedContact);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("contacts")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(new string[] { }), Encoding.UTF8, "application/json")
                });

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _freshdeskService.CreateOrUpdateContactAsync(subdomain, contactDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedContact.Email, result.Email);
        }

        [Fact]
        public async Task CreateOrUpdateContactAsync_ShouldUpdateContact_WhenExistingContactFound()
        {
            // Arrange
            var subdomain = "testdomain";
            var contactDto = new FreshdeskContactInputDto { Email = "test@example.com", Name = "Updated Name" };
            var existingContacts = new[] { new FreshdeskContactDto { Id = 999, Email = "test@example.com", Name = "Old Name" } };
            var updatedContact = new FreshdeskContactDto { Id = 999, Email = "test@example.com", Name = "Updated Name" };
            var responseContent = JsonSerializer.Serialize(updatedContact);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().Contains("contacts")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(existingContacts), Encoding.UTF8, "application/json")
                });

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
                });

            // Act
            var result = await _freshdeskService.CreateOrUpdateContactAsync(subdomain, contactDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(updatedContact.Name, result.Name);
        }
    }
}
