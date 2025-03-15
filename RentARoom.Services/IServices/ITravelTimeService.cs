using RentARoom.Models;
using RentARoom.Models.DTOs;

namespace RentARoom.Services.IServices
{
    public interface ITravelTimeService
    {
        Task<(List<double> TravelTimes, List<double> Distances)> GetTravelTimesAndDistancesAsync(List<Coordinate> userCoordinates, Coordinate propertyLocation, string profile);

        Task<TravelTimeResultDTO> GetTravelTimeAsync(string userId, int propertyId, string profile);
    }
}
