using Microsoft.AspNetCore.SignalR;

namespace Capstone.Api.Module.Statuses.SignalR
{
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            try
            {
                Console.WriteLine("Connnect success");
                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Connnect Fail" + ex.Message);
            }
        }

        public async Task JoinGroup(string groupId)
        {
            Console.WriteLine("Join group success success");
            await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
        }

        public async Task NotificationRequest(string groupId)
        {
            try
            {
                await Clients.Group(groupId).SendAsync("NotificationResponse", "Success");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
