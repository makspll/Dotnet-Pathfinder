using System.Threading.Tasks;
using System.Web.Mvc;

namespace dotnetframework472.Mvc
{
    
    [Route("attributecontrollerprefix2")]
    [Route("attributecontrollerprefix")]
    public class AttributeController : Controller
    {

        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));


        [HttpGet]
        [Route("getwithroute")]
        public Task GetWithRoute() => Task.FromResult(new HttpStatusCodeResult(200));

        [HttpGet]
        [Route("multipleroutes/1")]
        [Route("multipleroutes/2")]
        public Task MultipleRoutes() => Task.FromResult(new HttpStatusCodeResult(200));

    }
    
    public class AttributeControllerNoRoute : Controller
    {   
        [Route("attributecontrollernoroute/getWithHttpGetRoute")]
        [HttpGet] // ApiController attributes must have a route on each method if there is no route on the class (runtime error)
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));


        [Route("attributecontrollernoroute/getwithroute")]
        public Task GetWithRoute() => Task.FromResult(new HttpStatusCodeResult(200));
    }

    [Route("inheritingcontrollerprefix")]
    [Route("inheritingcontrollerprefix2")]
    public class InheritingController : ControllerBase
    {
        [HttpGet]
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));

        [Route("getwithroute")]
        public Task GetWithRoute() => Task.FromResult(new HttpStatusCodeResult(200));

        protected override void ExecuteCore()
        {
            throw new System.NotImplementedException();
        }
    }

    public class InheritingControllerNoRoute : ControllerBase
    {
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));

        [Route("InheritingControllerNoRoute/getwithroute")]
        public Task GetWithRoute() => Task.FromResult(new HttpStatusCodeResult(200));

        protected override void ExecuteCore()
        {
            throw new System.NotImplementedException();
        }
    }

    [Route("inheritingcontroller2prefix")]
    [Route("inheritingcontroller2prefix2")]
    public class InheritingController2 : Controller
    {
        [HttpGet]
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));

        [Route("getwithroute")]
        public Task GetWithRoute() => Task.FromResult(new HttpStatusCodeResult(200));

    }

    public abstract class CustomBase : Controller {}

    public class CustomBaseInheritingController : CustomBase
    {

        [HttpGet]
        [Route("custombase/route")]
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));
    }

    public class InheritingController2NoRoute : Controller
    {
        [HttpGet]
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));

        [Route("inheritingcontroller2noroute/getwithroute")]
        public Task GetWithRoute() => Task.FromResult(new HttpStatusCodeResult(200));
    }

    [Route("activeController")]
    [Route("activeController2")]
    public class ActiveControllerWithNoMethodRoute : Controller
    {
        public Task HelloWorld() => Task.FromResult(new HttpStatusCodeResult(200));
    }

    [Route("controllerComplexHttpMethods")]
    [Route("controllerComplexHttpMethods2")]
    public class ControllerComplexHttpMethods : Controller
    {
        
        [Route("HttpMethodWithSameRouteAsAnotherButDifferentMethod")]
        public Task HttpMethodWithSameRouteAsAnotherButNoMethod() => Task.FromResult(new HttpStatusCodeResult(200));
            
        [Route("HttpMethodWithSameRouteAsAnotherButDifferentMethod")]
        public Task HttpMethodWithSameRouteAsAnotherButGetMethod() => Task.FromResult(new HttpStatusCodeResult(200));
        
        [Route("HttpMethodWithSameRouteAsAnotherButDifferentMethod")]
        [HttpPost]
        public Task HttpMethodWithSameRouteAsAnotherButPostMethod() => Task.FromResult(new HttpStatusCodeResult(200));

    }

    [Route("acceptverbscontroller")]
    public class AcceptVerbsController : Controller
    {
        [Route("acceptverbsroute")]
        [AcceptVerbs("GET", "POST")]
        public Task AcceptVerbsRoute() => Task.FromResult(new HttpStatusCodeResult(200));

        [AcceptVerbs("PATCH", "DELETE")]
        public Task AcceptVerbsWithoutRoute() => Task.FromResult(new HttpStatusCodeResult(200));
    }


    [Route("controllerWithPrefixButOnlyVerbsOnMethods")]
    public class ControllerWithPrefixButOnlyVerbsOnMethods : Controller
    {
        [HttpGet]
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));

        [HttpPost]
        public Task Post() => Task.FromResult(new HttpStatusCodeResult(200));

        [HttpPut]
        public Task Put() => Task.FromResult(new HttpStatusCodeResult(200));

        [HttpDelete]
        public Task Delete() => Task.FromResult(new HttpStatusCodeResult(200));

        [HttpPatch]
        public Task Patch() => Task.FromResult(new HttpStatusCodeResult(200));

        [HttpOptions]
        public Task Options() => Task.FromResult(new HttpStatusCodeResult(200));

        [HttpHead]
        public Task Head() => Task.FromResult(new HttpStatusCodeResult(200));

    }

    public class DefaultConventionalController : Controller
    {
        [HttpGet]
        public Task DefaultAction() => Task.FromResult(new HttpStatusCodeResult(200));

        [HttpGet]
        public Task NonDefaultAction() => Task.FromResult(new HttpStatusCodeResult(200));
    }

    public class DefaultControllerConventional : Controller
    {
        [NonAction]
        [HttpGet]
        public Task DefaultAction() => Task.FromResult(new HttpStatusCodeResult(200));

        public Task NonDefaultAction() => Task.FromResult(new HttpStatusCodeResult(200));
    }

    public class ConventionalControllerOverriddenActionNames : Controller
    {
        [HttpGet]
        [ActionName("OverridenActionName")]
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));

        [HttpPost]
        [ActionName("OverridenActionName")]
        public Task Post() => Task.FromResult(new HttpStatusCodeResult(200));
    }


    public class ControllerWithNoRoutes : Controller {};


    [Route("[controller]")]
    public class ControllerWithPlaceholders : Controller
    {
        [Route("[action]")]
        [ActionName("OverridenActionName")]
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));
        
        [Route("[action]")]
        public Task Post() => Task.FromResult(new HttpStatusCodeResult(200));
    }
}