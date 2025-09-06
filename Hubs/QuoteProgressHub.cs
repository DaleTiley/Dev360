using Microsoft.AspNet.SignalR;

namespace MillenniumWebFixed.Hubs
{
    public class QuoteProgressHub : Hub
    {
        public static void Send(string message)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<QuoteProgressHub>();
            context.Clients.All.receiveQuoteProgress(message);
        }
    }
}