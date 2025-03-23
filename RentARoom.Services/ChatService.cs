using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentARoom.DataAccess.Data;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Services.IServices;
using RentARoom.Models;
using RentARoom.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RentARoom.Services.IServices
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly ILogger<ChatService> _logger;
        public ChatService(ILogger<ChatService> logger, IUnitOfWork unitOfWork, IUserService userService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        public async Task<string> CreateOrGetConversationIdAsync(string senderId, string recipientId)
        {
            if (string.IsNullOrEmpty(senderId))
            {
                throw new ArgumentNullException(nameof(senderId), "Sender ID cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(recipientId))
            {
                throw new ArgumentNullException(nameof(recipientId), "Recipient ID cannot be null or empty.");
            }

            // Check if conversation already exists
            var conversation = await _unitOfWork.ChatConversations
                .GetAsync(c => c.Participants.Any(p => p.UserId == senderId) &&
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

            _unitOfWork.ChatConversations.Add(newConversation);
            _unitOfWork.ChatConversationParticipants.AddRange(new List<ChatConversationParticipant>
            {
                new ChatConversationParticipant { ChatConversationId = newConversation.ChatConversationId, UserId = senderId },
                new ChatConversationParticipant { ChatConversationId = newConversation.ChatConversationId, UserId = recipientId }
            });

            await _unitOfWork.SaveAsync();

            return newConversation.ChatConversationId;
        }

        public async Task<IEnumerable<ChatMessage>> GetConversationMessagesAsync(string conversationId)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId), "Conversation ID cannot be null or empty.");
            }

            return await _unitOfWork.ChatMessages.GetMessagesByConversationIdAsync(conversationId);
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email), "Email cannot be null or empty.");
            }
            return await _userService.GetUserByEmailAsync(email);
        }

        public async Task<IEnumerable<ChatConversation>> GetUserConversationsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");
            }
            return await _unitOfWork.ChatConversations.GetUserConversationsAsync(userId);
        }

        public async Task<ChatMessage> SaveMessageAsync(string conversationId, string senderId, string recipientId, string content)
        {
            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId), "Conversation ID cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(senderId))
            {
                throw new ArgumentNullException(nameof(senderId), "Sender ID cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(recipientId))
            {
                throw new ArgumentNullException(nameof(recipientId), "Recipient ID cannot be null or empty.");
            }
            if (string.IsNullOrEmpty(content))
            {
                throw new ArgumentNullException(nameof(content), "Content cannot be null or empty.");
            }
            var message = new ChatMessage
            {
                ChatMessageId = Guid.NewGuid().ToString(),
                ChatConversationId = conversationId,
                SenderId = senderId,
                RecipientId = recipientId,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            _unitOfWork.ChatMessages.Add(message);
            await _unitOfWork.SaveAsync();

            // Log the saved message
            _logger.LogInformation("Message saved: {MessageId}, ConversationId: {ConversationId}, SenderId: {SenderId}, RecipientId: {RecipientId}",
                message.ChatMessageId, conversationId, senderId, recipientId);

            return message;
        }
        public async Task<List<string>> GetUserConversationIdsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");
            }
            var conversationIds = await _unitOfWork.ChatConversations.GetConversationIdsByUserIdAsync(userId);
            return conversationIds;
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesByConversationIdAsync(string userId, string conversationId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId), "Conversation ID cannot be null or empty.");
            }
            var isParticipant = await IsUserPartOfConversationAsync(userId, conversationId);
            if (!isParticipant)
            {
                return Enumerable.Empty<ChatMessage>(); // Return no messages if user isn't part of the conversation
            }

            return await _unitOfWork.ChatMessages.GetMessagesByConversationIdAsync(conversationId);
        }

        public async Task<bool> IsUserPartOfConversationAsync(string userId, string conversationId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(conversationId))
            {
                throw new ArgumentNullException(nameof(conversationId), "Conversation ID cannot be null or empty.");
            }
            return await _unitOfWork.ChatConversationParticipants.IsUserPartOfConversationAsync(userId, conversationId);
        }

        public async Task<IEnumerable<ChatConversationDTO>> GetUserExistingConversationsAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");
            }
            var conversations = await _unitOfWork.ChatConversations.GetUserConversationsAsync(userId);
            var result = conversations.Select(c => new ChatConversationDTO
            {
                ChatConversationId = c.ChatConversationId,
                RecipientEmail = c.Participants
                    .FirstOrDefault(p => p.UserId != userId)?.User.Email,
                LastMessage = c.ChatMessages.OrderByDescending(m => m.Timestamp).FirstOrDefault()?.Content,
                LastMessageTimestamp = c.ChatMessages.OrderByDescending(m => m.Timestamp).FirstOrDefault()?.Timestamp
            });
            return result;
        }

        public async Task<ChatDataDTO> GetUserConversationsDataAsync (string userId, string recipientEmail = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentNullException(nameof(userId), "User ID cannot be null or empty.");
            }
            // Fetch ids and convos for user
            var conversationIds = await _unitOfWork.ChatConversations.GetConversationIdsByUserIdAsync(userId);
            var conversations = await _unitOfWork.ChatConversations.GetUserConversationsAsync(userId);

            // Map the conversations to the DTO
            var conversationsDTO = await GetUserExistingConversationsAsync(userId);

            // Fetch application User
            var applicationUser = await _userService.GetUserByIdAsync(userId);

            var chatDataDTO = new ChatDataDTO
            {
                UserId = userId,
                ConversationIds = conversationIds,
                Conversations = conversationsDTO.ToList(),
                ApplicationUser = applicationUser,
                RecipientEmail = recipientEmail
            };
            return chatDataDTO;
        }

        public async Task<string> CreateOrGetConversationIdByEmailAsync(string senderId, string recipientEmail)
        {
            if (string.IsNullOrEmpty(senderId))
            {
                throw new ArgumentNullException(nameof(senderId), "Sender ID cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(recipientEmail))
            {
                throw new ArgumentNullException(nameof(recipientEmail), "Recipient email cannot be null or empty.");
            }
            // Use UserService to fetch the recipient Id from their email
            var recipient = await _userService.GetUserByEmailAsync(recipientEmail);

            if (recipient == null) { return null; }

            var recipientId = recipient.Id;

            return await CreateOrGetConversationIdAsync(senderId, recipientId);
        }
    }
}
