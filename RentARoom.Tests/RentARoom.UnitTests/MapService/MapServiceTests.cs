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
    [Trait("Category", "Unit")]
    public class MapServiceTests
    {

        // Mock dependencies
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILocationService _locationService;
        private readonly IPropertyService _propertyService;
        private readonly MapService _mapService;

        public MapServiceTests()
        {
            // Setup mock dependencies
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _locationService = Substitute.For<ILocationService>();
            _propertyService = Substitute.For<IPropertyService>();

            // Create MapService instance with mocked dependencies
            _mapService = new MapService(_unitOfWork, _locationService, _propertyService);
        }

        [Fact]
        public async Task MapService_GetMapProperties_Should_ReturnListOfProperties()
        {
            // Arrange
            var expectedProperties = new List<Property> { new Property(), new Property() };
            _propertyService.GetAllPropertiesAsync().Returns(Task.FromResult((IEnumerable<Property>)expectedProperties));

            // Act
            var result = await _mapService.GetMapProperties();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProperties.Count, result.Count);
            Assert.Equal(expectedProperties, result);
            await _propertyService.Received(1).GetAllPropertiesAsync();
        }

        [Fact]
        public async Task MapService_GetMapLocations_Should_ReturnListOfLocations()
        {
            // Arrange
            var expectedLocations = new List<Location> { new Location(), new Location() };
            _locationService.GetAllLocationsAsync().Returns(Task.FromResult((IEnumerable<Location>)expectedLocations));

            // Act
            var result = await _mapService.GetMapLocations();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLocations.Count, result.Count);
            Assert.Equal(expectedLocations, result);
            await _locationService.Received(1).GetAllLocationsAsync();
        }

        [Fact]
        public void MapService_GetLocationById_Should_ReturnLocation()
        {
            // Arrange
            var expectedLocation = new Location { Id = 123 };
            _locationService.GetLocationById(123).Returns(expectedLocation);

            // Act
            var result = _mapService.GetLocationById(123);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedLocation, result);
            _locationService.Received(1).GetLocationById(123);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void MapService_GetLocationById_Should_ThrowArgumentException_WhenIdIsInvalid(int invalidId)
        {

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _mapService.GetLocationById(invalidId));
            _locationService.DidNotReceive().GetLocationById(invalidId); // Corrected line
        }

        [Fact]
        public void MapService_SaveNewLocation_Should_ReturnTrueIfSaved()
        {
            // Arrange
            var locationVM = new LocationVM(); // Create a mock LocationVM object
            var userId = "testUserId"; // Example user ID
            _locationService.SaveNewLocation(locationVM, userId).Returns(true);

            // Act
            var result = _mapService.SaveNewLocation(locationVM, userId);

            // Assert
            Assert.True(result);
            _locationService.Received(1).SaveNewLocation(locationVM, userId);
        }

        [Fact]
        public void MapService_SaveNewLocation_Should_ThrowArgumentNullException_WhenLocationVMIsNull()
        {
            // Arrange
            LocationVM locationVM = null;
            string userId = "testUserId";

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _mapService.SaveNewLocation(locationVM, userId));
            _locationService.DidNotReceive().SaveNewLocation(Arg.Any<LocationVM>(), Arg.Any<string>());
        }

        [Fact]
        public void MapService_SaveNewLocation_Should_ThrowArgumentNullException_WhenUserIdIsNullOrEmpty()
        {
            // Arrange
            LocationVM locationVM = new LocationVM();
            string nullUserId = null;
            string emptyUserId = "";

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _mapService.SaveNewLocation(locationVM, nullUserId));
            Assert.Throws<ArgumentException>(() => _mapService.SaveNewLocation(locationVM, emptyUserId));
            _locationService.DidNotReceive().SaveNewLocation(Arg.Any<LocationVM>(), Arg.Any<string>());
        }

        [Fact]
        public void MapService_EditLocation_Should_ReturnTrueIfEdited()
        {
            // Arrange
            var locationId = 1;
            var updatedLocation = new Location();
            _locationService.EditLocation(locationId, updatedLocation).Returns(true);

            // Act
            var result = _mapService.EditLocation(locationId, updatedLocation);

            // Assert
            Assert.True(result);
            _locationService.Received(1).EditLocation(locationId, updatedLocation);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void MapService_EditLocation_Should_ThrowArgumentException_WhenIdIsInvalid(int invalidId)
        {
            // Arrange
            Location updatedLocation = new Location();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => _mapService.EditLocation(invalidId, updatedLocation));
            _locationService.DidNotReceive().EditLocation(Arg.Any<int>(), Arg.Any<Location>());
        }

        [Fact]
        public void MapService_EditLocation_Should_ThrowArgumentNullException_WhenUpdatedLocationIsNull()
        {
            // Arrange
            int locationId = 1;
            Location updatedLocation = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _mapService.EditLocation(locationId, updatedLocation));
            _locationService.DidNotReceive().EditLocation(Arg.Any<int>(), Arg.Any<Location>());
        }

        [Fact]
        public void MapService_DeleteLocation_Should_ReturnTrueIfDeleted()
        {
            // Arrange
            var locationId = 1;
            _locationService.DeleteLocation(locationId).Returns(true);

            // Act
            var result = _mapService.DeleteLocation(locationId);

            // Assert
            Assert.True(result);
            _locationService.Received(1).DeleteLocation(locationId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void MapService_DeleteLocation_Should_ThrowArgumentException_WhenIdIsInvalid(int invalidId)
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _mapService.DeleteLocation(invalidId));
            _locationService.DidNotReceive().DeleteLocation(invalidId);
        }
    }
}