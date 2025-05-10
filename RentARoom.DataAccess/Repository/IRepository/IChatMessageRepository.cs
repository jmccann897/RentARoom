using Microsoft.VisualBasic;
using System;
using RentARoom.Models;

namespace RentARoom.DataAccess.Repository.IRepository
{
    public interface IChatMessageRepository : IRepository<ChatMessage>
    {
        /// <summary>
        /// Get messages for conversation based on conversation id
        /// </summary>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        Task<IEnumerable<ChatMessage>> GetMessagesByConversationIdAsync(string conversationId);

        /// <summary>
        /// Retrieves a list of chat messages sent or received by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose chat messages are to be retrieved.</param>
        /// <returns>A task representing the asynchronous operation, containing a list of <see cref="ChatMessage"/> objects.</returns>
        Task<List<ChatMessage>> GetMessagesForUserAsync(string userId);

        /// <summary>
        /// Removes all chat messages sent or received by a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user whose chat messages should be deleted.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task RemoveMessagesForUserAsync(string userId);
    }
}
