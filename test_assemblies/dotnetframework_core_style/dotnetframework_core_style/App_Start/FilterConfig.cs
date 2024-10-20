using System.Web;
using System.Web.Mvc;

namespace dotnetframework_core_style
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}