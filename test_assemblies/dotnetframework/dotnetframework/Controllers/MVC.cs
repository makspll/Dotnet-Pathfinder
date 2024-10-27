using System.Threading.Tasks;
using System.Web.Mvc;

namespace dotnetframework472.Mvc
{

    [RoutePrefix("attributecontrollerprefix")]
    public class AttributeController : Controller
    {

        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));


        [HttpGet]
        [Route("attributegetwithroute")]
        public Task GetWithRoute() => Task.FromResult(new HttpStatusCodeResult(200));

        [HttpGet]
        [Route("multipleroutes/1")]
        [Route("multipleroutes/2")]
        public Task MultipleRoutes() => Task.FromResult(new HttpStatusCodeResult(200));

    }

    public class EmptyControllerBaseController : ControllerBase
    {
        protected override void ExecuteCore() { }
    }

    public class AttributeControllerNoRouteController : Controller
    {
        [Route("attributecontrollernoroute/getWithHttpGetRoute")]
        [HttpGet] // ApiController attributes must have a route on each method if there is no route on the class (runtime error)
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));


        [Route("attributecontrollernoroute/getwithroute")]
        public Task GetWithRoute() => Task.FromResult(new HttpStatusCodeResult(200));
    }

    [RoutePrefix("inheritingcontrollerprefix")]
    public class InheritingController : Controller
    {
        [HttpGet]
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));

        [Route("getwithroute")]
        public Task GetWithRoute() => Task.FromResult(new HttpStatusCodeResult(200));
    }

    public class InheritingControllerNoRouteController : Controller
    {
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));

        [Route("InheritingControllerNoRoute/getwithroute")]
        public Task GetWithRoute() => Task.FromResult(new HttpStatusCodeResult(200));
    }

    [RoutePrefix("inheritingcontroller2prefix")]
    public class InheritingController2Controller : Controller
    {
        [HttpGet]
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));

        [Route("getwithroute2")]
        public Task GetWithRoute() => Task.FromResult(new HttpStatusCodeResult(200));

    }

    public abstract class CustomBase : Controller { }

    public class CustomBaseInheritingController : CustomBase
    {

        [HttpGet]
        [Route("custombase/route")]
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));
    }

    public class InheritingController2NoRouteController : Controller
    {
        [HttpGet]
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));

        [Route("inheritingcontroller2noroute/getwithroute")]
        public Task GetWithRoute() => Task.FromResult(new HttpStatusCodeResult(200));
    }

    [RoutePrefix("activeController")]
    public class ActiveControllerWithNoMethodRouteController : Controller
    {
        public Task HelloWorld() => Task.FromResult(new HttpStatusCodeResult(200));
    }

    [RoutePrefix("controllerComplexHttpMethods")]
    public class ControllerComplexHttpMethodsController : Controller
    {

        [Route("HttpMethodWithSameRouteAsAnotherButDifferentMethod")]
        public Task HttpMethodWithSameRouteAsAnotherButNoMethod() => Task.FromResult(new HttpStatusCodeResult(200));

        [Route("HttpMethodWithSameRouteAsAnotherButDifferentMethod")]
        [HttpGet]
        [HttpDelete]
        public Task HttpMethodWithSameRouteAsAnotherButGetMethod() => Task.FromResult(new HttpStatusCodeResult(200));

        [Route("HttpMethodWithSameRouteAsAnotherButDifferentMethod")]
        [HttpPut]
        public Task HttpMethodWithSameRouteAsAnotherButPostMethod() => Task.FromResult(new HttpStatusCodeResult(200));

    }

    [RoutePrefix("acceptverbscontroller")]
    public class AcceptVerbsController : Controller
    {
        [Route("acceptverbsroute")]
        [AcceptVerbs("GET", "POST")]
        public Task AcceptVerbsRoute() => Task.FromResult(new HttpStatusCodeResult(200));

        [AcceptVerbs("PATCH", "DELETE")]
        public Task AcceptVerbsWithoutRoute() => Task.FromResult(new HttpStatusCodeResult(200));
    }


    [RoutePrefix("controllerWithPrefixButOnlyVerbsOnMethods")]
    public class ControllerWithPrefixButOnlyVerbsOnMethodsController : Controller
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

    public class DefaultControllerConventionalController : Controller
    {
        [NonAction]
        [HttpGet]
        public Task DefaultAction() => Task.FromResult(new HttpStatusCodeResult(200));

        public Task NonDefaultAction() => Task.FromResult(new HttpStatusCodeResult(200));
    }

    public class ConventionalControllerOverriddenActionNamesController : Controller
    {
        [HttpGet]
        [ActionName("OverridenActionName")]
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));

        [HttpPost]
        [ActionName("OverridenActionName")]
        public Task Post() => Task.FromResult(new HttpStatusCodeResult(200));
    }


    public class ControllerWithNoRoutesController : Controller { };


    [RoutePrefix("[controller]")]
    public class ControllerWithPlaceholdersController : Controller
    {
        [Route("[action]")]
        [ActionName("OverridenActionName")]
        [HttpGet]
        public Task Get() => Task.FromResult(new HttpStatusCodeResult(200));

        [Route("[action]")]
        [HttpPost]
        public Task Post() => Task.FromResult(new HttpStatusCodeResult(200));
    }

    [RoutePrefix("controllerwithverbnamedactions")]
    public class ControllerWithVerbNamedActionsController : Controller
    {
        [Route("a")]
        public string Get() => "get";

        [Route("b")]
        public string Post() => "post";

        [Route("c")]
        public string Put() => "put";

        [Route("d")]
        public string Delete() => "delete";

        [Route("e")]
        public string Patch() => "patch";

        [Route("f")]
        public string Options() => "options";

        [Route("g")]
        public string Head() => "head";
    }

    [RoutePrefix("controllerwithrouteandprefixcontrollerprefix")]
    [Route("controllerwithrouteandprefixcontrollerroute")]
    public class ControllerWithRouteAndPrefixController : Controller
    {
        [Route("withroute")]
        public string Get() => "hello";

        [HttpPost]
        public string P() => "hello";
    }

    [RoutePrefix("controllerwithprefixandemptystrings")]
    public class ControllerWithPrefixAndEmptyStringsController : Controller
    {
        [Route("")]
        public string Pos() => "post";

        [Route("")]
        [HttpDelete]
        public string Del() => "del";
    }


    public class ControllerForeignBaseController : TestUtils.ForeignMVCControllerBase
    {
        [Route("controllerforeignbaseget")]
        [HttpGet]
        public string Get() => "get";

        [HttpPost]
        public string Post() => "post";
    }


}