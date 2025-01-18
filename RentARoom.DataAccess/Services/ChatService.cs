using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentARoom.DataAccess.Data;
using RentARoom.DataAccess.Services.IServices;
using RentARoom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.DataAccess.Services
{
    public class ChatService : IChatService
    {

        private readonly ApplicationDbContext _db;
        private readonly ILogger<ChatService> _logger;
        public ChatService(ApplicationDbContext db, ILogger<ChatService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<string> CreateOrGetConversationIdAsync(string senderId, string recipientId)
        {
            // Check if conversation already exists
            var conversation = await _db.ChatConversations
                .Include(c => c.Participants)
                .FirstOrDefaultAsync(c => c.Participants.Any(p => p.UserId == senderId) &&
                                     c.Participants.Any(p => p.UserId == recipientId));

            if (conversation != null)
            {
                return conversation.ChatConversationId;
            }

            // Otherwise need to create a new conversation
            var newConversation = new ChatConversation
            {
                ChatConversationId = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _db.ChatConversations.Add(newConversation);
            _db.ChatConversationParticipants.AddRange(
                new ChatConversationParticipant { ChatConversationId = newConversation.ChatConversationId, UserId = senderId },
                new ChatConversationParticipant { ChatConversationId = newConversation.ChatConversationId, UserId = recipientId }
                );

            await _db.SaveChangesAsync();

            return newConversation.ChatConversationId;
        }

        public async Task<IEnumerable<ChatMessage>> GetConversationMessagesAsync(string conversationId)
        {
            return await _db.ChatMessages
                .Where(m => m.ChatConversationId == conversationId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<ChatConversation>> GetUserConversationsAsync(string userId)
        {
            return await _db.ChatConversations
                .Include(c => c.Participants)
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .ToListAsync();
        }

        public async Task SaveMessageAsync(string conversationId, string senderId, string recipientId, string content)
        {
            var message = new ChatMessage
            {
                ChatMessageId = Guid.NewGuid().ToString(),
                ChatConversationId = conversationId,
                SenderId = senderId,
                RecipientId = recipientId,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            _db.ChatMessages.Add(message);
            await _db.SaveChangesAsync();

            // Log the saved message
            _logger.LogInformation("Message saved: {MessageId}, ConversationId: {ConversationId}, SenderId: {SenderId}, RecipientId: {RecipientId}",
                message.ChatMessageId, conversationId, senderId, recipientId);
        }
        public async Task<List<string>> GetUserConversationIdsAsync(string userId)
        {
            var conversationIds = await _db.ChatConversations
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .Select(c => c.ChatConversationId)
                .ToListAsync();

            return conversationIds;
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesByConversationIdAsync(string userId, string conversationId)
        {
            var isParticipant = await IsUserPartOfConversationAsync(userId, conversationId);
            if (!isParticipant)
            {
                return Enumerable.Empty<ChatMessage>(); // Return no messages if user isn't part of the conversation
            }

            return await _db.ChatMessages
                .Where(msg => msg.ChatConversationId == conversationId)
                .OrderBy(msg => msg.Timestamp) // Ensure chronological order
                .ToListAsync();
        }

        public async Task<bool> IsUserPartOfConversationAsync(string userId, string conversationId)
        {
            return await _db.ChatConversationParticipants
        .AnyAsync(participant => participant.UserId == userId && participant.ChatConversationId == conversationId);
        }
    }
}
