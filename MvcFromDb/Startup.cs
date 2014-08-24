using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MvcFromDb.Startup))]
namespace MvcFromDb
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
