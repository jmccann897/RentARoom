using RentARoom.Models;
using RentARoom.Models.DTOs;
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
        //Task<ApplicationUser> GetUserById(string userId);

        // #region Admin Controller
        /// <summary>
        /// Get a userDTO by id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<UserDTO> GetUserById(string userId);

        /// <summary>
        ///  Checks if a user is an admin.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> IsUserAdmin(string userId);

        /// <summary>
        /// Retrieves all users with their roles included.
        /// </summary>
        /// <returns></returns>
        Task<List<UserDTO>> GetAllUsersWithRoles();

        /// <summary>
        /// Deletes a user by Id.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> DeleteUser(string userId);
        // #endregion
    }
}
