using Azure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.DataAccess.Services.IServices;
using RentARoom.Models;
using RentARoom.Services;
using RentARoom.Utility;

namespace RentARoom.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserService _userService;

        public AdminController(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            // Need to add a datatable fo the users
            // Fetch the logged-in user and check their admin role
            var applicationUser = await _userManager.GetUserAsync(User);
            if (applicationUser == null || !User.IsInRole(SD.Role_Admin))
            {
                return Unauthorized();
            }
            
            var userList = await _userService.GetAllUsers();                   
            
            // Need to pass in users
            return View(userList);
        }


        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            // Check role first then filter results if Agent to their properties
            if (!User.IsInRole(SD.Role_Admin))
            {
                return Unauthorized();
            }
            
            var users = await _userService.GetAllUsers();
            var formattedUsers = users.Select(u => new
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = _userManager.GetRolesAsync(u).Result.FirstOrDefault()
            });

            return Json(new { data = formattedUsers });

        }

        [HttpDelete]
        [Route("Admin/Admin/DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var userToBeDeleted =  await _userService.GetUserById(id);
            if (userToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while Deleting - User not found" });
            }
            var result = await _userManager.DeleteAsync(userToBeDeleted);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "User deleted successfully" });
            }
            else
            {
                return BadRequest(new { success = false, message="Error deleting user" });
            }    
        }


        #endregion
    }
}
