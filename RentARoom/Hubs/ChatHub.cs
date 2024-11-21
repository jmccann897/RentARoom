using Microsoft.AspNetCore.SignalR;

namespace RentARoom.Hubs
{
    public class ChatHub : Hub
    {
        // broadcasts received messages to all connected users
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
