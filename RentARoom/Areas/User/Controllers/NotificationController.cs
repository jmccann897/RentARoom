using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.Services.IServices;
using RentARoom.Models.ViewModels;
using System.Security.Claims;

namespace RentARoom.Areas.User.Controllers
{
    [Area("User")]
    public class NotificationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatService _chatService;
        private readonly IUserService _userService;
        private readonly IPropertyService _propertyService;

        public NotificationController(IUnitOfWork unitOfWork, IChatService chatService, IUserService userService, IPropertyService propertyService)
        {
            _unitOfWork = unitOfWork;
            _chatService = chatService;
            _userService = userService;
            _propertyService = propertyService;
        }

        [Authorize]
        public async Task<IActionResult> Chat(string recipientEmail, string propertyAddress, string propertyCity, decimal? propertyPrice, int? propertyId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);  // Get logged-in user's ID

            if (string.IsNullOrWhiteSpace(userId)) { return Unauthorized(); }

            var chatData = await _chatService.GetUserConversationsDataAsync(userId);
            var propertyList = await _unitOfWork.Property.GetAllAsync(includeProperties: "ApplicationUser,Images");

            var chatVM = new ChatVM
            {
                UserId = userId,
                Conversations = chatData.Conversations,
                ApplicationUser = chatData.ApplicationUser,
                RecipientEmail = string.IsNullOrEmpty(recipientEmail) ? null : recipientEmail,
                PropertyAddress = string.IsNullOrEmpty(propertyAddress) ? null : propertyAddress,
                PropertyCity = string.IsNullOrEmpty(propertyCity) ? null : propertyCity,
                PropertyPrice = propertyPrice,
                PropertyId = propertyId,
                PropertyList = propertyList.ToList()
            };

            return View(chatVM);
        }


        #region API calls
        [HttpGet]
        public async Task<IActionResult> GetChatMessages(string conversationId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            // Validate if the user is part of the conversation
            var isParticipant = await _chatService.IsUserPartOfConversationAsync(userId, conversationId);
            if (!isParticipant) { return Forbid(); }

            // Fetch the messages for the conversation
            var messages = await _chatService.GetMessagesByConversationIdAsync(userId, conversationId);

            if (messages == null) { return NotFound(new { error = "No messages found for the conversation." }); }

            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
        {



            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);  // Get logged-in user's ID

            if (string.IsNullOrWhiteSpace(userId)) { return Unauthorized(); }

            var conversationId = await _chatService.CreateOrGetConversationIdByEmailAsync(userId, request.ReceiverEmail, request.PropertyId);

            if (conversationId == null) { return BadRequest(new { error = "Could not create or find conversation." }); }

            return Ok(new { conversationId });
        }

        public class CreateConversationRequest
        {
            public string ReceiverEmail { get; set; }
            public int? PropertyId { get; set; }
        }


        [HttpGet("check-user-email")]
        public async Task<IActionResult> CheckUserEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest(new { exists = false, error = "Email cannot be empty." });

            bool userExists = await _userService.CheckUserEmailExistsAsync(email);

            return Ok(new { exists = userExists });
        }


        [HttpGet]
        public async Task<IActionResult> GetReceiverName(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return BadRequest(new { exists = false, error = "Email cannot be empty." });

            var user = await _userService.GetUserByEmailAsync(email);
            if (user == null) return NotFound();

            return Ok(new { Name = user.Name, Email = user.Email });
        }


        [HttpGet]
        public async Task<IActionResult> GetUserPropertiesByEmail(string recipientEmail)
        {
            if (string.IsNullOrWhiteSpace(recipientEmail)) return BadRequest(new { exists = false, error = "Email cannot be empty." });

            var user = await _userService.GetUserByEmailAsync(recipientEmail);
            if (user == null) return NotFound();
            var propertiesList = await _propertyService.GetPropertiesForUserAsync(user);
            return Ok(new { properties = propertiesList });
        }

        [HttpGet("/{propertyId}")]
        public async Task<IActionResult> GetPropertyDetailsForChat(int propertyId)
        {
            Console.WriteLine($"Received propertyId: {propertyId}");
            var property = await _propertyService.GetPropertyAsync(propertyId);

            if (property == null)
            {
                return NotFound();
            }

            var imageUrl = property.Images?.FirstOrDefault()?.ImageUrl;

            // Select only the data that is needed for the chat.
            var chatPropertyInfo = new
            {
                property.Address,
                property.City,
                property.Price,
                imageUrl
            };

            return Ok(chatPropertyInfo);
        }

        #endregion
    }
}
