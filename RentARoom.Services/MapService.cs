using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Models;
using RentARoom.Models.DTOs;
using RentARoom.Models.ViewModels;
using RentARoom.Services.IServices;
using SixLabors.ImageSharp;
using System.Text.Json;

namespace RentARoom.Services.IServices
{
    public class MapService : IMapService
    {
        private readonly ILocationService _locationService;
        private readonly IPropertyService _propertyService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MapService(IUnitOfWork unitOfWork, ILocationService locationService, 
            IPropertyService propertyService, IUserService userService, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork = unitOfWork;
            _locationService = locationService;
            _propertyService = propertyService;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<List<Property>> GetMapProperties()
        {
            return (await _propertyService.GetAllPropertiesAsync()).ToList();
        }

        public async Task<List<Location>> GetMapLocations()
        {
            // Get userid
            var claimsUser = _httpContextAccessor.HttpContext.User;
            var user = await _userService.GetCurrentUserAsync(claimsUser);

            //
            if(user == null)
            {
                return new List<Location>();
            }
            if(await _userService.IsUserAdmin(user.Id))
            {
                return (await _locationService.GetAllLocationsAsync()).ToList();
            }
            else 
            {
                return _locationService.GetUserLocations(user.Id).ToList();
            }
        }

        public Location GetLocationById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id must be a positive integer.", nameof(id));
            }
            return _locationService.GetLocationById(id);
        }

        public bool SaveNewLocation(LocationVM locationVM, string userId)
        {
            if (locationVM == null)
            {
                throw new ArgumentNullException(nameof(locationVM));
            }

            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be empty.", nameof(userId));
            }
            return _locationService.SaveNewLocation(locationVM, userId); 
        }

        public bool EditLocation(int id, Location updatedLocation)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id must be a positive integer.", nameof(id));
            }
            if (updatedLocation == null)
            {
                throw new ArgumentNullException(nameof(updatedLocation));
            }
            return _locationService.EditLocation(id, updatedLocation);
        }

        public bool DeleteLocation(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Id must be a positive integer.", nameof(id));
            }
            return _locationService.DeleteLocation(id);
        }
    }
}