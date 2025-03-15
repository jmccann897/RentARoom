using Microsoft.VisualBasic;
using System;
using RentARoom.Models;

namespace RentARoom.DataAccess.Repository.IRepository
{
    public interface IChatConversationRepository : IRepository<ChatConversation>
    {
        /// <summary>
        /// Get conversations for a user by their user id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<IEnumerable<ChatConversation>> GetUserConversationsAsync(string userId);

        /// <summary>
        /// Get conversation ids async based on users id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<string>> GetConversationIdsByUserIdAsync(string userId);

    }
}
