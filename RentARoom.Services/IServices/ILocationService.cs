using RentARoom.Models;

namespace RentARoom.Services.IServices
{
    public interface ILocationService
    {
        /// <summary>
        /// Return users locations
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IEnumerable<Location> GetUserLocations(string userId);
    }

}
