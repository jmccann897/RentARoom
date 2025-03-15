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

        public MapService(IUnitOfWork unitOfWork, ILocationService locationService, IPropertyService propertyService)
        {
            _unitOfWork = unitOfWork;
            _locationService = locationService;
            _propertyService = propertyService;
        }


        public async Task<List<Property>> GetMapProperties()
        {
            return (await _propertyService.GetAllPropertiesAsync()).ToList();
        }

        public async Task<List<Location>> GetMapLocations()
        {
            return (await _locationService.GetAllLocationsAsync()).ToList();
        }

        public Location GetLocationById(int id)
        {
            return _locationService.GetLocationById(id);
        }

        public bool SaveNewLocation(LocationVM locationVM, string userId)
        {
            return _locationService.SaveNewLocation(locationVM, userId); 
        }

        public bool EditLocation(int id, Location updatedLocation)
        {
            return _locationService.EditLocation(id, updatedLocation);
        }

        public bool DeleteLocation(int id)
        {
            return _locationService.DeleteLocation(id);
        }
    }
}