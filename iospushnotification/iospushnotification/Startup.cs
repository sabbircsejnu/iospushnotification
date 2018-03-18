using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(iospushnotification.Startup))]
namespace iospushnotification
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            
        }
    }
}
