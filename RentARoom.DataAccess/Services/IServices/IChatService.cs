using RentARoom.Models;
using RentARoom.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.DataAccess.Services.IServices
{
    public interface IChatService
    {
        // Check conversation
        Task<string> CreateOrGetConversationIdAsync(string senderId, string recipientId);
        // Save messages to Db
        Task SaveMessageAsync(string conversationId, string senderId, string recipientId, string content);
        // Get chat history
        Task<IEnumerable<ChatMessage>> GetConversationMessagesAsync(string conversationId);
        // Get conversations
        Task<IEnumerable<ChatConversation>> GetUserConversationsAsync(string userId);

        Task<ApplicationUser> GetUserByEmailAsync(string email);

        Task<List<string>> GetUserConversationIdsAsync(string userId);
        Task<IEnumerable<ChatMessage>> GetMessagesByConversationIdAsync(string userId, string conversationId);
        Task<bool> IsUserPartOfConversationAsync(string userId, string conversationId);
        Task<IEnumerable<ChatConversationDTO>> GetUserExistingConversationsAsync(string userId);

    }
}
