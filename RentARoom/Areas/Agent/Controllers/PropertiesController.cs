using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.DataAccess.Services.IServices;
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
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;

        public PropertiesController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _userService = userService;
        }

        public IActionResult Index()
        {
            if (User.IsInRole(SD.Role_Agent))
            {
                // Update as owner changed to user object and name should be accessible
                List<Property> objPropertyList = _unitOfWork.Property.Find(x => x.ApplicationUserId == User.Identity.Name).ToList();
                return View(objPropertyList);
            }
            else
            {
                List<Property> objPropertyList = _unitOfWork.Property.GetAll(includeProperties: "PropertyType").ToList();
                return View(objPropertyList);
            }
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
                    string wwwRootPath = _webHostEnvironment.WebRootPath;// Gets to wwwRoot folder
                    var imageUrls = new List<string>(); // List to hold image URLs

                    // Save images to file system
                    if (files != null && files.Any())
                    {
                        foreach (var file in files)
                        {
                            // Generate random name for file + extension from upload
                            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                            //Generate location to save file to
                            string propertyPath = Path.Combine(wwwRootPath, @"images/property");

                            // Ensure directory exists
                            Directory.CreateDirectory(propertyPath);

                            // Save each image to the file system
                            using (var fileStream = new FileStream(Path.Combine(propertyPath, fileName), FileMode.Create))
                            {
                                file.CopyTo(fileStream);
                            }

                            // Add the image URL to the list
                            imageUrls.Add(@"\images\property\" + fileName);
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
                List<Property> objPropertyList = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser").Where(x => x.ApplicationUser.UserName == User.Identity.Name).ToList();
                return Json(new { data = objPropertyList });
            }
            // If Admin, show all
            else
            {
                List<Property> objPropertyList = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser").ToList();
                return Json(new { data = objPropertyList });
            }          
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var propertyToBeDeleted = _unitOfWork.Property.Get(u => u.Id == id, includeProperties: "Images");
            if (propertyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }

            // Iterate through the images and delete each file
            if(propertyToBeDeleted.Images != null && propertyToBeDeleted.Images.Any())
            {
                foreach (var image in propertyToBeDeleted.Images)
                {
                    var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, image.ImageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                        _unitOfWork.Image.Remove(image); // manual removal from Db - likely need redirected to r2
                    }
                }
            }

            _unitOfWork.Property.Remove(propertyToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successful" });
        }

        #endregion

    }
}
