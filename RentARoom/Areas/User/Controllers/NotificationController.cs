using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RentARoom.DataAccess.Data;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.DataAccess.Services.IServices;
using RentARoom.Models;
using RentARoom.Models.DTOs;
using RentARoom.Models.ViewModels;
using System.Security.Claims;

namespace RentARoom.Areas.User.Controllers
{
    [Area("User")]
    public class NotificationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IChatService _chatService;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(IUnitOfWork unitOfWork, IChatService chatService, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _chatService = chatService;
            _db = db;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            
            return View();
        }
        public async Task<IActionResult> Chat(string recipientEmail, string propertyAddress, string propertyCity, decimal propertyPrice)
        {
            ViewBag.RecipientEmail = string.IsNullOrEmpty(recipientEmail) ? string.Empty : recipientEmail;
            ViewBag.PropertyAddress = string.IsNullOrEmpty(propertyAddress) ? string.Empty : propertyAddress;
            ViewBag.PropertyCity = string.IsNullOrEmpty(propertyCity) ? string.Empty : propertyCity;
            if (!propertyPrice.Equals(null))
            {
                ViewBag.PropertyPrice = propertyPrice;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);  // Get logged-in user's ID

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            // Fetch the user's conversation IDs
            var conversationIds = await _chatService.GetUserConversationIdsAsync(userId);

            // Fetch the users conversations
            var conversations = await _chatService.GetUserConversationsAsync(userId);

            // Convert to DTOs for view
            var conversationsDTO = await _chatService.GetUserExistingConversationsAsync(userId);

            // Fetch the user
            var applicationUser = await _userManager.GetUserAsync(User);


            var chatVM = new ChatVM
            {
                UserId = userId,
                ConversationIds = conversationIds,
                Conversations = conversationsDTO.ToList(),
                ApplicationUser = applicationUser
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
            if (!isParticipant)
            {
                return Forbid();
            }

            // Fetch the messages for the conversation
            var messages = await _chatService.GetMessagesByConversationIdAsync(userId, conversationId);

            if (messages == null)
            {
                return NotFound(new { error = "No messages found for the conversation." });
            }

            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);  // Get logged-in user's ID

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            // Resolve recipient Id from recipient email
            var recipient = await _db.ApplicationUsers
                .FirstOrDefaultAsync(u => u.Email == request.ReceiverEmail);

            if (recipient == null)
            {
                return BadRequest(new { error = "Recipient not found." });
            }

            var recipientId = recipient.Id;

            // Call your service to create or get the conversation ID
            var conversationId = await _chatService.CreateOrGetConversationIdAsync(userId, recipientId);

            if (conversationId == null)
            {
                return BadRequest(new { error = "Could not create or find conversation." });
            }

            return Ok(new { conversationId = conversationId });
        }

        public class CreateConversationRequest
        {
            public string ReceiverEmail { get; set; }
        }


        [HttpGet("check-user-email")]
        public async Task<IActionResult> CheckUserEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { exists = false, error = "Email cannot be empty." });

            bool userExists = await _userManager.Users.AnyAsync(u => u.Email == email);

            return Ok(new { exists = userExists });
        }

        #endregion
    }
}
