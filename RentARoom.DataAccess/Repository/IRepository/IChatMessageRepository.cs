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

    }
}
