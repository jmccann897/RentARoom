using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.DataAccess.Services.IServices;
using RentARoom.Models;
using RentARoom.Models.DTOs;
using RentARoom.Models.ViewModels;
using RentARoom.Utility;

namespace RentARoom.Services.IServices
{
    public class PropertyService : IPropertyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAzureBlobService _azureBlobService;

        public PropertyService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IAzureBlobService azureBlobService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _azureBlobService = azureBlobService;
        }

        public IEnumerable<Property> GetAllProperties()
        {
            var properties = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser,Images");
            return properties;
        }
        public async Task<IEnumerable<Property>> GetAllPropertiesAsync()
        {
            return await _unitOfWork.Property.GetAllAsync(includeProperties: "PropertyType,ApplicationUser,Images");
        }

        public async Task<IEnumerable<Property>> SearchPropertiesAsync(string searchType, string searchPhrase)
        {
            var properties = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser,Images");
            if (!string.IsNullOrEmpty(searchPhrase))
            {
                properties = _unitOfWork.Property.Find(p =>
                p.Address.Contains(searchPhrase) ||
                p.ApplicationUser.Name.Contains(searchPhrase) ||
                p.Postcode.Contains(searchPhrase) ||
                p.City.Contains(searchPhrase));
            }

            if (!string.IsNullOrEmpty(searchType))
            {
                if (searchType.Equals("Bedroom"))
                {
                    properties = properties.Where(p => p.PropertyType.Name.Equals("Bedroom"));
                }
                else if (searchType.Equals("House"))
                {
                    properties = properties.Where(p => !p.PropertyType.Name.Equals("Bedroom"));
                }
            }

            return await Task.FromResult(properties);
        }
        public async Task<List<Property>> GetPropertiesForUserAsync(ApplicationUser user)
        {
            // null check
            if (user == null)
            {
                return new List<Property>();
            }

            // need to fetch users role to check what to return
            var role = await _userManager.GetRolesAsync(user);

            if(role.Contains(SD.Role_Agent))
            {
                var agentProperties = _unitOfWork.Property
                    .GetAll(includeProperties: "PropertyType,ApplicationUser,PropertyViews")
                    .Where(x => x.ApplicationUserId == user.Id)
                    .ToList();
                return agentProperties;
            }
            var allProperties = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser,PropertyViews").ToList();
            return allProperties;
        }

        public async Task<Property> GetPropertyAsync(int id)
        {
            return await _unitOfWork.Property.GetAsync(u => u.Id == id, includeProperties: "PropertyType,ApplicationUser,Images,PropertyViews");
        }

        public async Task<bool> SavePropertyAsync(PropertyVM propertyVM, ApplicationUser user, IEnumerable<IFormFile>? files)
        {
            // Map view model to DTO
            PropertyDTO propertyDTO = MapPropertyVMToDTO(propertyVM);
            Property property;

            // Save any images sent
            var imageUrls = new List<string>();
            if (files != null & files.Any())
            {
                foreach (var file in files)
                {
                    string imageUrl = await _azureBlobService.UploadFileAsync(file, 800, 600);
                    imageUrls.Add(imageUrl);
                }
            }

            if (propertyDTO.Id == 0)
            {
                property = new Property
                {
                    Address = propertyDTO.Address,
                    Postcode = propertyDTO.Postcode,
                    Price = propertyDTO.Price,
                    NumberOfBedrooms = propertyDTO.NumberOfBedrooms,
                    NumberOfBathrooms = propertyDTO.NumberOfBathrooms,
                    NumberOfEnsuites = propertyDTO.NumberOfEnsuites,
                    FloorArea = propertyDTO.FloorArea,
                    City = propertyDTO.City,
                    Latitude = propertyDTO.Latitude,
                    Longitude = propertyDTO.Longitude,
                    ApplicationUserId = user.Id,
                    PropertyTypeId = propertyDTO.PropertyTypeId,
                    CreateDate = DateTime.UtcNow,
                    PropertyViews = new List<PropertyView>(),
                };

                _unitOfWork.Property.Add(property);
            }
            else
            {
                property = await _unitOfWork.Property.GetAsync(p => p.Id == propertyDTO.Id);
                if (property == null) return false;

                property.Address = propertyDTO.Address;
                property.Postcode = propertyDTO.Postcode;
                property.Price = propertyDTO.Price;
                property.NumberOfBedrooms = propertyDTO.NumberOfBedrooms;
                property.NumberOfBathrooms = propertyDTO.NumberOfBathrooms;
                property.NumberOfEnsuites = propertyDTO.NumberOfEnsuites;
                property.FloorArea = propertyDTO.FloorArea;
                property.City = propertyDTO.City;
                property.Longitude = propertyDTO.Longitude;
                property.Latitude = propertyDTO.Latitude;
                property.PropertyTypeId = propertyDTO.PropertyTypeId;

                _unitOfWork.Property.Update(property);

            }

            // Save property
            _unitOfWork.Save();

            // Process file images

            foreach (var imageUrl in imageUrls)
            {
                var image = new Image
                {
                    ImageUrl = imageUrl,
                    PropertyId = property.Id
                };
                _unitOfWork.Image.Add(image);
            }

            // Save images
            _unitOfWork.Save();

            return true;
     
        }

        public async Task<bool> DeletePropertyAsync(int propertyId)
        {
            // 1. Fetch property including images
            var propertyToBeDeleted = await _unitOfWork.Property.GetAsync(u => u.Id == propertyId, includeProperties: "Images");
            if (propertyToBeDeleted == null) { return false; }

            // 2. Loop through images ande delete each from Azure blob storage and DB
            if(propertyToBeDeleted.Images != null && propertyToBeDeleted.Images.Any())
            {
                foreach (var image in propertyToBeDeleted.Images)
                {
                    try
                    {
                        var blobName = Path.GetFileName(new Uri(image.ImageUrl).AbsolutePath);
                        await _azureBlobService.DeleteFileAsync(blobName);
                        _unitOfWork.Image.Remove(image);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting blob: {ex.Message}");
                    }
                }
            }

            // 3. Remove property from DB
            _unitOfWork.Property.Remove(propertyToBeDeleted);
            _unitOfWork.Save();

            return true;
        }

        public async Task<bool> DeleteImageAsync(string imageUrl, int propertyId)
        {
            if (string.IsNullOrEmpty(imageUrl) || propertyId == 0)
            {
                return false;
            }
               

            var image = _unitOfWork.Image.Get(img => img.ImageUrl == imageUrl);
            if (image == null)
            {
                return false;
            }

            _unitOfWork.Image.Remove(image);
            _unitOfWork.Save();

            var fileName = Path.GetFileName(imageUrl);
            var blobClient = _azureBlobService.GetContainerClient().GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();

            return true;
        }

        private PropertyDTO MapPropertyVMToDTO(PropertyVM vm)
        {
            return new PropertyDTO
            {
                Id = vm.Property.Id,
                Address = vm.Property.Address,
                Postcode = vm.Property.Postcode,
                Price = vm.Property.Price,
                NumberOfBedrooms = vm.Property.NumberOfBedrooms,
                NumberOfBathrooms = vm.Property.NumberOfBathrooms,
                NumberOfEnsuites = vm.Property.NumberOfEnsuites,
                FloorArea = vm.Property.FloorArea,
                City = vm.Property.City,
                Latitude = vm.Property.Latitude,
                Longitude = vm.Property.Longitude,
                PropertyTypeId = vm.Property.PropertyTypeId,
                ApplicationUserId = vm.Property.ApplicationUserId,

                // Include PropertyViews count
                ViewCount = vm.Property.PropertyViews?.Count ?? 0
            };
        }

       
    }
}
