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
            var userList = await _userService.GetAllUsersWithRoles();                   
            return View(userList);
        }


        #region API CALLS
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersWithRoles();
            return Json(new { data = users });
        }

        [HttpDelete]
        [Route("Admin/Admin/DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var success = await _userService.DeleteUser(id);
            if (success) 
            { 
                return Json(new { success = true, message = "User deleted successfully" });
            }
            return BadRequest(new { success = false, message = "Error deleting user" });
        }
        #endregion
    }
}
