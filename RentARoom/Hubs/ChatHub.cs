using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RentARoom.DataAccess.Data;
using RentARoom.DataAccess.Repository.IRepository;

namespace RentARoom.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ApplicationDbContext db, ILogger<ChatHub> logger)
        {
            _db = db;
            _logger = logger;
        }

        // broadcasts received messages to all connected users
        public async Task SendMessageToAll(string user, string message)
        {
            await Clients.All.SendAsync("MessageReceived", user, message);
        }
        // Default authorisation based on if email has been confirmed by aspnet Identity
        [Authorize]
        // Sends message to specified receiver
        public async Task SendMessageToReceiver(string sender,string receiver, string message)
        {
            // LINQ to find the receiver's user ID
            var user = _db.ApplicationUsers.FirstOrDefault(u => u.Email.ToLower() == receiver.ToLower());

            if (user == null)
            {
                // Handle case where receiver is not found
                throw new HubException("The specified receiver does not exist.");
            }

            var userId = user.Id;

            // Log sender and recipient details
            _logger.LogInformation("Sending message from {Sender} to {Receiver}.", sender, receiver);

            // Send message to the specified receiver
            await Clients.User(userId).SendAsync("MessageReceived", sender, message, receiver);
        }
    }
}
