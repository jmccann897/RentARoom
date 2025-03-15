using Microsoft.VisualBasic;
using System;
using RentARoom.Models;

namespace RentARoom.DataAccess.Repository.IRepository
{
    public interface IChatConversationParticipantRepository : IRepository<ChatConversationParticipant>
    {
        /// <summary>
        /// Check if user is part of a conversation based on the user id and conversation id
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="conversationId"></param>
        /// <returns></returns>
        Task<bool> IsUserPartOfConversationAsync(string userId, string conversationId);
    }
}
