using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.DataAccess.Services.IServices;
using RentARoom.Models;
using RentARoom.Models.ViewModels;
using RentARoom.Services;
using RentARoom.Utility;
using Property = RentARoom.Models.Property;

namespace RentARoom.Areas.Agent.Controllers
{
    [Area("Agent")]
    [Authorize(Roles = SD.Role_Agent+","+SD.Role_Admin)]
    public class PropertiesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;
        private readonly IAzureBlobService _azureBlobService;

        public PropertiesController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager, IUserService userService, IAzureBlobService azureBlobService)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _userService = userService;
            _azureBlobService = azureBlobService;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch the logged-in user
            var applicationUser = await _userManager.GetUserAsync(User);

            if (applicationUser == null)
            {
                return Unauthorized(); // Handle the case where the user is not found
            }

            // Based on user role determine property list to use
            List<Property> objPropertyList;
            if(User.IsInRole(SD.Role_Agent))
            {
                objPropertyList = _unitOfWork.Property.Find(x => x.ApplicationUserId == User.Identity.Name).ToList();
            } else
            {
                objPropertyList = _unitOfWork.Property.GetAll(includeProperties: "PropertyType").ToList();
            }

            PropertyDatatableVM propertyDatatableVM = new PropertyDatatableVM
            {
                PropertyList = objPropertyList,
                ApplicationUser = applicationUser
            };

            return View(propertyDatatableVM);
        }

        // Get
        public async Task<IActionResult> Upsert(int? id)
        {
            PropertyVM propertyVM = new()
            {
                PropertyTypeList = _unitOfWork.PropertyType
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                ApplicationUserList = (await _userService.GetAdminAndAgentUsersAsync()).Select(user => new SelectListItem
                {
                    Text = user.UserName, // Display the username
                    Value = user.Id.ToString() // Use the userId as the value
                }).ToList(),
                Property = new Property()
            };

            // Create
            if (id == null || id == 0)
            {
                if (propertyVM.Property.Images == null)
                {
                    propertyVM.Property.Images = new List<Image>();
                }
                return View(propertyVM);
            }
            else
            {
                // Update
                propertyVM.Property = _unitOfWork.Property.Get(u => u.Id == id, includeProperties: "Images");
                propertyVM.ImageUrls = propertyVM.Property.Images?.Select(i => i.ImageUrl) ?? Enumerable.Empty<string>(); //check for null or empty
                return View(propertyVM);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(PropertyVM propertyVM, IEnumerable<IFormFile>? files)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var imageUrls = new List<string>(); // List to hold image URLs

                    // Save images to Azure blob storage
                    if (files != null && files.Any())
                    {
                        foreach (var file in files)
                        {
                            // Upload to Azure Blob Storage and get the URL
                            string imageUrl = await _azureBlobService.UploadFileAsync(file, 800, 600); // Example dimensions
                            imageUrls.Add(imageUrl);
                        }
                    }

                    // Check if update or create by whether id is 0 (create)
                    // Save property before adding images to it
                    if (propertyVM.Property.Id == 0)
                    {
                        _unitOfWork.Property.Add(propertyVM.Property);
                        _unitOfWork.Save();
                    }
                    else
                    {
                        _unitOfWork.Property.Update(propertyVM.Property);
                        _unitOfWork.Save();
                    }

                    // Create new images for the property
                    foreach (var imageUrl in imageUrls)
                    {
                        var image = new Image
                        {
                            ImageUrl = imageUrl,
                            PropertyId = propertyVM.Property.Id
                        };
                        _unitOfWork.Image.Add(image);
                    }

                    _unitOfWork.Save();
                    TempData["success"] = "Property created successfully";
                    return RedirectToAction("Index", "Properties");

                } catch (Exception ex)
                {
                    TempData["error"] = "An error occurred while saving the property. Please try again.";
                    Console.WriteLine(ex.Message); 
                    return View(propertyVM);
                }
            }
          
            // If issue, need to re-populate dropdown
            propertyVM.PropertyTypeList = _unitOfWork.PropertyType
            .GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            }).ToList();
            var users = await _userService.GetAdminAndAgentUsersAsync() ?? new List<ApplicationUser>();
            propertyVM.ApplicationUserList = users.Select(user => new SelectListItem
            {
                Text = user.UserName,
                Value = user.Id.ToString() 
            }).ToList();
            return View(propertyVM);
        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            // Check role first then filter results if Agent to their properties
            if (User.IsInRole(SD.Role_Agent))
            {
                List<Property> objPropertyList = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser,PropertyViews").Where(x => x.ApplicationUser.UserName == User.Identity.Name).ToList();
                return Json(new { data = objPropertyList });
            }
            // If Admin, show all
            else
            {
                List<Property> objPropertyList = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser,PropertyViews").ToList();
                return Json(new { data = objPropertyList });
            }          
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            var propertyToBeDeleted = _unitOfWork.Property.Get(u => u.Id == id, includeProperties: "Images");
            if (propertyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }

            // Iterate through the images and delete each file from Azure Blob Storage
            if (propertyToBeDeleted.Images != null && propertyToBeDeleted.Images.Any())
            {
                foreach (var image in propertyToBeDeleted.Images)
                {
                    try
                    {
                        // Extract blob name from ImageUrl (assuming it's the full Azure URL)
                        var blobName = Path.GetFileName(new Uri(image.ImageUrl).AbsolutePath);

                        // Delete blob using AzureBlobService
                        await _azureBlobService.DeleteFileAsync(blobName);

                        // Remove image entry from the database
                        _unitOfWork.Image.Remove(image);
                    }
                    catch (Exception ex)
                    {
                        // Log the error (optional)
                        Console.WriteLine($"Error deleting blob: {ex.Message}");
                    }
                }
            }

            // Remove the property itself
            _unitOfWork.Property.Remove(propertyToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successful" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(string imageUrl, int propertyId)
        {
            if (string.IsNullOrEmpty(imageUrl) || propertyId == 0)
            {
                TempData["error"] = "Invalid image URL.";
                return RedirectToAction("Upsert", new { id = propertyId }); // Adjust with the appropriate action
            }

            try
            {
                // Find the image in the database based on its URL
                var image = _unitOfWork.Image.Get(img => img.ImageUrl == imageUrl);
                if (image != null)
                {
                    // Delete the image from the database
                    _unitOfWork.Image.Remove(image);
                     _unitOfWork.Save();

                    // Get filename for deletion from blob
                    var fileName = Path.GetFileName(imageUrl);

                    // Delete the image from Azure Blob Storage
                    var blobClient = _azureBlobService.GetContainerClient().GetBlobClient(fileName);
                    await blobClient.DeleteIfExistsAsync();

                    TempData["success"] = "Image deleted successfully.";
                }
                else
                {
                    TempData["error"] = "Image not found.";
                }
            }
            catch (Exception ex)
            {
                TempData["error"] = "An error occurred while deleting the image.";
                Console.WriteLine(ex.Message);
            }

            return RedirectToAction("Upsert", new { id = propertyId });
        }

        #endregion

    }
}
