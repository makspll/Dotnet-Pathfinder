using System.Web.Http;

namespace WebApplication2
{

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "apiconventionalprefix/{controller}/{action}",
                defaults: new { }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi2",
                routeTemplate: "apiconventionalprefix2/{controller}",
                defaults: new { action = "DefaultAction" }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultApi3",
                routeTemplate: "apiconventionalwithnoactionspecs",
                defaults: new { controller = "ApiDefaultConventionalApi", action = "DefaultAction" }
            );
        }
    }
}