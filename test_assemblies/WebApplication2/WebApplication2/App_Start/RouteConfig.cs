using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebApplication2
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapMvcAttributeRoutes();

            routes.MapRoute(
                name: "Default",
                url: "conventionalprefix/{controller}/{action}",
                defaults: new { }
            );

            routes.MapRoute(
                name: "Default2",
                url: "conventionalprefix2/{controller}",
                defaults: new { action = "DefaultAction" }
            );

            routes.MapRoute(
                name: "Default3",
                url: "conventionalwithnoactionspecs",
                defaults: new { controller = "DefaultConventional", action = "DefaultAction" }
            );
        }
    }
}