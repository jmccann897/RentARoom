using System.Text;
using NSubstitute;
using RentARoom.Services.IServices;

namespace RentARoom.Tests.RentARoom.UnitTests
{
    public class HttpClientWrapperTests
    {
        [Fact]
        public async Task HttpClientWrapper_PostAsync_CallsHttpClientPostAsync()
        {
            // Arrange: Create a mock HttpClient, instantiate HttpClientWrapper, and prepare request parameters.
            var mockHttpClient = Substitute.For<HttpClient>();
            var httpClientWrapper = new HttpClientWrapper(mockHttpClient);
            var requestUri = "testUri";
            var content = new StringContent("testContent", Encoding.UTF8, "application/json");

            // Act: Call PostAsync on the HttpClientWrapper.
            await httpClientWrapper.PostAsync(requestUri, content);

            // Assert: Verify that the underlying HttpClient's SendAsync was called with the correct parameters.
            await mockHttpClient.Received(1).SendAsync(
                Arg.Is<HttpRequestMessage>(msg =>
                    msg.Method == HttpMethod.Post &&
                    msg.RequestUri.ToString() == requestUri &&
                    msg.Content == content),
                Arg.Any<CancellationToken>()
            );
        }

        [Fact]
        public async Task HttpClientWrapper_GetAsync_CallsHttpClientGetAsync()
        {
            // Arrange: Create a mock HttpClient, instantiate HttpClientWrapper, and prepare request parameters.
            var mockHttpClient = Substitute.For<HttpClient>();
            var httpClientWrapper = new HttpClientWrapper(mockHttpClient);
            var requestUri = "https://example.com/testUri";


            // Configure the mock HttpClient to return a dummy HttpResponseMessage.
            mockHttpClient.SendAsync(Arg.Is<HttpRequestMessage>(msg => msg.Method == HttpMethod.Get && msg.RequestUri.ToString() == requestUri), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(new HttpResponseMessage()));

            // Act: Call GetAsync on the HttpClientWrapper.
            await httpClientWrapper.GetAsync(requestUri);

            // Assert: Verify that the underlying HttpClient's GetAsync was called with the correct parameters.
            await mockHttpClient.Received(1).GetAsync(requestUri, Arg.Any<CancellationToken>());
        }
    }
}
