using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BudgetTools.Startup))]
namespace BudgetTools
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
        }
    }
}
