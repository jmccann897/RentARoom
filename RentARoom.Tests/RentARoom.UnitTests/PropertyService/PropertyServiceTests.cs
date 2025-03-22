using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Models;
using RentARoom.Models.ViewModels;
using RentARoom.Services.IServices;
using RentARoom.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Tests.RentARoom.UnitTests
{
    public class PropertyServiceTests
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAzureBlobService _azureBlobService;
        private readonly PropertyService _propertyService;

        public PropertyServiceTests()
        {
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _userManager = Substitute.For<UserManager<ApplicationUser>>(Substitute.For<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
            _azureBlobService = Substitute.For<IAzureBlobService>();
            _propertyService = new PropertyService(_unitOfWork, _userManager, _azureBlobService);
        }

        // 1. GetAllProperties
        [Fact]
        public void GetAllProperties_ReturnsAllProperties()
        {
            // Arrange
            var properties = new List<Property> { new Property { Id = 1 }, new Property { Id = 2 } };
            _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser,Images").Returns(properties);

            // Act
            var result = _propertyService.GetAllProperties();

            // Assert
            Assert.Equal(properties, result);
            _unitOfWork.Property.Received(1).GetAll(includeProperties: "PropertyType,ApplicationUser,Images");
        }

        // 2. GetAllPropertiesAsync
        [Fact]
        public async Task GetAllPropertiesAsync_ReturnsAllPropertiesAsync()
        {
            // Arrange
            var properties = new List<Property> { new Property { Id = 1 }, new Property { Id = 2 } };
            _unitOfWork.Property.GetAllAsync(includeProperties: "PropertyType,ApplicationUser,Images").Returns(Task.FromResult((IEnumerable<Property>)properties));

            // Act
            var result = await _propertyService.GetAllPropertiesAsync();

            // Assert
            Assert.Equal(properties, result);
            await _unitOfWork.Property.Received(1).GetAllAsync(includeProperties: "PropertyType,ApplicationUser,Images");
        }

        // 3. SearchPropertiesAsync
        [Fact]
        public async Task SearchPropertiesAsync_ReturnsFilteredProperties()
        {
            // Arrange
            string searchType = "Bedroom";
            string searchPhrase = "London";
            var properties = new List<Property>
            {
                new Property { Address = "123 London Street", City = "London", Postcode = "E1 6AN", ApplicationUser = new ApplicationUser { Name = "John Doe" }, PropertyType = new PropertyType { Name = "Bedroom" } },
                new Property { Address = "456 Manchester Road", City = "Manchester", Postcode = "M1 2AB", ApplicationUser = new ApplicationUser { Name = "Jane Smith" }, PropertyType = new PropertyType { Name = "House" } }
            };

            _unitOfWork.Property.GetAll("PropertyType,ApplicationUser,Images").Returns(properties);
            _unitOfWork.Property.Find(Arg.Any<Expression<Func<Property, bool>>>()).Returns(properties.Where(p => p.City.Contains(searchPhrase)));

            // Act
            var result = await _propertyService.SearchPropertiesAsync(searchType, searchPhrase);

            // Assert
            Assert.Single(result);
            Assert.All(result, p => Assert.Equal("Bedroom", p.PropertyType.Name));
        }

        // 4. GetPropertiesForUserAsync
        [Fact]
        public async Task GetPropertiesForUserAsync_ReturnsPropertiesForAgent()
        {
            // Arrange
            var user = new ApplicationUser { Id = "testUserId" };
            var role = new List<string> { SD.Role_Agent };
            var agentProperties = new List<Property>
            {
                new Property { Id = 1, ApplicationUserId = "testUserId" },
                new Property { Id = 2, ApplicationUserId = "testUserId" }
            };
                    var allProperties = new List<Property>
            {
                new Property { Id = 1, ApplicationUserId = "testUserId" },
                new Property { Id = 2, ApplicationUserId = "testUserId" },
                new Property { Id = 3, ApplicationUserId = "otherUserId" }
            };

            _userManager.GetRolesAsync(user).Returns(Task.FromResult((IList<string>)role));
            _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser,PropertyViews").Returns(allProperties);

            // Act
            var result = await _propertyService.GetPropertiesForUserAsync(user);

            // Assert
            Assert.Equal(agentProperties.Count, result.Count);
            Assert.Contains(result, p => p.Id == 1);
            Assert.Contains(result, p => p.Id == 2);
            Assert.DoesNotContain(result, p => p.Id == 3);
        }

        [Fact]
        public async Task GetPropertiesForUserAsync_ReturnsAllPropertiesForNonAgent()
        {
            // Arrange
            var user = new ApplicationUser { Id = "testUserId" };
            var role = new List<string> { "NonAgentRole" }; // Simulate a non-agent role
            var allProperties = new List<Property>
            {
                new Property { Id = 1, ApplicationUserId = "testUserId" },
                new Property { Id = 2, ApplicationUserId = "otherUserId" }
            };

            _userManager.GetRolesAsync(user).Returns(Task.FromResult((IList<string>)role)); // Corrected line
            _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser,PropertyViews").Returns(allProperties);

            // Act
            var result = await _propertyService.GetPropertiesForUserAsync(user);

            // Assert
            Assert.Equal(allProperties.Count, result.Count);
            Assert.Contains(result, p => p.Id == 1);
            Assert.Contains(result, p => p.Id == 2);
        }

        [Fact]
        public async Task GetPropertiesForUserAsync_ReturnsEmptyListForNullUser()
        {
            // Arrange
            ApplicationUser user = null;

            // Act
            var result = await _propertyService.GetPropertiesForUserAsync(user);

            // Assert
            Assert.Empty(result);
        }

        // 5. GetPropertyAsync
        [Fact]
        public async Task GetPropertyAsync_ReturnsPropertyById()
        {
            // Arrange
            int propertyId = 123;
            var expectedProperty = new Property { Id = propertyId, Address = "Test Address" };

            _unitOfWork.Property.GetAsync(Arg.Any<Expression<Func<Property, bool>>>(), Arg.Any<string>())
                .Returns(Task.FromResult(expectedProperty));

            // Act
            var result = await _propertyService.GetPropertyAsync(propertyId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedProperty.Id, result.Id);
            Assert.Equal(expectedProperty.Address, result.Address);
            await _unitOfWork.Property.Received(1).GetAsync(Arg.Any<Expression<Func<Property, bool>>>(), Arg.Any<string>());
        }


        // 6. SavePropertyAsync
        [Fact]
        public async Task SavePropertyAsync_CreatesNewProperty()
        {
            // Arrange
            var propertyVM = new PropertyVM
            {
                Property = new Property
                {
                    Id = 0, // Indicate a new property
                    Address = "123 Test St",
                    Postcode = "Test Postcode",
                    Price = 100000,
                    NumberOfBedrooms = 3,
                    NumberOfBathrooms = 2,
                    NumberOfEnsuites = 1,
                    FloorArea = 150,
                    City = "Test City",
                    Latitude = 1.0,
                    Longitude = 2.0,
                    PropertyTypeId = 1
                }
            };

            var user = new ApplicationUser { Id = "testUserId" };
            var files = new List<IFormFile>
            {
                Substitute.For<IFormFile>(),
                Substitute.For<IFormFile>()
            };

            _azureBlobService.UploadFileAsync(Arg.Any<IFormFile>(), 800, 600)
                .Returns("http://testimage.com/image1.jpg", "http://testimage.com/image2.jpg");

            // Act
            var result = await _propertyService.SavePropertyAsync(propertyVM, user, files);

            // Assert
            Assert.True(result);
            await _azureBlobService.Received(2).UploadFileAsync(Arg.Any<IFormFile>(), 800, 600);
            _unitOfWork.Property.Received(1).Add(Arg.Is<Property>(p => p.Address == "123 Test St"));
            _unitOfWork.Image.Received(2).Add(Arg.Any<Image>());
            _unitOfWork.Received(2).Save();
        }

        [Fact]
        public async Task SavePropertyAsync_UpdatesExistingProperty()
        {

            // Arrange
            int propertyId = 123;
            var propertyVM = new PropertyVM
            {
                Property = new Property
                {
                    Id = propertyId,
                    Address = "Updated Address",
                    Postcode = "Updated Postcode",
                    Price = 200000,
                    NumberOfBedrooms = 4,
                    NumberOfBathrooms = 3,
                    NumberOfEnsuites = 2,
                    FloorArea = 200,
                    City = "Updated City",
                    Latitude = 3.0,
                    Longitude = 4.0,
                    PropertyTypeId = 2
                }
            };

            var user = new ApplicationUser { Id = "testUserId" };
            var files = new List<IFormFile>
            {
                Substitute.For<IFormFile>(),
                Substitute.For<IFormFile>()
            };

            var existingProperty = new Property { Id = propertyId, Address = "Original Address" };

            _azureBlobService.UploadFileAsync(Arg.Any<IFormFile>(), 800, 600)
                .Returns(Task.FromResult("http://testimage.com/updated1.jpg"), Task.FromResult("http://testimage.com/updated2.jpg"));

            _unitOfWork.Property.GetAsync(Arg.Any<Expression<Func<Property, bool>>>(), Arg.Any<string>())
                .Returns(Task.FromResult(existingProperty));

            // Act
            var result = await _propertyService.SavePropertyAsync(propertyVM, user, files);

            // Assert
            Assert.True(result);
            await _azureBlobService.Received(2).UploadFileAsync(Arg.Any<IFormFile>(), 800, 600);
            _unitOfWork.Property.Received(1).Update(Arg.Is<Property>(p => p.Address == "Updated Address"));
            _unitOfWork.Image.Received(2).Add(Arg.Any<Image>());
            _unitOfWork.Received(1).Save();
        }

        [Fact]
        public async Task SavePropertyAsync_ReturnsFalseIfPropertyNotFoundForUpdate()
        {
            // Arrange
            int propertyId = 123;
            var propertyVM = new PropertyVM
            {
                Property = new Property
                {
                    Id = propertyId,
                    Address = "Updated Address",
                    Postcode = "Updated Postcode",
                    Price = 200000,
                    NumberOfBedrooms = 4,
                    NumberOfBathrooms = 3,
                    NumberOfEnsuites = 2,
                    FloorArea = 200,
                    City = "Updated City",
                    Latitude = 3.0,
                    Longitude = 4.0,
                    PropertyTypeId = 2
                }
            };

            var user = new ApplicationUser { Id = "testUserId" };
            var files = new List<IFormFile>
            {
                Substitute.For<IFormFile>(),
                Substitute.For<IFormFile>()
            };

            _azureBlobService.UploadFileAsync(Arg.Any<IFormFile>(), 800, 600)
                .Returns(Task.FromResult("http://testimage.com/updated1.jpg"), Task.FromResult("http://testimage.com/updated2.jpg")); // Explicit Task.FromResult

            _unitOfWork.Property.GetAsync(Arg.Any<Expression<Func<Property, bool>>>(), Arg.Any<string>())
                .Returns(Task.FromResult((Property)null));

            // Act
            var result = await _propertyService.SavePropertyAsync(propertyVM, user, files);

            // Assert
            Assert.False(result);
            await _azureBlobService.DidNotReceive().UploadFileAsync(Arg.Any<IFormFile>(), 800, 600);
            _unitOfWork.Property.DidNotReceive().Update(Arg.Any<Property>());
            _unitOfWork.Image.DidNotReceive().Add(Arg.Any<Image>());
            _unitOfWork.DidNotReceive().Save();
        }

        // 7. DeletePropertyAsync
        [Fact]
        public async Task DeletePropertyAsync_DeletesPropertyAndImages()
        {
            // Arrange
            int propertyId = 123;
            var images = new List<Image>
            {
                new Image { Id = 1, ImageUrl = "http://testblob.com/image1.jpg", PropertyId = propertyId },
                new Image { Id = 2, ImageUrl = "http://testblob.com/image2.jpg", PropertyId = propertyId }
            };
            var propertyToBeDeleted = new Property { Id = propertyId, Images = images };

            _unitOfWork.Property.GetAsync(Arg.Any<Expression<Func<Property, bool>>>(), Arg.Any<string>())
                .Returns(Task.FromResult(propertyToBeDeleted));
            _azureBlobService.DeleteFileAsync(Arg.Any<string>()).Returns(Task.CompletedTask);

            // Act
            var result = await _propertyService.DeletePropertyAsync(propertyId);

            // Assert
            Assert.True(result);
            await _azureBlobService.Received(2).DeleteFileAsync(Arg.Any<string>());
            _unitOfWork.Image.Received(2).Remove(Arg.Any<Image>());
            _unitOfWork.Property.Received(1).Remove(Arg.Is<Property>(p => p.Id == propertyId));
            _unitOfWork.Received(1).Save();
        }

        [Fact]
        public async Task DeletePropertyAsync_ReturnsFalseIfPropertyNotFound()
        {
            //Arrange
            int propertyId = 123;
            _unitOfWork.Property.GetAsync(Arg.Any<Expression<Func<Property, bool>>>(), Arg.Any<string>())
                .Returns(Task.FromResult((Property)null));

            //Act
            var result = await _propertyService.DeletePropertyAsync(propertyId);

            //Assert
            Assert.False(result);
            await _azureBlobService.DidNotReceive().DeleteFileAsync(Arg.Any<string>());
            _unitOfWork.Image.DidNotReceive().Remove(Arg.Any<Image>());
            _unitOfWork.Property.DidNotReceive().Remove(Arg.Any<Property>());
            _unitOfWork.DidNotReceive().Save();
        }

        // 8. DeleteImageAsync
        [Fact]
        public async Task DeleteImageAsync_DeletesImage()
        {
            // Arrange
            string imageUrl = "https://storage.blob.core.windows.net/container/sample.jpg";
            int propertyId = 1;
            var image = new Image { ImageUrl = imageUrl, PropertyId = propertyId };

            _unitOfWork.Image.Get(Arg.Any<Expression<Func<Image, bool>>>()).Returns(image);

            var blobClient = Substitute.For<BlobClient>();
            _azureBlobService.GetContainerClient().GetBlobClient(Arg.Any<string>()).Returns(blobClient);

            // Act
            var result = await _propertyService.DeleteImageAsync(imageUrl, propertyId);

            // Assert
            Assert.True(result);
            _unitOfWork.Image.Received(1).Remove(image);
            _unitOfWork.Received(1).Save();
            await blobClient.Received(1).DeleteIfExistsAsync();
        }

        [Fact]
        public async Task DeleteImageAsync_ReturnsFalseIfImageNotFound()
        {
            // Arrange
            string imageUrl = "http://testblob.com/nonexistent_image.jpg";
            int propertyId = 123;

            // Simulate Image.Get returning null (image not found)
            _unitOfWork.Image.Get(Arg.Any<Expression<Func<Image, bool>>>()).Returns((Image)null);

            // Act
            var result = await _propertyService.DeleteImageAsync(imageUrl, propertyId);

            // Assert
            Assert.False(result);

            // Verify that no further actions were taken
            _unitOfWork.Image.DidNotReceive().Remove(Arg.Any<Image>());
            _unitOfWork.DidNotReceive().Save();
            _azureBlobService.GetContainerClient().DidNotReceive().GetBlobClient(Arg.Any<string>());
        }

        [Fact]
        public async Task DeleteImageAsync_ReturnsFalseIfInvalidInput()
        {
            // Arrange
            string nullImageUrl = null;
            string emptyImageUrl = "";
            int zeroPropertyId = 0;
            int validPropertyId = 123;
            string validImageUrl = "http://testblob.com/image.jpg";

            // Act
            var result1 = await _propertyService.DeleteImageAsync(nullImageUrl, validPropertyId);
            var result2 = await _propertyService.DeleteImageAsync(emptyImageUrl, validPropertyId);
            var result3 = await _propertyService.DeleteImageAsync(validImageUrl, zeroPropertyId);

            // Assert
            Assert.False(result1);
            Assert.False(result2);
            Assert.False(result3);

            // Verify that no further actions were taken
            _unitOfWork.Image.DidNotReceive().Get(Arg.Any<Expression<Func<Image, bool>>>());
            _unitOfWork.Image.DidNotReceive().Remove(Arg.Any<Image>());
            _unitOfWork.DidNotReceive().Save();
            _azureBlobService.GetContainerClient().DidNotReceive().GetBlobClient(Arg.Any<string>());
        }
    }

}
