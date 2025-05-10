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

        /// <summary>
        /// Retrieves the list of conversation participants for a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user for whom the conversation participants are being retrieved.</param>
        /// <returns>A task representing the asynchronous operation, with a result of a list of <see cref="ChatConversationParticipant"/> objects.</returns>
        Task<List<ChatConversationParticipant>> GetConversationParticipantsForUserAsync(string userId);

        /// <summary>
        /// Removes all conversation participants associated with a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user for whom the conversation participants are being removed.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task RemoveConversationParticipantsForUserAsync(string userId);

    }
}
