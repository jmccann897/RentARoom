using Azure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Models;
using RentARoom.Models.DTOs;
using RentARoom.Services.IServices;
using SixLabors.ImageSharp;
using System;
using System.Net.Http.Headers;
using System.Text.Json;

namespace RentARoom.Services.IServices
{
    public class TravelTimeService : ITravelTimeService
    {
        private readonly string _apiKey;
        private readonly IHttpClientWrapper _httpClientWrapper;
        private readonly ILocationService _locationService;
        private readonly IUnitOfWork _unitOfWork;

        public TravelTimeService(IConfiguration configuration, IUnitOfWork unitOfWork, ILocationService locationService, 
            IHttpClientWrapper httpClientWrapper)
        {
            _apiKey = configuration["OpenRouteServiceAPI:OSRRentARoom"];
            _unitOfWork = unitOfWork;
            _locationService = locationService;
            _httpClientWrapper = httpClientWrapper;
            _httpClientWrapper.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);


        }

        public async Task<TravelTimeResultDTO> GetTravelTimeAsync(string userId, int propertyId, string profile)
        {
            var userLocations = _locationService.GetUserLocations(userId);
            if (!userLocations.Any())
            {
                throw new Exception("No saved locations for the user.");
            }

            var property = _unitOfWork.Property.Get(u => u.Id == propertyId);
            if(property == null)
            {
                throw new Exception("Invalid property.");
            }

            var userCoordinates = userLocations.Select(l => new Coordinate { Lat = l.Latitude, Lon = l.Longitude }).ToList();
            var propertyLocation = new Coordinate { Lat = property.Latitude, Lon = property.Longitude };

            var (travelTimes, distances) = await GetTravelTimesAndDistancesAsync(userCoordinates, propertyLocation, profile);

            return new TravelTimeResultDTO
            {
                TravelTimes = travelTimes,
                Distances = distances
            };
        }

        public async Task<(List<double> TravelTimes, List<double> Distances)> GetTravelTimesAndDistancesAsync(List<Coordinate> userCoordinates, Coordinate propertyLocation, string profile)
        {
            if (userCoordinates == null || !userCoordinates.Any())
            {
                throw new ArgumentException("User coordinates cannot be null or empty.", nameof(userCoordinates));
            }

            if (propertyLocation == null)
            {
                throw new ArgumentNullException(nameof(propertyLocation), "Property location cannot be null.");
            }

            if (string.IsNullOrEmpty(profile))
            {
                throw new ArgumentException("Profile cannot be null or empty.", nameof(profile));
            }
            // Prepare the request body
            var requestBody = new
            {
                locations = new[] {
                    new[] { propertyLocation.Lon, propertyLocation.Lat }
                    }.Concat(userCoordinates.Select(l => new[] { l.Lon, l.Lat })), 
                metrics = new[] { "duration", "distance" },
                units = "m" 
            };

            var requestUri = $"https://api.openrouteservice.org/v2/matrix/{profile}";

            try
            {
                // Serialize requestBody to JSON
                var content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

                Console.WriteLine(content);

                // Send the request to Open Route Service
                var response = await _httpClientWrapper.PostAsync(requestUri, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                Console.WriteLine("API Response: " + responseContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response: {errorResponse}");
                    throw new Exception($"Error fetching travel times: {response.StatusCode} - {errorResponse}");
                }

                // Handle the API response data
                return HandleApiResponse(responseContent);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in GetTravelTimesAndDistancesAsync: {ex.Message}");
                Console.WriteLine($"API Key is null: {_apiKey == null}");
                throw;
            }
        }

        private (List<double> TravelTimes, List<double> Distances) HandleApiResponse(string responseContent)
        {
            // Declare matrixData outside the try block
            JsonElement matrixData; 

            try
            {
                // Deserialize the response into JsonElement
                matrixData = JsonSerializer.Deserialize<JsonElement>(responseContent);
            }
            catch (JsonException ex)
            {
                throw new Exception("Invalid JSON response", ex);
            }

            // Extract durations from the response
            List<double> travelTimes = new List<double>();
            if (matrixData.TryGetProperty("durations", out var durationsElement) && durationsElement.ValueKind == JsonValueKind.Array)
            {
                try
                {
                    var durations = durationsElement[0].EnumerateArray().Select(d => d.GetDouble()).ToList();
                    // Convert the durations from seconds to minutes (rounding)
                    travelTimes = durations.Select(d => Math.Round(d / 60.0, 0)).ToList();
                }
                catch (InvalidOperationException ex)
                {
                    throw new Exception("Invalid duration data type", ex);
                }
            }
            else
            {
                Console.WriteLine("No durations found in API response.");
            }

            // Extract distances from the response
            List<double> distances = new List<double>();
            if (matrixData.TryGetProperty("distances", out var distancesElement) && distancesElement.ValueKind == JsonValueKind.Array)
            {
                try
                {
                    var distancesArray = distancesElement[0].EnumerateArray().Select(d => d.GetDouble()).ToList();
                    // Format distance to 2 dp
                    distances = distancesArray.Select(d => Math.Round(d, 2)).ToList();
                }
                catch (InvalidOperationException ex)
                {
                    throw new Exception("Invalid distance data type", ex);
                }
            }
            else
            {
                Console.WriteLine("No distances found in API response.");
            }

            // Return both travel times and distances
            return (travelTimes, distances);
        }
    }
}