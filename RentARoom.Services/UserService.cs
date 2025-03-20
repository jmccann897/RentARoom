using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentARoom.Models;
using RentARoom.Models.DTOs;
using RentARoom.Utility;
using System.Security.Claims;

namespace RentARoom.Services.IServices
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService (UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<ApplicationUser>> GetAdminAndAgentUsersAsync()
        {
            var adminAndAgentUsers = new List<ApplicationUser>();
            var agentList = await _userManager.GetUsersInRoleAsync(SD.Role_Agent);
            var adminList = await _userManager.GetUsersInRoleAsync(SD.Role_Admin);
            adminAndAgentUsers.AddRange(agentList);
            adminAndAgentUsers.AddRange(adminList);
            adminAndAgentUsers = adminAndAgentUsers.Distinct().ToList();

            return adminAndAgentUsers;
        }

        public async Task<List<ApplicationUser>> GetAllUsers()
        {
            var allUsers = new List<ApplicationUser>();
            var agentList = await _userManager.GetUsersInRoleAsync(SD.Role_Agent);
            var adminList = await _userManager.GetUsersInRoleAsync(SD.Role_Admin);
            var userList = await _userManager.GetUsersInRoleAsync(SD.Role_User);
            allUsers.AddRange(agentList);
            allUsers.AddRange(adminList);
            allUsers.AddRange(userList);
            allUsers = allUsers.Distinct().ToList();

            return allUsers;
        }

        public async Task<UserDTO> GetUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;
            return new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = _userManager.GetRolesAsync(user).Result.FirstOrDefault()
            };
        }

        // #region Admin Controller

        // Check if user is an admin
        public async Task<bool> IsUserAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;
            return await _userManager.IsInRoleAsync(user, SD.Role_Admin);
        }

        // Get all users with role included
        public async Task<List<UserDTO>> GetAllUsersWithRoles()
        {
            var allUsers = await _userManager.Users.ToListAsync();
            var userDTOs = allUsers.Select(user => new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = _userManager.GetRolesAsync(user).Result.FirstOrDefault()
            }).ToList();
          
            return userDTOs;
        }

        // Delete user by ID
        public async Task<bool> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }


        // #endregion

        // #region Property Controller
        public async Task<ApplicationUser> GetCurrentUserAsync(ClaimsPrincipal user)
        {
            if (user == null) { throw new ArgumentNullException(nameof(user), "ClaimsPrincipal cannot be null."); }
            return await _userManager.GetUserAsync(user);
        }
        // #endregion

        // #region Notification Controller
        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email)) // Check for null or empty string
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            if (string.IsNullOrEmpty(id)) // Check for null or empty string
            {
                throw new ArgumentException("Id cannot be null or empty.", nameof(id));
            }
            return await _userManager.FindByIdAsync(id);
        }

        public async Task<bool> CheckUserEmailExistsAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email cannot be null or empty.", nameof(email));
            }
            var user = await _userManager.FindByEmailAsync(email);
            return user != null;
        }
        // #endregion
    }
}
