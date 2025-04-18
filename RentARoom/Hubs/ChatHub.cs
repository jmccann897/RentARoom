﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NuGet.Packaging.Signing;
using RentARoom.DataAccess.Data;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Services.IServices;
using RentARoom.Models;
using Newtonsoft.Json.Linq;

namespace RentARoom.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ChatHub> _logger;
        private readonly IChatService _chatService;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public ChatHub(ApplicationDbContext db, ILogger<ChatHub> logger, IChatService chatService, IHubContext<NotificationHub> notificationHub)
        {
            _db = db;
            _logger = logger;
            _chatService = chatService;
            _notificationHub = notificationHub;
        }

        // Join conversation (SignalR Group)
        public async Task JoinConversation(string conversationId)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User is not authenticated");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
            _logger.LogInformation("User {UserId} joined conversation {ConversationId}", userId, conversationId);

        }

        // Leave conversation (SignalR Group)
        public async Task LeaveConversation(string conversationId)
        {
            var userId = Context.UserIdentifier;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User is not authenticated");
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId);
            _logger.LogInformation("User {UserId} left conversation {ConversationId}", userId, conversationId);
        }

        // Send message to specified receiver (now broadcasts to group of 2)
        // Default authorisation based on if email has been confirmed by aspnet Identity
        [Authorize]
        public async Task SendMessageToReceiver(string senderEmail, string receiverEmail, string message, string? propertyId)
        {
            try
            {
                // Get senderId from SignalR context
                var senderId = Context.UserIdentifier;
                if (string.IsNullOrWhiteSpace(senderId))
                {
                    throw new HubException("User not authenticated.");
                }

                // Validate inputs
                if (message == null || string.IsNullOrWhiteSpace(receiverEmail))
                {
                    throw new ArgumentException("Invalid message or recipient");
                }

                // LINQ to find the receiver's user Id
                var receiverUser = _db.ApplicationUsers.FirstOrDefault(u => u.Email.ToLower() == receiverEmail.ToLower());

                if (receiverUser == null)
                {
                    // Handle case where receiver is not found
                    throw new HubException("The specified receiver does not exist.");
                }

                // ReceiverId
                var receiverId = receiverUser.Id;

                // Handle propertyId
                int? parsedPropertyId = null;
                if (!string.IsNullOrWhiteSpace(propertyId))
                {
                    if (!int.TryParse(propertyId, out int propertyIdInt))
                    {
                        // Optionally log the invalid propertyId
                        _logger.LogWarning("Invalid propertyId format: {PropertyId}", propertyId);
                        // You can choose to set parsedPropertyId to null, or you can set a default value like 0
                        parsedPropertyId = null;
                    }
                    else
                    {
                        parsedPropertyId = propertyIdInt;
                    }
                }
                // Create or get conversationId

                var conversationId = await _chatService.CreateOrGetConversationIdAsync(senderId, receiverId, parsedPropertyId);

                // Save message
                ChatMessage savedMessage = await _chatService.SaveMessageAsync(conversationId, senderId, receiverId, message);


                // Log sender and recipient details
                _logger.LogInformation("Sending message from {Sender} to {Receiver}.", senderEmail, receiverEmail);

                var messagePayload = new
                {
                    ConversationId = conversationId,
                    SenderEmail = senderEmail,
                    ReceiverEmail = receiverEmail,
                    Content = message,
                    Timestamp = DateTime.UtcNow,
                    SenderId = senderId,
                    ChatMessageId = savedMessage.ChatMessageId,
                    PropertyId = parsedPropertyId
                };

                // Send message to the specified receiver
                await Clients.User(receiverId).SendAsync("MessageReceived", messagePayload);

                // Notify the sender's client to update the chat window
                await Clients.User(senderId).SendAsync("MessageAppended", messagePayload);

                // Call notification hub to send user notification
                await _notificationHub.Clients.User(receiverId).SendAsync("ReceiveChatMessageNotification", message, conversationId);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error sending message: {Message}", ex.Message);
                throw new HubException("An error occurred while sending the message.");
            }
           
        }

        public async Task AddToConversationOnMessage(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId);
            _logger.LogInformation("Connection {ConnectionId} added to group {ConversationId}", Context.ConnectionId, conversationId);
        }
    }
}
