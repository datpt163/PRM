using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Capstone.Application.Common.Cohere;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace Suggest
{
    public class CohereServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly CohereService _cohereService;

        public CohereServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);

            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(config => config["Cohere:ApiKey"]).Returns("fake-api-key");

            _cohereService = new CohereService(_httpClient, _configurationMock.Object);
        }

        [Fact]
        public async Task GetResponseAsync_ShouldReturnExpectedText_WhenResponseIsSuccessful()
        {
            var expectedResponseText = "This is a response from Cohere.";
            var cohereResponse = new CohereResponse { Text = expectedResponseText };
            var responseContent = new StringContent(JsonSerializer.Serialize(cohereResponse), Encoding.UTF8, "application/json");

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            var result = await _cohereService.GetResponseAsync("Hello", "Test System Message", 100);

            Assert.Equal(expectedResponseText, result);
        }

        [Fact]
        public async Task GetResponseAsync_ShouldThrowHttpRequestException_WhenResponseIsUnsuccessful()
        {
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _cohereService.GetResponseAsync("Hello", "Test System Message", 100));
        }
    }
}
