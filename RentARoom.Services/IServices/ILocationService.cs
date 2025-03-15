using RentARoom.Models;
using RentARoom.Models.ViewModels;

namespace RentARoom.Services.IServices
{
    public interface ILocationService
    {
        /// <summary>
        /// Return users locations.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        IEnumerable<Location> GetUserLocations(string userId);

        /// <summary>
        /// Gets all locations.
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<Location>> GetAllLocationsAsync();

        /// <summary>
        /// Get location by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Location GetLocationById(int id);

        /// <summary>
        /// Save new location.
        /// </summary>
        /// <param name="locationVM"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool SaveNewLocation(LocationVM locationVM, string userId);

        /// <summary>
        /// Edit location.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updatedLocation"></param>
        /// <returns></returns>
        bool EditLocation(int id, Location updatedLocation);

        /// <summary>
        /// Delete a location.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool DeleteLocation(int id);
    }

}
