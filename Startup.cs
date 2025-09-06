using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MillenniumWebFixed.Startup))]

namespace MillenniumWebFixed
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
        }
    }
}
