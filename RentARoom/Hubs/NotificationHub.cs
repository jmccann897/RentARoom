using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using NuGet.Packaging.Signing;
using RentARoom.DataAccess.Data;
using RentARoom.DataAccess.Repository.IRepository;
using RentARoom.DataAccess.Services.IServices;

namespace RentARoom.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ChatHub> _logger;
        private readonly IChatService _chatService;

        public NotificationHub(ApplicationDbContext db, ILogger<ChatHub> logger, IChatService chatService)
        {
            _db = db;
            _logger = logger;
            _chatService = chatService;
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Within the onconnectedAsync in notHub");
            var userId = Context.UserIdentifier;
            Console.WriteLine($"User {userId} connected to NotificationHub");
            await base.OnConnectedAsync();
        }


        public async Task SendNewChatMessageNotification(string userId, string message, string conversationId)
        {
            // Send notification to the specific user
            await Clients.User(userId).SendAsync("ReceiveChatMessageNotification", message, conversationId);
        }

       
    }
}
