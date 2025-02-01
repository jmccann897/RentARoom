using RentARoom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.DataAccess.Services.IServices
{
    public interface ITravelTimeService
    {
        Task<(List<double> TravelTimes, List<double> Distances)> GetTravelTimesAndDistancesAsync(List<Coordinate> userCoordinates, Coordinate propertyLocation, string profile);

    }
}
