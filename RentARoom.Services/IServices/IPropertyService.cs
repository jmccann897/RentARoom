using Microsoft.AspNetCore.Http;
using RentARoom.Models;
using RentARoom.Models.ViewModels;

namespace RentARoom.Services.IServices
{
    public interface IPropertyService
    {
        /// <summary>
        /// Return a list of all properties
        /// </summary>
        /// <returns></returns>
        IEnumerable<Property> GetAllProperties();

        /// <summary>
        /// Return a list of properties filtered by search type and search phrase.
        /// </summary>
        /// <param name="searchType"></param>
        /// <param name="searchPhrase"></param>
        /// <returns></returns>
        Task<IEnumerable<Property>> SearchPropertiesAsync(string searchType, string searchPhrase);

        /// <summary>
        /// Get List of Properties for user (Agent list filtered to their ids, all for Admin).
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<List<Property>> GetPropertiesForUserAsync(ApplicationUser user);

        /// <summary>
        /// Get Property.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Property> GetPropertyAsync(int id);
        
        /// <summary>
        /// Save property which returns true or false.
        /// </summary>
        /// <param name="propertyVM"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<bool> SavePropertyAsync(PropertyVM propertyVM, ApplicationUser user, IEnumerable<IFormFile>? files);

        /// <summary>
        /// Delete a property.
        /// </summary>
        /// <param name="propertyId"></param>
        /// <returns></returns>
        Task<bool> DeletePropertyAsync(int propertyId);

        /// <summary>
        /// Delete image from blob and property.
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <param name="propertyId"></param>
        /// <returns></returns>
        Task<bool> DeleteImageAsync(string imageUrl, int propertyId);
    }
}
