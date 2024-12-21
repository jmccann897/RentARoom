using Microsoft.AspNetCore.Identity;
using RentARoom.DataAccess.Services.IServices;
using RentARoom.Models;
using RentARoom.Utility;

namespace RentARoom.Services
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

    }
}
