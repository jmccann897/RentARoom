using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RentARoom.DataAccess.Data;
using RentARoom.DataAccess.Repository.IRepository;
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

        public PropertiesController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            if (User.IsInRole(SD.Role_Agent))
            {
                //Update as owner changed to user object and name should be accessible
                List<Property> objPropertyList = _unitOfWork.Property.Find(x => x.ApplicationUserId == User.Identity.Name).ToList();
                return View(objPropertyList);
            }
            else
            {
                List<Property> objPropertyList = _unitOfWork.Property.GetAll(includeProperties: "PropertyType").ToList();
                return View(objPropertyList);
            }
        }

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
                ApplicationUserList = (await GetAdminAndAgentUsersAsync()).Select(user => new SelectListItem
                {
                    Text = user.UserName, // Display the username
                    Value = user.Id.ToString() // Use the userId as the value
                }).ToList(),
                Property = new Property()
            };

            //create
            if (id == null || id == 0)
            {
                return View(propertyVM);
            }
            else
            {
                //update
                propertyVM.Property = _unitOfWork.Property.Get(u => u.Id == id);
                return View(propertyVM);
            }
        }

        public async Task<List<ApplicationUser>> GetAdminAndAgentUsersAsync()
        {
            var adminAndAgentUsers = new List<ApplicationUser>();
            var agentList =  await _userManager.GetUsersInRoleAsync(SD.Role_Agent);
            var adminList =  await _userManager.GetUsersInRoleAsync(SD.Role_Admin);
            adminAndAgentUsers.AddRange(agentList);
            adminAndAgentUsers.AddRange(adminList);
            adminAndAgentUsers = adminAndAgentUsers.Distinct().ToList();

            return adminAndAgentUsers;
        }

        [HttpPost]
        public async Task<IActionResult> Upsert(PropertyVM propertyVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;//gets to wwwRoot folder
                if (file != null)
                {
                    //Generate random name for file + extension from upload
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    //Generate location to save file to
                    string propertyPath = Path.Combine(wwwRootPath, @"images/property");

                    //check if already present - as uploading new file + existing = update
                    if (!string.IsNullOrEmpty(propertyVM.Property.ImageUrl))
                    {
                        //delete old img

                        //generate path to old img
                        //need to remove leading backslash with trim
                        var oldImagePath = Path.Combine(wwwRootPath, propertyVM.Property.ImageUrl.TrimStart('\\'));

                        //delete old img
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    //using auto disposes of service post use
                    //FileStream to copy file(complete path name, mode)
                    using (var fileStream = new FileStream(Path.Combine(propertyPath, fileName), FileMode.Create))
                    {
                        //copy file to new location generated above
                        file.CopyTo(fileStream);
                    }

                    propertyVM.Property.ImageUrl = @"\images\property\" + fileName;
                }

                //check if update or create by whether id is 0 (create)
                if (propertyVM.Property.Id == 0)
                {
                    _unitOfWork.Property.Add(propertyVM.Property);
                }
                else
                {
                    _unitOfWork.Property.Update(propertyVM.Property);
                }

                _unitOfWork.Save();
                TempData["success"] = "Property created successfully";
                return RedirectToAction("Index", "Properties");
            }
            else
            {
                //if issue, need to re-populate dropdown
                propertyVM.PropertyTypeList = _unitOfWork.PropertyType
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                var users = await GetAdminAndAgentUsersAsync() ?? new List<ApplicationUser>();
                propertyVM.ApplicationUserList = users.Select(user => new SelectListItem
                {
                    Text = user.UserName,
                    Value = user.Id.ToString() 
                }).ToList();
                return View(propertyVM);
            }

        }


        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            //Check role first then filter results if Agent to their properties
            if (User.IsInRole(SD.Role_Agent))
            {
                List<Property> objPropertyList = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser").Where(x => x.ApplicationUser.UserName == User.Identity.Name).ToList();
                return Json(new { data = objPropertyList });
            }
            // if Admin, show all
            else
            {
                List<Property> objPropertyList = _unitOfWork.Property.GetAll(includeProperties: "PropertyType,ApplicationUser").ToList();
                return Json(new { data = objPropertyList });
            }          
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var propertyToBeDeleted = _unitOfWork.Property.Get(u => u.Id == id);
            if (propertyToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while Deleting" });
            }

            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, propertyToBeDeleted.ImageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Property.Remove(propertyToBeDeleted);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successful" });
        }

        #endregion

    }
}
