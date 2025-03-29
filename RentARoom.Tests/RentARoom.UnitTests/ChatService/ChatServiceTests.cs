using Microsoft.Extensions.Logging;
using NSubstitute;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Models;
using RentARoom.Services.IServices;
using System.Linq.Expressions;


namespace RentARoom.Tests.RentARoom.UnitTests
{
    public class ChatServiceTests
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly ILogger<ChatService> _logger;
        private readonly ChatService _chatService;

        public ChatServiceTests()
        {
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _userService = Substitute.For<IUserService>();
            _logger = Substitute.For<ILogger<ChatService>>();
            _chatService = new ChatService(_logger, _unitOfWork, _userService);
        }

        //#region CreateOrGetConversationIdAsync Tests

        //[Fact]
        //public async Task ChatService_CreateOrGetConversationIdAsync_Should_ReturnConversationId_IfExistingConversation()
        //{
        //    // Arrange
        //    string senderId = "sender123";
        //    string recipientId = "recipient456";
        //    string existingConversationId = Guid.NewGuid().ToString();

        //    var existingConversation = new ChatConversation
        //    {
        //        ChatConversationId = existingConversationId,
        //        Participants = new List<ChatConversationParticipant>
        //        {
        //            new ChatConversationParticipant { UserId = senderId },
        //            new ChatConversationParticipant { UserId = recipientId }
        //        }
        //    };

        //    _unitOfWork.ChatConversations
        //        .GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ChatConversation, bool>>>())
        //        .Returns(existingConversation);

        //    // Act
        //    string result = await _chatService.CreateOrGetConversationIdAsync(senderId, recipientId);

        //    // Assert
        //    Assert.Equal(existingConversationId, result);
        //}

        //[Fact]
        //public async Task ChatService_CreateOrGetConversationIdAsync_Should_CreateAndReturnConversationId_IfNoExistingConversation()
        //{
        //    // Arrange
        //    string senderId = "sender123";
        //    string recipientId = "recipient456";

        //    // Variables to capture the ChatConversation and ChatConversationParticipants that are added.
        //    ChatConversation capturedConversation = null;
        //    List<ChatConversationParticipant> capturedParticipants = null;

        //    // Simulate that no existing conversation is found.
        //    _unitOfWork.ChatConversations
        //        .GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ChatConversation, bool>>>())
        //        .Returns((ChatConversation)null);

        //    // Capture the ChatConversation added to the repository.
        //    _unitOfWork.ChatConversations
        //        .When(x => x.Add(Arg.Any<ChatConversation>()))
        //        .Do(x => capturedConversation = x.Arg<ChatConversation>());

        //    // Capture the list of ChatConversationParticipants added to the repository.
        //    _unitOfWork.ChatConversationParticipants
        //        .When(x => x.AddRange(Arg.Any<List<ChatConversationParticipant>>()))
        //        .Do(x => capturedParticipants = x.Arg<List<ChatConversationParticipant>>());

        //    // Simulate that SaveAsync() completes successfully.
        //    _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

        //    // Act
        //    string result = await _chatService.CreateOrGetConversationIdAsync(senderId, recipientId);

        //    // Assert
        //    // Ensure that a new conversation ID was generated.
        //    Assert.NotNull(result);

        //    // Ensure that a ChatConversation was added to the repository.
        //    Assert.NotNull(capturedConversation);

        //    // Ensure that a list of ChatConversationParticipants was added to the repository.
        //    Assert.NotNull(capturedParticipants);

        //    // Ensure that the returned conversation ID matches the ID of the added ChatConversation.
        //    Assert.Equal(result, capturedConversation.ChatConversationId);

        //    // Ensure that two participants were added.
        //    Assert.Equal(2, capturedParticipants.Count);

        //    // Ensure that the participants have the correct UserId and ChatConversationId.
        //    Assert.Contains(capturedParticipants, p => p.UserId == senderId && p.ChatConversationId == result);
        //    Assert.Contains(capturedParticipants, p => p.UserId == recipientId && p.ChatConversationId == result);

        //    // Verify that ChatConversations.Add(), ChatConversationParticipants.AddRange() and SaveAsync()  were called once each.
        //    _unitOfWork.ChatConversations.Received(1).Add(Arg.Any<ChatConversation>());
        //    _unitOfWork.ChatConversationParticipants.Received(1).AddRange(Arg.Any<List<ChatConversationParticipant>>());
        //    await _unitOfWork.Received(1).SaveAsync();
        //}

        //[Fact]
        //public async Task ChatService_CreateOrGetConversationIdAsync_Should_ThrowException_IfIdsInvalid()
        //{
        //    // Arrange
        //    string invalidSenderId = null;
        //    string invalidRecipientId = "";

        //    // Act & Assert
        //    await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        //        await _chatService.CreateOrGetConversationIdAsync(invalidSenderId, invalidRecipientId));

        //    // Verify that no interactions with the UnitOfWork occurred.
        //    await _unitOfWork.ChatConversations.DidNotReceive().GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ChatConversation, bool>>>());
        //    _unitOfWork.ChatConversations.DidNotReceive().Add(Arg.Any<ChatConversation>());
        //    _unitOfWork.ChatConversationParticipants.DidNotReceive().AddRange(Arg.Any<List<ChatConversationParticipant>>());
        //    await _unitOfWork.DidNotReceive().SaveAsync();
        //}

        //#endregion

        #region GetConversationMessagesAsync Tests

        [Fact]
        public async Task ChatService_GetConversationMessagesAsync_Should_ReturnMessages()
        {
            // Arrange
            string conversationId = Guid.NewGuid().ToString();
            var expectedMessages = new List<ChatMessage>
            {
                new ChatMessage { ChatMessageId = Guid.NewGuid().ToString(), ChatConversationId = conversationId, Content = "Hello!" },
                new ChatMessage { ChatMessageId = Guid.NewGuid().ToString(), ChatConversationId = conversationId, Content = "Hi there!" }
            };

            // Configure the mock to return the expected messages.
            _unitOfWork.ChatMessages.GetMessagesByConversationIdAsync(conversationId).Returns(expectedMessages);

            // Act
            var actualMessages = await _chatService.GetConversationMessagesAsync(conversationId);

            // Assert
            // Ensure that the returned messages are the same as the expected messages.
            Assert.NotNull(actualMessages);
            Assert.Equal(expectedMessages.Count, actualMessages.Count());
            // Check that the lists are equal
            Assert.Equal(expectedMessages, actualMessages);
            // Verify that the method was called once.
            await _unitOfWork.ChatMessages.Received(1).GetMessagesByConversationIdAsync(conversationId);
        }

        [Fact]
        public async Task ChatService_GetConversationMessagesAsync_Should_ThrowException_WhenInvalidId()
        {
            // Arrange
            string invalidConversationId = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _chatService.GetConversationMessagesAsync(invalidConversationId));

            // Verify that no interactions with the UnitOfWork occurred.
            await _unitOfWork.ChatMessages.DidNotReceive().GetMessagesByConversationIdAsync(Arg.Any<string>());

        }

        #endregion

        #region GetUserByEmailAsync Tests

        [Fact]
        public async Task ChatService_GetUserByEmailAsync_Should_ReturnUser()
        {
            // Arrange
            string email = "test@example.com";
            var expectedUser = new ApplicationUser { Id = "user123", Email = email };

            // Configure the mock UserService to return the expected user.
            _userService.GetUserByEmailAsync(email).Returns(expectedUser);

            // Act
            var actualUser = await _chatService.GetUserByEmailAsync(email);

            // Assert
            // Ensure that the returned user is the same as the expected user.
            Assert.NotNull(actualUser);
            Assert.Equal(expectedUser, actualUser);
            // Verify the service call.
            await _userService.Received(1).GetUserByEmailAsync(email); 
        }

        [Fact]
        public async Task ChatService_GetUserByEmailAsync_Should_ThrowException_WhenEmailIsNull()
        {
            // Arrange
            string nullEmail = null;
            string emptyEmail = "";

            // Act & Assert (null case)
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _chatService.GetUserByEmailAsync(nullEmail));

            // Act & Assert (empty case)
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _chatService.GetUserByEmailAsync(emptyEmail));

            // Verify that no interactions with the UserService occurred
            await _userService.DidNotReceive().GetUserByEmailAsync(Arg.Any<string>());

        }

        #endregion

        #region GetUserConversationsAsync Tests

        [Fact]
        public async Task ChatService_GetUserConversationsAsync_Should_ReturnConversations()
        {
            // Arrange
            string userId = "user123";
            var expectedConversations = new List<ChatConversation>
            {
                new ChatConversation { ChatConversationId = Guid.NewGuid().ToString(), Participants = new List<ChatConversationParticipant> { new ChatConversationParticipant { UserId = userId } } },
                new ChatConversation { ChatConversationId = Guid.NewGuid().ToString(), Participants = new List<ChatConversationParticipant> { new ChatConversationParticipant { UserId = userId } } }
            };

            // Configure the mock UnitOfWork to return the expected conversations.
            _unitOfWork.ChatConversations.GetUserConversationsAsync(userId).Returns(expectedConversations);

            // Act
            var actualConversations = await _chatService.GetUserConversationsAsync(userId);

            // Assert
            // Ensure that the returned conversations are the same as the expected conversations.
            Assert.NotNull(actualConversations);
            Assert.Equal(expectedConversations.Count, actualConversations.Count());
            Assert.Equal(expectedConversations, actualConversations);
            // Verify service call.
            await _unitOfWork.ChatConversations.Received(1).GetUserConversationsAsync(userId); 
        }

        [Fact]
        public async Task ChatService_GetUserConversationsAsync_Should_ThrowException_WhenInvalidId()
        {
            // Arrange
            string invalidUserId = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _chatService.GetUserConversationsAsync(invalidUserId));

            // Verify that no interactions with the UnitOfWork occurred.
            await _unitOfWork.ChatConversations.DidNotReceive().GetUserConversationsAsync(Arg.Any<string>());
        }
        #endregion

        #region SaveMessageAsync Tests

        [Fact]
        public async Task ChatService_SaveMessageAsync_Should_SaveAndReturnMessage()
        {
            // Arrange
            string conversationId = Guid.NewGuid().ToString();
            string senderId = "sender123";
            string recipientId = "recipient456";
            string content = "Hello, world!";

            ChatMessage capturedMessage = null;

            // Capture the ChatMessage added to the repository.
            _unitOfWork.ChatMessages
                .When(x => x.Add(Arg.Any<ChatMessage>()))
                .Do(x => capturedMessage = x.Arg<ChatMessage>());

            // Simulate that SaveAsync() completes successfully.
            _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

            // Act
            var result = await _chatService.SaveMessageAsync(conversationId, senderId, recipientId, content);

            // Assert
            // Ensure that the returned message is the same as the captured message.
            Assert.NotNull(result);
            Assert.NotNull(capturedMessage);
            Assert.Equal(result, capturedMessage);

            // Ensure that the captured message has the correct properties.
            Assert.Equal(conversationId, capturedMessage.ChatConversationId);
            Assert.Equal(senderId, capturedMessage.SenderId);
            Assert.Equal(recipientId, capturedMessage.RecipientId);
            Assert.Equal(content, capturedMessage.Content);
            Assert.NotEqual(default, capturedMessage.Timestamp); // Ensure timestamp is set.

            // Verify that ChatMessages.Add() and SaveAsync() were called once.
            _unitOfWork.ChatMessages.Received(1).Add(Arg.Any<ChatMessage>());
            await _unitOfWork.Received(1).SaveAsync();

            // Verify that the Logger was called at all.
            _logger.Received(1).Log(Arg.Any<LogLevel>(), Arg.Any<EventId>(), Arg.Any<object>(), Arg.Any<Exception>(), Arg.Any<Func<object, Exception, string>>());
        }

        [Fact]
        public async Task ChatService_SaveMessageAsync_Should_ThrowException_WhenInvalidConversationId()
        {
            // Arrange
            string invalidConversationId = null;
            string senderId = "sender123";
            string recipientId = "recipient456";
            string content = "Hello!";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _chatService.SaveMessageAsync(invalidConversationId, senderId, recipientId, content));

            // Verify that no interactions with the UnitOfWork occurred.
            _unitOfWork.ChatMessages.DidNotReceive().Add(Arg.Any<ChatMessage>());
            await _unitOfWork.DidNotReceive().SaveAsync();
        }

        [Fact]
        public async Task ChatService_SaveMessageAsync_Should_ThrowException_WhenInvalidUserIds()
        {
            // Arrange
            string conversationId = Guid.NewGuid().ToString();
            string invalidSenderId = null;
            string invalidRecipientId = "";
            string content = "Hello!";

            // Act & Assert (invalid senderId)
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _chatService.SaveMessageAsync(conversationId, invalidSenderId, "recipient456", content));

            // Act & Assert (invalid recipientId)
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _chatService.SaveMessageAsync(conversationId, "sender123", invalidRecipientId, content));

            // Verify that no interactions with the UnitOfWork occurred.
            _unitOfWork.ChatMessages.DidNotReceive().Add(Arg.Any<ChatMessage>());
            await _unitOfWork.DidNotReceive().SaveAsync();
        }

        [Fact]
        public async Task ChatService_SaveMessageAsync_Should_ThrowException_WhenInvalidContent()
        {
            // Arrange
            string conversationId = Guid.NewGuid().ToString();
            string senderId = "sender123";
            string recipientId = "recipient456";
            string invalidContent = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _chatService.SaveMessageAsync(conversationId, senderId, recipientId, invalidContent));

            // Verify that no interactions with the UnitOfWork occurred.
            _unitOfWork.ChatMessages.DidNotReceive().Add(Arg.Any<ChatMessage>());
            await _unitOfWork.DidNotReceive().SaveAsync();
        }

        #endregion

        #region GetUserConversationIdsAsync Tests

        [Fact]
        public async Task ChatService_GetUserConversationIdsAsync_Should_ReturnConversationIds()
        {
            // Arrange
            string userId = "user123";
            var expectedConversationIds = new List<string>
            {
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString()
            };

            // Configure the mock UnitOfWork to return the expected conversation IDs.
            _unitOfWork.ChatConversations.GetConversationIdsByUserIdAsync(userId).Returns(expectedConversationIds);

            // Act
            var actualConversationIds = await _chatService.GetUserConversationIdsAsync(userId);

            // Assert
            // Ensure that the returned conversation IDs are the same as the expected IDs.
            Assert.NotNull(actualConversationIds);
            Assert.Equal(expectedConversationIds.Count, actualConversationIds.Count);
            Assert.Equal(expectedConversationIds, actualConversationIds);
            // Verify the service call
            await _unitOfWork.ChatConversations.Received(1).GetConversationIdsByUserIdAsync(userId); 
        }

        [Fact]
        public async Task ChatService_GetUserConversationIdsAsync_Should_ThrowException_WhenInvalidUserId()
        {
            // Arrange
            string invalidUserId = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _chatService.GetUserConversationIdsAsync(invalidUserId));

            // Verify that no interactions with the UnitOfWork occurred.
            await _unitOfWork.ChatConversations.DidNotReceive().GetConversationIdsByUserIdAsync(Arg.Any<string>());
        }

        #endregion

        #region GetMessagesByConversationIdAsync Tests

        [Fact]
        public async Task ChatService_GetMessagesByConversationIdAsync_Should_ReturnMessages_WhenUserIsParticipant()
        {
            // Arrange
            string userId = "user123";
            string conversationId = Guid.NewGuid().ToString();
            var expectedMessages = new List<ChatMessage>
            {
                new ChatMessage { ChatMessageId = Guid.NewGuid().ToString(), ChatConversationId = conversationId, Content = "Hello!" },
                new ChatMessage { ChatMessageId = Guid.NewGuid().ToString(), ChatConversationId = conversationId, Content = "Hi there!" }
            };

            // Configure the mocks.
            _unitOfWork.ChatConversationParticipants.IsUserPartOfConversationAsync(userId, conversationId).Returns(true);
            _unitOfWork.ChatMessages.GetMessagesByConversationIdAsync(conversationId).Returns(expectedMessages);

            // Act
            var actualMessages = await _chatService.GetMessagesByConversationIdAsync(userId, conversationId);

            // Assert
            // Ensure that the returned messages are the same as the expected messages.
            Assert.NotNull(actualMessages);
            Assert.Equal(expectedMessages.Count, actualMessages.Count());
            Assert.Equal(expectedMessages, actualMessages);

            // Verify the mocks were called.
            await _unitOfWork.ChatConversationParticipants.Received(1).IsUserPartOfConversationAsync(userId, conversationId);
            await _unitOfWork.ChatMessages.Received(1).GetMessagesByConversationIdAsync(conversationId);
        }

        [Fact]
        public async Task ChatService_GetMessagesByConversationIdAsync_Should_ReturnEmptyList_WhenUserIsNotParticipant()
        {
            // Arrange
            string userId = "user123";
            string conversationId = Guid.NewGuid().ToString();

            // Configure the mock to return false, simulating that the user is not a participant.
            _unitOfWork.ChatConversationParticipants.IsUserPartOfConversationAsync(userId, conversationId).Returns(false);

            // Act
            var actualMessages = await _chatService.GetMessagesByConversationIdAsync(userId, conversationId);

            // Assert
            // Ensure that the returned list is empty.
            Assert.NotNull(actualMessages);
            Assert.Empty(actualMessages);

            // Verify that IsUserPartOfConversationAsync was called and GetMessagesByConversationIdAsync was not called.
            await _unitOfWork.ChatConversationParticipants.Received(1).IsUserPartOfConversationAsync(userId, conversationId);
            await _unitOfWork.ChatMessages.DidNotReceive().GetMessagesByConversationIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task ChatService_GetMessagesByConversationIdAsync_Should_ThrowException_WhenUserIdInvalid()
        {
            // Arrange
            string invalidUserId = null;
            string conversationId = Guid.NewGuid().ToString();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _chatService.GetMessagesByConversationIdAsync(invalidUserId, conversationId));

            // Verify that no interactions with the UnitOfWork occurred.
            await _unitOfWork.ChatConversationParticipants.DidNotReceive().IsUserPartOfConversationAsync(Arg.Any<string>(), Arg.Any<string>());
            await _unitOfWork.ChatMessages.DidNotReceive().GetMessagesByConversationIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task ChatService_GetMessagesByConversationIdAsync_Should_ThrowException_WhenConversationIdInvalid()
        {
            // Arrange
            string userId = "user123";
            string invalidConversationId = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _chatService.GetMessagesByConversationIdAsync(userId, invalidConversationId));

            // Verify that no interactions with the UnitOfWork occurred.
            await _unitOfWork.ChatConversationParticipants.DidNotReceive().IsUserPartOfConversationAsync(Arg.Any<string>(), Arg.Any<string>());
            await _unitOfWork.ChatMessages.DidNotReceive().GetMessagesByConversationIdAsync(Arg.Any<string>());
        }

        #endregion

        #region IsUserPartOfConversationAsync Tests

        [Fact]
        public async Task ChatService_IsUserPartOfConversationAsync_Should_ReturnTrue_WhenUserIsParticipant()
        {
            // Arrange
            string userId = "user123";
            string conversationId = Guid.NewGuid().ToString();

            // Configure the mock to return true.
            _unitOfWork.ChatConversationParticipants.IsUserPartOfConversationAsync(userId, conversationId).Returns(true);

            // Act
            var result = await _chatService.IsUserPartOfConversationAsync(userId, conversationId);

            // Assert
            // Ensure that the result is true.
            Assert.True(result);

            // Verify the mock was called.
            await _unitOfWork.ChatConversationParticipants.Received(1).IsUserPartOfConversationAsync(userId, conversationId);
        }

        [Fact]
        public async Task ChatService_IsUserPartOfConversationAsync_Should_ReturnFalse_WhenUserIsNotParticipant()
        {
            // Arrange
            string userId = "user123";
            string conversationId = Guid.NewGuid().ToString();

            // Configure the mock to return false.
            _unitOfWork.ChatConversationParticipants.IsUserPartOfConversationAsync(userId, conversationId).Returns(false);

            // Act
            var result = await _chatService.IsUserPartOfConversationAsync(userId, conversationId);

            // Assert
            // Ensure that the result is false.
            Assert.False(result);

            // Verify the mock was called.
            await _unitOfWork.ChatConversationParticipants.Received(1).IsUserPartOfConversationAsync(userId, conversationId);
        }

        [Fact]
        public async Task ChatService_IsUserPartOfConversationAsync_Should_ThrowException_WhenUserIdInvalid()
        {
            // Arrange
            string invalidUserId = null;
            string conversationId = Guid.NewGuid().ToString();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _chatService.IsUserPartOfConversationAsync(invalidUserId, conversationId));

            // Verify that no interactions with the UnitOfWork occurred.
            await _unitOfWork.ChatConversationParticipants.DidNotReceive().IsUserPartOfConversationAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ChatService_IsUserPartOfConversationAsync_Should_ThrowException_WhenConversationIdInvalid()
        {
            // Arrange
            string userId = "user123";
            string invalidConversationId = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _chatService.IsUserPartOfConversationAsync(userId, invalidConversationId));

            // Verify that no interactions with the UnitOfWork occurred.
            await _unitOfWork.ChatConversationParticipants.DidNotReceive().IsUserPartOfConversationAsync(Arg.Any<string>(), Arg.Any<string>());
        }
        #endregion

        #region GetUserExistingConversationsAsync Tests

        [Fact]
        public async Task ChatService_GetUserExistingConversationsAsync_Should_ReturnConversationDTOs()
        {
            // Arrange
            string userId = "user123";
            var conversations = new List<ChatConversation>
            {
                new ChatConversation
                {
                    ChatConversationId = Guid.NewGuid().ToString(),
                    Participants = new List<ChatConversationParticipant>
                    {
                        new ChatConversationParticipant { UserId = userId, User = new ApplicationUser { Email = "user123@example.com" } },
                        new ChatConversationParticipant { UserId = "user456", User = new ApplicationUser { Email = "user456@example.com" } }
                    },
                    ChatMessages = new List<ChatMessage>
                    {
                        new ChatMessage { Content = "Last message 1", Timestamp = DateTime.UtcNow.AddMinutes(-10) }
                    }
                },
                new ChatConversation
                {
                    ChatConversationId = Guid.NewGuid().ToString(),
                    Participants = new List<ChatConversationParticipant>
                    {
                        new ChatConversationParticipant { UserId = userId, User = new ApplicationUser { Email = "user123@example.com" } },
                        new ChatConversationParticipant { UserId = "user789", User = new ApplicationUser { Email = "user789@example.com" } }
                    },
                    ChatMessages = new List<ChatMessage>
                    {
                        new ChatMessage { Content = "Last message 2", Timestamp = DateTime.UtcNow.AddMinutes(-5) }
                    }
                }
            };

            // Configure the mock UnitOfWork to return the conversations.
            _unitOfWork.ChatConversations.GetUserConversationsAsync(userId).Returns(conversations);

            // Act
            var result = await _chatService.GetUserExistingConversationsAsync(userId);

            // Assert
            // Ensure the result is not null and has the correct count.
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());

            // Assert the properties of the first DTO.
            Assert.Equal(conversations[0].ChatConversationId, result.First().ChatConversationId);
            Assert.Equal("user456@example.com", result.First().RecipientEmail);
            Assert.Equal("Last message 1", result.First().LastMessage);
            Assert.Equal(conversations[0].ChatMessages.First().Timestamp, result.First().LastMessageTimestamp);

            // Assert the properties of the second DTO.
            Assert.Equal(conversations[1].ChatConversationId, result.Last().ChatConversationId);
            Assert.Equal("user789@example.com", result.Last().RecipientEmail);
            Assert.Equal("Last message 2", result.Last().LastMessage);
            Assert.Equal(conversations[1].ChatMessages.First().Timestamp, result.Last().LastMessageTimestamp);

            // Verify the mock was called.
            await _unitOfWork.ChatConversations.Received(1).GetUserConversationsAsync(userId);
        }

        [Fact]
        public async Task ChatService_GetUserExistingConversationsAsync_Should_ThrowException_WhenUserIdInvalid()
        {
            // Arrange
            string invalidUserId = null;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _chatService.GetUserExistingConversationsAsync(invalidUserId));

            // Verify that no interactions with the UnitOfWork occurred.
            await _unitOfWork.ChatConversations.DidNotReceive().GetUserConversationsAsync(Arg.Any<string>());
        }

        #endregion

        #region GetUserConversationsDataAsync Tests

        //[Fact]
        //public async Task ChatService_GetUserConversationsDataAsync_Should_ReturnChatDataDTO()
        //{
        //    // Arrange
        //    string userId = "user123";
        //    string recipientEmail = "recipient@example.com";
        //    var conversations = new List<ChatConversation>
        //    {
        //        new ChatConversation
        //        {
        //            ChatConversationId = Guid.NewGuid().ToString(),
        //            Participants = new List<ChatConversationParticipant>
        //            {
        //                new ChatConversationParticipant { UserId = userId, User = new ApplicationUser { Email = "user123@example.com" } },
        //                new ChatConversationParticipant { UserId = "user456", User = new ApplicationUser { Email = "user456@example.com" } }
        //            },
        //            ChatMessages = new List<ChatMessage>
        //            {
        //                new ChatMessage { Content = "Last message 1", Timestamp = DateTime.UtcNow.AddMinutes(-10) }
        //            }
        //        }
        //    };

        //    var conversationIds = conversations.Select(c => c.ChatConversationId).ToList();

        //    var applicationUser = new ApplicationUser { Id = userId, Email = "user123@example.com" };

        //    // Configure the mocks.
        //    _unitOfWork.ChatConversations.GetConversationIdsByUserIdAsync(userId).Returns(conversationIds);
        //    _unitOfWork.ChatConversations.GetUserConversationsAsync(userId).Returns(conversations);
        //    _userService.GetUserByIdAsync(userId).Returns(applicationUser);

        //    // Act
        //    var result = await _chatService.GetUserConversationsDataAsync(userId, recipientEmail);

        //    // Assert
        //    // Ensure the result is not null.
        //    Assert.NotNull(result);

        //    // Assert the properties of the returned DTO.
        //    Assert.Equal(userId, result.UserId);
        //    Assert.Equal(conversationIds, result.ConversationIds);
        //    Assert.Equal(applicationUser, result.ApplicationUser);
        //    Assert.Equal(recipientEmail, result.RecipientEmail);

        //    // Ensure the Conversations are mapped correctly
        //    Assert.NotNull(result.Conversations);
        //    Assert.Single(result.Conversations);
        //    Assert.Equal(conversations.First().ChatConversationId, result.Conversations.First().ChatConversationId);

        //    // Verify the mocks were called.
        //    await _unitOfWork.ChatConversations.Received(1).GetConversationIdsByUserIdAsync(userId);
        //    await _unitOfWork.ChatConversations.Received(2).GetUserConversationsAsync(userId); // 2 times as GetUserConversationsDataAsync calls it also
        //    await _userService.Received(1).GetUserByIdAsync(userId);
        //}

        [Fact]
        public async Task ChatService_GetUserConversationsDataAsync_Should_ThrowException_WhenUserIdInvalid()
        {
            // Arrange
            string invalidUserId = null;
            string recipientEmail = "recipient@example.com";

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await _chatService.GetUserConversationsDataAsync(invalidUserId, recipientEmail));

            // Verify that no interactions with the UnitOfWork occurred.
            await _unitOfWork.ChatConversations.DidNotReceive().GetConversationIdsByUserIdAsync(Arg.Any<string>());
            await _unitOfWork.ChatConversations.DidNotReceive().GetUserConversationsAsync(Arg.Any<string>());
            await _userService.DidNotReceive().GetUserByIdAsync(Arg.Any<string>());
        }
        #endregion

        //#region CreateOrGetConversationIdByEmailAsync Tests

        //[Fact(Skip = "Temporarily skipping due to PropertyId implementation changes.")]
        //public async Task ChatService_CreateOrGetConversationIdByEmailAsync_Should_ReturnConversationId_WhenEmailValid()
        //{
        //    // Arrange
        //    string senderId = "sender123";
        //    string recipientEmail = "recipient@example.com";
        //    string recipientId = "recipient456";
        //    string expectedConversationId = Guid.NewGuid().ToString();

        //    // Configure the mocks.
        //    _userService.GetUserByEmailAsync(recipientEmail).Returns(new ApplicationUser { Id = recipientId });
        //    // Simulate no existing conversation
        //    _unitOfWork.ChatConversations
        //        .GetAsync(Arg.Any<Expression<Func<ChatConversation, bool>>>())
        //        .Returns((ChatConversation)null); 
        //    _unitOfWork.ChatConversations
        //        .When(x => x.Add(Arg.Any<ChatConversation>()))
        //        .Do(x => x.Arg<ChatConversation>().ChatConversationId = expectedConversationId);
        //    _unitOfWork.SaveAsync().Returns(Task.CompletedTask);

        //    // Act
        //    var result = await _chatService.CreateOrGetConversationIdByEmailAsync(senderId, recipientEmail);

        //    // Assert
        //    // Ensure the result is not null and is the expected conversation ID.
        //    Assert.NotNull(result);
        //    Assert.Equal(expectedConversationId, result);

        //    // Verify the mocks were called.
        //    await _userService.Received(1).GetUserByEmailAsync(recipientEmail);
        //    await _unitOfWork.Received(1).SaveAsync();
        //}

        //[Fact(Skip = "Temporarily skipping due to PropertyId implementation changes.")]
        //public async Task ChatService_CreateOrGetConversationIdByEmailAsync_Should_ReturnNull_WhenEmailInvalid()
        //{
        //    // Arrange
        //    string senderId = "sender123";
        //    string invalidEmail = "invalid@example.com";

        //    // Configure the UserService to return null for an invalid email.
        //    _userService.GetUserByEmailAsync(invalidEmail).Returns((ApplicationUser)null);

        //    // Act
        //    var result = await _chatService.CreateOrGetConversationIdByEmailAsync(senderId, invalidEmail);

        //    // Assert
        //    // Ensure the result is null.
        //    Assert.Null(result);

        //    // Verify the UserService was called.
        //    await _userService.Received(1).GetUserByEmailAsync(invalidEmail);

        //    // Verify that save async was not called.
        //    await _unitOfWork.DidNotReceive().SaveAsync();
        //}

        //[Fact(Skip = "Temporarily skipping due to PropertyId implementation changes.")]
        //public async Task ChatService_CreateOrGetConversationIdByEmailAsync_Should_ThrowException_WhenUserIdInvalid()
        //{
        //    // Arrange
        //    string nullSenderId = null;
        //    string emptySenderId = "";
        //    string nullRecipientEmail = null;
        //    string emptyRecipientEmail = "";

        //    // Act & Assert (null senderId)
        //    await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        //        await _chatService.CreateOrGetConversationIdByEmailAsync(nullSenderId, "recipient@example.com"));

        //    // Act & Assert (empty senderId)
        //    await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        //        await _chatService.CreateOrGetConversationIdByEmailAsync(emptySenderId, "recipient@example.com"));

        //    // Act & Assert (null recipientEmail)
        //    await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        //        await _chatService.CreateOrGetConversationIdByEmailAsync("sender123", nullRecipientEmail));

        //    // Act & Assert (empty recipientEmail)
        //    await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        //        await _chatService.CreateOrGetConversationIdByEmailAsync("sender123", emptyRecipientEmail));

        //    // Verify that no interactions with the UnitOfWork or UserService occurred.
        //    await _userService.DidNotReceive().GetUserByEmailAsync(Arg.Any<string>());
        //    await _unitOfWork.ChatConversations.DidNotReceive().GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<ChatConversation, bool>>>());
        //    _unitOfWork.ChatConversations.DidNotReceive().Add(Arg.Any<ChatConversation>());
        //    _unitOfWork.ChatConversationParticipants.DidNotReceive().AddRange(Arg.Any<List<ChatConversationParticipant>>());
        //    await _unitOfWork.DidNotReceive().SaveAsync();
        //}

        //#endregion
    }
}
