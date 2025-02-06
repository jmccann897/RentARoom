using Microsoft.AspNetCore.SignalR;

namespace RentARoom.Hubs
{
    public class PropertyViewHub : Hub
    {
        public async Task NotifyView(int propertyId)
        {
            await Clients.All.SendAsync("ReceiveViewUpdate", propertyId);
        }

        public async Task SendViewUpdate(int propertyId, int newViewCount)
        {
            newViewCount++;

            await Clients.All.SendAsync("UpdateViewCount", propertyId, newViewCount);
        }
    }
}
