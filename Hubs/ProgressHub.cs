using Microsoft.AspNet.SignalR;

namespace MillenniumWebFixed.Hubs
{
    public class ProgressHub : Hub
    {
        public static void Send(string message)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            context.Clients.All.receiveProgress(message);
        }
    }
}
