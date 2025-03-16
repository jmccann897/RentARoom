using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Services.IServices;
using RentARoom.Models;
using RentARoom.Models.ViewModels;
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
        private readonly IUserService _userService;
        private readonly IAzureBlobService _azureBlobService;
        private readonly IPropertyService _propertyService;

        public PropertiesController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager, IUserService userService, IAzureBlobService azureBlobService, IPropertyService propertyService)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _userService = userService;
            _azureBlobService = azureBlobService;
            _propertyService = propertyService;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch the logged-in user
            var applicationUser = await _userService.GetCurrentUserAsync(User);

            if (applicationUser == null) { return Unauthorized(); }

            var properties = await _propertyService.GetPropertiesForUserAsync(applicationUser);

            PropertyDatatableVM propertyDatatableVM = new PropertyDatatableVM
            {
                PropertyList = properties,
                ApplicationUser = applicationUser
            };

            return View(propertyDatatableVM);
        }

        // Get
        public async Task<IActionResult> Upsert(int? id)
        {
            PropertyVM propertyVM = new()
            {
                PropertyTypeList = _unitOfWork.PropertyType.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                ApplicationUserList = (await _userService.GetAdminAndAgentUsersAsync()).Select(user => new SelectListItem
                {
                    Text = user.UserName, 
                    Value = user.Id.ToString()
                }).ToList(),
                Property = new Property()
            };

            // Create
            if (id == null || id == 0)
            { 
                return View(propertyVM);
            }
            else
            {
                // Update
                var applicationUser = await _userService.GetCurrentUserAsync(User);
                propertyVM.Property = await _propertyService.GetPropertyAsync(id.Value);
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
                    var applicationUser = await _userService.GetCurrentUserAsync(User);
                    if (applicationUser == null) { return Unauthorized(); }

                    bool success = await _propertyService.SavePropertyAsync(propertyVM, applicationUser, files);
                    if (success)
                    {
                        TempData["success"] = propertyVM.Property.Id == 0 ? "Property created successfully" : "Property updated successfully";
                        return RedirectToAction("Index", "Properties");
                    }
                    else
                    {
                        TempData["error"] = "Error saving property";
                    }
                }
                catch (Exception ex)
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
        public async Task<IActionResult> GetAll()
        {
            var applicationUser = await _userService.GetCurrentUserAsync(User);
            if(applicationUser == null) { return Unauthorized(); }

            var properties = await _propertyService.GetPropertiesForUserAsync(applicationUser);

            return Json(new { data = properties });      
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return Json(new { success = false, message = "Invalid property Id." });
            }


            var result = await _propertyService.DeletePropertyAsync(id.Value);
            if (!result)
            {
                return Json(new { success = false, message = "Error while deleting." });
            }

            return Json(new { success = true, message = "Property deleted successfully." });     
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(string imageUrl, int propertyId)
        {
            if (string.IsNullOrEmpty(imageUrl) || propertyId == 0)
            {
                TempData["error"] = "Invalid image URL.";
                return RedirectToAction("Upsert", new { id = propertyId }); 
            }

            try
            {

                var result = await _propertyService.DeleteImageAsync(imageUrl, propertyId);
                if (result)
                {
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
