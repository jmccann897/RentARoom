using RentARoom.Models;
using RentARoom.Models.DTOs;
using RentARoom.Models.ViewModels;

namespace RentARoom.Services.IServices
{
    public interface IMapService
    {
        /// <summary>
        /// Get the properties for the map.
        /// </summary>
        /// <returns></returns>
        Task<List<Property>> GetMapProperties();

        /// <summary>
        /// Get the locations for the map.
        /// </summary>
        /// <returns></returns>
        Task<List<Location>> GetMapLocations();

        /// <summary>
        /// Calls Get location by id in location service.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Location GetLocationById(int id);

        /// <summary>
        /// Calls Save New Location in location service
        /// </summary>
        /// <param name="locationVM"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool SaveNewLocation(LocationVM locationVM, string userId);

        /// <summary>
        /// Calls Edit Location in location service
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updatedLocation"></param>
        /// <returns></returns>
        bool EditLocation(int id, Location updatedLocation);

        /// <summary>
        /// Calls Delete Location in location service
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        bool DeleteLocation(int id);
    }
}
