using Microsoft.AspNetCore.Identity;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Models;

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
    }
}
