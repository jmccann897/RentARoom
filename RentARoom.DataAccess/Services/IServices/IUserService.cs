using RentARoom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.DataAccess.Services.IServices
{
    public interface IUserService
    {
        Task<List<ApplicationUser>> GetAdminAndAgentUsersAsync();
        Task<List<ApplicationUser>> GetAllUsers();
        Task<ApplicationUser> GetUserById(string userId);
    }
}
