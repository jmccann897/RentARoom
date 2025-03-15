using RentARoom.Models;
using RentARoom.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Services.IServices
{
    public interface IChatService
    {
        // Check conversation
        /// <summary>
        /// Checks if conversation exists and returns its id,
        /// Else creates it and return its id
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="recipientId"></param>
        /// <returns></returns>
        Task<string> CreateOrGetConversationIdAsync(string senderId, string recipientId);
        // Save messages to Db
        Task<ChatMessage> SaveMessageAsync(string conversationId, string senderId, string recipientId, string content);
        // Get chat history
        Task<IEnumerable<ChatMessage>> GetConversationMessagesAsync(string conversationId);
        // Get conversations
        Task<IEnumerable<ChatConversation>> GetUserConversationsAsync(string userId);

        Task<ApplicationUser> GetUserByEmailAsync(string email);

        Task<List<string>> GetUserConversationIdsAsync(string userId);
        Task<IEnumerable<ChatMessage>> GetMessagesByConversationIdAsync(string userId, string conversationId);
        Task<bool> IsUserPartOfConversationAsync(string userId, string conversationId);
        Task<IEnumerable<ChatConversationDTO>> GetUserExistingConversationsAsync(string userId);

        /// <summary>
        /// Get data for ChatVM to populate the conversation side bar
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="recipientEmail"></param>
        /// <returns></returns>
        Task<ChatDataDTO> GetUserConversationsDataAsync(string userId, string recipientEmail = null);

        /// <summary>
        /// Gets recipient email from userService then calls CreateOrGetConversationIdAsync
        /// </summary>
        /// <param name="senderId"></param>
        /// <param name="recipientEmail"></param>
        /// <returns></returns>
        Task<string> CreateOrGetConversationIdByEmailAsync(string senderId, string recipientEmail);

    }
}
