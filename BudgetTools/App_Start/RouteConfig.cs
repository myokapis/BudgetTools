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
                url: "{controller}/{action}/{id}",
                namespaces: new string[] { "BudgetTools.Controllers" },
                defaults: new { controller = "Transactions", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
