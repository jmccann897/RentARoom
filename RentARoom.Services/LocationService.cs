using Microsoft.AspNetCore.Identity;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Models;
using RentARoom.Models.ViewModels;

namespace RentARoom.Services.IServices
{
    public class LocationService : ILocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public LocationService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }
        public IEnumerable<Location> GetUserLocations(string userId)
        {
            if (string.IsNullOrEmpty(userId)) { throw new ArgumentNullException(nameof(userId), "User Id cannot be null or empty."); }
            var locations = _unitOfWork.Location.Find(l => l.ApplicationUserId == userId).ToList();
            return locations;
        }

        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            return await _unitOfWork.Location.GetAllAsync();
        }

        public Location GetLocationById(int id)
        {
            return _unitOfWork.Location.Get(l => l.Id == id);
        }

        public bool SaveNewLocation(LocationVM locationVM, string userId)
        {
            if (locationVM == null || string.IsNullOrEmpty(userId)) return false;
            var newLocation = new Location
            {
                LocationName = locationVM.LocationName,
                Address = locationVM.Address,
                City = locationVM.City,
                Postcode = locationVM.Postcode,
                Latitude = double.TryParse(locationVM.Latitude, out var lat) ? lat : 0,
                Longitude = double.TryParse(locationVM.Longitude, out var lng) ? lng : 0,
                ApplicationUserId = userId
            };

            _unitOfWork.Location.Add(newLocation);
            _unitOfWork.Save();
            return true;
        }

        public bool EditLocation(int id, Location updatedLocation)
        {
            if (updatedLocation == null || id != updatedLocation.Id) return false;

            var existingLocation = _unitOfWork.Location.Get(l => l.Id == id);
            if (existingLocation == null) return false;

            existingLocation.LocationName = updatedLocation.LocationName;
            existingLocation.Address = updatedLocation.Address;
            existingLocation.City = updatedLocation.City;
            existingLocation.Postcode = updatedLocation.Postcode;
            existingLocation.Latitude = updatedLocation.Latitude;
            existingLocation.Longitude = updatedLocation.Longitude;

            _unitOfWork.Location.Update(existingLocation);
            _unitOfWork.Save();
            return true;
        }

        public bool DeleteLocation(int id)
        {
            var location = _unitOfWork.Location.Get(l => l.Id == id);
            if (location == null) return false;

            _unitOfWork.Location.Remove(location);
            _unitOfWork.Save();
            return true;
        }
    }
}
