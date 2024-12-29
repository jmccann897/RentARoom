using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using RentARoom.DataAccess.Data;
using RentARoom.DataAccess.Repository.IRepository;

namespace RentARoom.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;
       

        public ChatHub(ApplicationDbContext db)
        {
            _db = db;
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
            // LINQ to find receiver from db table and return their id
            var userId = _db.ApplicationUsers.FirstOrDefault(u => u.Email.ToLower() == receiver.ToLower()).Id;

            // Null check on receiver
            if (!string.IsNullOrEmpty(userId))
            {
                // Single message send based on receiver Id
                await Clients.User(userId).SendAsync("MessageReceived", sender, message);
            }
        }
    }
}
