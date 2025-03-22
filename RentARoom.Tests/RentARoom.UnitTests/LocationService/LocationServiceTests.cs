using Microsoft.AspNetCore.Identity;
using NSubstitute;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Models;
using RentARoom.Models.ViewModels;
using RentARoom.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Tests.RentARoom.UnitTests
{
    public class LocationServiceTests
    {
        /*
         * 1. public IEnumerable<Location> GetUserLocations(string userId)
         * 2. public async Task<IEnumerable<Location>> GetAllLocationsAsync()
         * 3. public Location GetLocationById(int id)
         * 4. public bool SaveNewLocation(LocationVM locationVM, string userId)
         * 5. public bool EditLocation(int id, Location updatedLocation)
         * 6. public bool DeleteLocation(int id)
         */


        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly LocationService _locationService;

        public LocationServiceTests()
        {
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _userManager = Substitute.For<UserManager<ApplicationUser>>(Substitute.For<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _locationService = new LocationService(_unitOfWork, _userManager);
        }
        [Fact]
        public void LocationService_GetUserLocations_Should_ReturnLocations()
        {
            // Arrange
            string userId = "validUserId";
            var expectedLocations = new List<Location> { new Location { ApplicationUserId = userId } };
            _unitOfWork.Location.Find(Arg.Any<System.Linq.Expressions.Expression<Func<Location, bool>>>()).Returns(expectedLocations);

            // Act
            var result = _locationService.GetUserLocations(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLocations, result);
            _unitOfWork.Location.Received(1).Find(Arg.Any<System.Linq.Expressions.Expression<Func<Location, bool>>>());
        }

        [Fact]
        public void LocationService_GetUserLocations_Should_ThrowArgumentNullException_WhenUserIdNull()
        {
            // Arrange
            string userId = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _locationService.GetUserLocations(userId));
        }

        [Fact]
        public async Task LocationService_GetAllLocationsAsync_Should_ReturnAllLocations()
        {
            // Arrange
            var expectedLocations = new List<Location> { new Location { Id = 1 }, new Location { Id = 2 } };
            _unitOfWork.Location.GetAllAsync().Returns(Task.FromResult<IEnumerable<Location>>(expectedLocations));

            // Act
            var result = await _locationService.GetAllLocationsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLocations, result);
            await _unitOfWork.Location.Received(1).GetAllAsync();
        }

        [Fact]
        public void LocationService_GetLocationById_Should_ReturnLocation()
        {
            // Arrange
            int id = 1;
            var expectedLocation = new Location { Id = id };
            _unitOfWork.Location.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Location, bool>>>()).Returns(expectedLocation);

            // Act
            var result = _locationService.GetLocationById(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLocation, result);
            _unitOfWork.Location.Received(1).Get(Arg.Any<System.Linq.Expressions.Expression<Func<Location, bool>>>());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void LocationService_GetLocationById_InvalidId_ReturnsNull(int invalidId)
        {
            // Arrange
            _unitOfWork.Location.Get(l => l.Id == invalidId).Returns((Location)null);

            // Act
            var result = _locationService.GetLocationById(invalidId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void LocationService_SaveNewLocation_Should_ReturnTrue()
        {
            // Arrange
            var locationVM = new LocationVM { LocationName = "Test", Address = "Test Address", City = "Test City", Postcode = "Test Postcode", Latitude = "1.0", Longitude = "1.0" };
            string userId = "validUserId";

            // Act
            var result = _locationService.SaveNewLocation(locationVM, userId);

            // Assert
            Assert.True(result);
            _unitOfWork.Location.Received(1).Add(Arg.Any<Location>());
            _unitOfWork.Received(1).Save();
        }

        [Fact]
        public void LocationService_SaveNewLocation_Should_ReturnFalse_WhenLocationVMisNull()
        {
            // Arrange
            LocationVM locationVM = null;
            string userId = "validUserId";

            // Act
            var result = _locationService.SaveNewLocation(locationVM, userId);

            // Assert
            Assert.False(result);
            _unitOfWork.Location.DidNotReceive().Add(Arg.Any<Location>());
            _unitOfWork.DidNotReceive().Save();
        }
        [Fact]
        public void LocationService_SaveNewLocation_Should_ReturnFalse_WhenUserIdNull()
        {
            // Arrange
            var locationVM = new LocationVM { LocationName = "Test" };
            string userId = null;

            // Act
            var result = _locationService.SaveNewLocation(locationVM, userId);

            // Assert
            Assert.False(result);
            _unitOfWork.Location.DidNotReceive().Add(Arg.Any<Location>());
            _unitOfWork.DidNotReceive().Save();
        }

        [Fact]
        public void LocationService_EditLocation_Should_ReturnTrue()
        {
            // Arrange
            int id = 1;
            var updatedLocation = new Location { Id = id, LocationName = "Updated Test" };
            var existingLocation = new Location { Id = id, LocationName = "Original Test" };
            _unitOfWork.Location.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Location, bool>>>()).Returns(existingLocation);

            // Act
            var result = _locationService.EditLocation(id, updatedLocation);

            // Assert
            Assert.True(result);
            _unitOfWork.Location.Received(1).Update(Arg.Any<Location>());
            _unitOfWork.Received(1).Save();
        }

        [Fact]
        public void LocationService_EditLocation_Should_ReturnFalse_WhenUpdatedLocationIsNull()
        {
            // Arrange
            int id = 1;
            Location updatedLocation = null;

            // Act
            var result = _locationService.EditLocation(id, updatedLocation);

            // Assert
            Assert.False(result);
            _unitOfWork.Location.DidNotReceive().Update(Arg.Any<Location>());
            _unitOfWork.DidNotReceive().Save();
        }

        [Fact]
        public void LocationService_EditLocation_Should_ReturnFalse_WhenInvalidId()
        {
            // Arrange
            int id = 1;
            var updatedLocation = new Location { Id = 2 }; // Id mismatch

            // Act
            var result = _locationService.EditLocation(id, updatedLocation);

            // Assert
            Assert.False(result);
            _unitOfWork.Location.DidNotReceive().Update(Arg.Any<Location>());
            _unitOfWork.DidNotReceive().Save();
        }

        [Fact]
        public void LocationService_DeleteLocation_Should_ReturnTrue()
        {
            // Arrange
            int id = 1;
            var location = new Location { Id = id };
            _unitOfWork.Location.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Location, bool>>>()).Returns(location);

            // Act
            var result = _locationService.DeleteLocation(id);

            // Assert
            Assert.True(result);
            _unitOfWork.Location.Received(1).Remove(Arg.Any<Location>());
            _unitOfWork.Received(1).Save();
        }

        [Fact]
        public void LocationService_DeleteLocation_Should_ReturnFalse_WhenInvalidId()
        {
            // Arrange
            int id = 1;
            _unitOfWork.Location.Get(Arg.Any<System.Linq.Expressions.Expression<Func<Location, bool>>>()).Returns((Location)null);

            // Act
            var result = _locationService.DeleteLocation(id);

            // Assert
            Assert.False(result);
            _unitOfWork.Location.DidNotReceive().Remove(Arg.Any<Location>());
            _unitOfWork.DidNotReceive().Save();
        }

    }
}
