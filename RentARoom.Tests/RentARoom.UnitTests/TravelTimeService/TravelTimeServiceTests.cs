using Microsoft.Extensions.Configuration;
using NSubstitute;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Models;
using RentARoom.Services.IServices;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;


namespace RentARoom.Tests.RentARoom.UnitTests
{
    public class TravelTimeServiceTests
    {
        private readonly ILocationService _locationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly TravelTimeService _travelTimeService;
        private readonly IHttpClientWrapper _httpClientWrapper;

        public TravelTimeServiceTests()
        {
            _locationService = Substitute.For<ILocationService>();
            _unitOfWork = Substitute.For<IUnitOfWork>();
            Console.WriteLine("Starting TravelTimeServiceTests constructor");

            // 1. Build a test-specific configuration
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    // 2. Add the API key to the in-memory collection
                    { "OpenRouteServiceAPI:OSR-RentARoom", "test_api_key_value" }
                }!)
                .Build();
            Console.WriteLine("Configuration built");

            _httpClientWrapper = Substitute.For<IHttpClientWrapper>(); //and mock the wrapper
            // Setup the DefaultRequestHeaders mock
            _httpClientWrapper.DefaultRequestHeaders.Returns(new HttpClient().DefaultRequestHeaders);
            Console.WriteLine("HttpClientWrapper created");

            try
            {
                _travelTimeService = new TravelTimeService(_configuration, _unitOfWork, _locationService, _httpClientWrapper);
                Console.WriteLine("TravelTimeService created"); // Add this
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in TravelTimeServiceTests constructor: {ex.Message}");
                throw; // Re-throw the exception to fail the test
            }
            Console.WriteLine("Finishing TravelTimeServiceTests constructor");
        }

        // Need to invoke private method HandleApiResponse
        private (List<double> TravelTimes, List<double> Distances) InvokeHandleApiResponse(string responseContent)
        {
            try
            {
                return (ValueTuple<List<double>, List<double>>)_travelTimeService.GetType()
                    .GetMethod("HandleApiResponse", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Invoke(_travelTimeService, new object[] { responseContent });
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                if (ex.InnerException != null)
                {
                    // If not included, it throws invoke exception type which will mismatch the service exception type
                    throw ex.InnerException;
                }
                throw;
            }
        }

        [Fact]
        public async Task TravelTimeService_GetTravelTimeAsync_Should_ReturnDTO_WhenUserLocationAndPropertyExist()
        {
            // Arrange
            var userId = "testUser";
            var propertyId = 123;
            var profile = "driving-car";

            var userLocations = new List<Location>
            {
                new Location { Latitude = 52.5200, Longitude = 13.4050 },
                new Location { Latitude = 51.5074, Longitude = -0.1278 }
            };
            _locationService.GetUserLocations(userId).Returns(userLocations);

            var property = new Property { Id = propertyId, Latitude = 48.8566, Longitude = 2.3522 };
            _unitOfWork.Property.Get(Arg.Any<System.Linq.Expressions.Expression<System.Func<Property, bool>>>()).Returns(property);

            // Mock the API response
            var expectedTravelTimes = new List<double> { 0, 10, 20 };
            var expectedDistances = new List<double> { 0, 100, 200 };
            var mockApiResponse = new
            {
                durations = new[] { new[] { 0.0, 600.0, 1200.0 } }, // Durations in seconds
                distances = new[] { new[] { 0.0, 100.0, 200.0 } }
            };
            var mockResponseJson = JsonSerializer.Serialize(mockApiResponse);

            var mockHttpMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mockResponseJson, Encoding.UTF8, "application/json")
            };
            _httpClientWrapper.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>())
            .Returns(Task.FromResult(mockHttpMessage)); //mock the wrapper to return the response

            // Act
            var result = await _travelTimeService.GetTravelTimeAsync(userId, propertyId, profile);
            Console.WriteLine("result from mock service test",result.ToString());

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedTravelTimes, result.TravelTimes);
            Assert.Equal(expectedDistances, result.Distances);
        }

        [Fact]
        public async Task TravelTimeService_GetTravelTimeAsync_ShouldThrowException_WhenNoUserLocations()
        {
            // Arrange
            var userId = "testUser";
            var propertyId = 123;
            var profile = "driving-car";
            _locationService.GetUserLocations(userId).Returns(new List<Location>());

            // Act and Assert
            await Assert.ThrowsAsync<System.Exception>(() => _travelTimeService.GetTravelTimeAsync(userId, propertyId, profile));
        }

        [Fact]
        public async Task TravelTimeService_GetTravelTimeAsync_Should_ThrowException_WhenPropertyDoesNotExist()
        {
            // Arrange
            var userId = "testUser";
            var propertyId = 123;
            var profile = "driving-car";

            var userLocations = new List<Location>
            {
                new Location { Latitude = 52.5200, Longitude = 13.4050 },
                new Location { Latitude = 51.5074, Longitude = -0.1278 }
            };
            _locationService.GetUserLocations(userId).Returns(userLocations);
            _unitOfWork.Property.Get(Arg.Any<System.Linq.Expressions.Expression<System.Func<Property, bool>>>()).Returns((Property)null);

            // Act and Assert
            await Assert.ThrowsAsync<System.Exception>(() => _travelTimeService.GetTravelTimeAsync(userId, propertyId, profile));
        }

        [Fact]
        public void TravelTimeService_HandleApiResponse_Should_ListDoubleOfListDouble_WhenValidJson()
        {
            // Arrange
            var responseJson = "{\"durations\":[[0,600,1200]],\"distances\":[[0,100,200]]}";

            // Act
            var (travelTimes, distances) = InvokeHandleApiResponse(responseJson);

            // Assert
            Assert.Equal(new List<double> { 0, 10, 20 }, travelTimes);
            Assert.Equal(new List<double> { 0, 100, 200 }, distances);
        }


        [Fact]
        public void TravelTimeService_HandleApiResponse_Should_ReturnEmptyDistances_WhenNoDistancesFound()
        {
            // Arrange
            var responseJson = "{\"durations\":[[0,600,1200]]}";

            // Act
            var (travelTimes, distances) = InvokeHandleApiResponse(responseJson);

            // Assert
            Assert.Equal(new List<double> { 0, 10, 20 }, travelTimes);
            Assert.Empty(distances);
            
        }

        [Fact]
        public void TravelTimeService_HandleApiResponse_Should_ReturnEmptyDurations_WhenNoDurationsFound()
        {
            // Arrange
            var responseJson = "{\"distances\":[[0,100,200]]}";

            // Act
            var (travelTimes, distances) = InvokeHandleApiResponse(responseJson);

            // Assert
            Assert.Empty(travelTimes);
            Assert.Equal(new List<double> { 0, 100, 200 }, distances);
        }

        [Fact]
        public void TravelTimeService_HandleApiResponse_Should_ThrowException_WhenInvalidJson()
        {
            // Arrange
            var responseJson = "invalid json";

            // Act & Assert
            Assert.Throws<Exception>(() => InvokeHandleApiResponse(responseJson));
        }

        [Fact]
        public void TravelTimeService_HandleApiResponse_Should_ThrowException_WhenInvalidDurationDataType()
        {
            // Arrange
            var responseJson = "{\"durations\":[[0,\"abc\",1200]],\"distances\":[[0,100,200]]}";

            // Act & Assert
            Assert.Throws<Exception>(() => InvokeHandleApiResponse(responseJson));
        }

        [Fact]
        public void TravelTimeService_HandleApiResponse_Should_ThrowException_WhenInvalidDistanceDataType()
        {
            // Arrange
            var responseJson = "{\"durations\":[[0,600,1200]],\"distances\":[[0,\"abc\",200]]}";

            // Act & Assert
            Assert.Throws<Exception>(() => InvokeHandleApiResponse(responseJson));
        }

        [Fact]
        public async Task TravelTimeService_GetTravelTimesAndDistancesAsync_Should_ReturnListDoubleOfListDouble_WhenValidInput()
        {
            // Arrange
            var userCoordinates = new List<Coordinate> { new Coordinate { Lat = 1.0, Lon = 2.0 } };
            var propertyLocation = new Coordinate { Lat = 3.0, Lon = 4.0 };
            var profile = "driving-car";

            var mockApiResponse = new
            {
                durations = new[] { new[] { 0.0, 600.0 } },
                distances = new[] { new[] { 0.0, 100.0 } }
            };
            var mockResponseJson = JsonSerializer.Serialize(mockApiResponse);

            var mockHttpMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(mockResponseJson, Encoding.UTF8, "application/json")
            };
            _httpClientWrapper.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>())
                .Returns(Task.FromResult(mockHttpMessage));

            // Act
            var (travelTimes, distances) = await _travelTimeService.GetTravelTimesAndDistancesAsync(userCoordinates, propertyLocation, profile);

            // Assert
            Assert.NotNull(travelTimes);
            Assert.NotNull(distances);
            Assert.Equal(new List<double> { 0, 10 }, travelTimes); // 600 seconds / 60 = 10 minutes
            Assert.Equal(new List<double> { 0, 100 }, distances);
        }

        [Fact]
        public async Task TravelTimeService_GetTravelTimesAndDistancesAsync_Should_ThrowException_WhenApiErrorReturned()
        {
            // Arrange
            var userCoordinates = new List<Coordinate> { new Coordinate { Lat = 1.0, Lon = 2.0 } };
            var propertyLocation = new Coordinate { Lat = 3.0, Lon = 4.0 };
            var profile = "driving-car";

            var mockHttpMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest, // Or any non-200 status code
                Content = new StringContent("Error response", Encoding.UTF8)
            };
            _httpClientWrapper.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>())
                .Returns(Task.FromResult(mockHttpMessage));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                _travelTimeService.GetTravelTimesAndDistancesAsync(userCoordinates, propertyLocation, profile));

        }

        [Fact]
        public async Task TravelTimeService_GetTravelTimesAndDistancesAsync_Should_ThrowException_WhenInvalidInput()
        {
            // Arrange
            var userCoordinates = new List<Coordinate>();
            var propertyLocation = new Coordinate { Lat = 3.0, Lon = 4.0 };
            var profile = "driving-car";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _travelTimeService.GetTravelTimesAndDistancesAsync(userCoordinates, propertyLocation, profile));
        }



    }
}
