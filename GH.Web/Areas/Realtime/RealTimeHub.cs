using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace GH.Web.Areas.Realtime
{
    [HubName("realtime")]
    public class RealTimeHub : Hub
    {
        public void Hello()
        {
            Clients.All.hello();
        }
    }
}