using System.Web.Mvc;
using System.Web.Routing;

namespace BudgetTools
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "Angular/{controller}/{action}/{id}",
                namespaces: new string[] { "BudgetTools.Controllers.Angular" },
                // TODO: reset this to the transaction page when done developing
                defaults: new { controller = "Import", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
