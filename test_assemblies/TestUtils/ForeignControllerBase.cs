#if NETCOREAPP || NET8_0_OR_GREATER
using Microsoft.AspNetCore.Mvc;

#elif NET472
using System.Web.Mvc;
using System.Web.Http;

#endif

namespace TestUtils
{

#if NETCOREAPP || NET8_0_OR_GREATER

    public class ForeignControllerBase : Controller
    {
    }

#elif NET472

    public class ForeignMVCControllerBase : System.Web.Mvc.Controller
    {
    }

    public class ForeignApiControllerBase : System.Web.Http.ApiController
    {
    }
    
#endif
}