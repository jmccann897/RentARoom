﻿using RentARoom.Models;
using RentARoom.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Services.IServices
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
        ///  Checks if a user is an admin
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> IsUserAdmin(string userId);

        /// <summary>
        /// Retrieves all users with their roles included
        /// </summary>
        /// <returns></returns>
        Task<List<UserDTO>> GetAllUsersWithRoles();

        /// <summary>
        /// Deletes a user by Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<bool> DeleteUser(string userId);
        // #endregion


        // #region Property Controller
        /// <summary>
        /// Gets the current applicationuser based on the claims principal of user in window
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<ApplicationUser> GetCurrentUserAsync(ClaimsPrincipal user);
        // #endregion

        // #region Notification Controller
        /// <summary>
        ///  Gets a user based on their email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<ApplicationUser> GetUserByEmailAsync(string email);

        /// <summary>
        /// Gets a user by their id async
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<ApplicationUser> GetUserByIdAsync(string id);

        /// <summary>
        /// Bool check if user exists by their email async
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        Task<bool> CheckUserEmailExistsAsync(string email);

        // #endregion
    }
}
