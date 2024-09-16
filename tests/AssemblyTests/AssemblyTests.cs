using Makspll.ReflectionUtils;
using FluentAssertions;
using static AssemblyTests.AssemblyTestUtils;
using FluentAssertions.Execution;
using Xunit.Abstractions;
using System.Text.Json;
using Makspll.ReflectionUtils.Search;
using Makspll.ReflectionUtils.Serialization;

namespace AssemblyTests
{
    public class AssemblyTests
    {
        // Pass ITestOutputHelper into the test class, which xunit provides per-test
        public AssemblyTests(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        private ITestOutputHelper OutputHelper { get; }

        [Fact]
        public void TestAssembly()
        {
            foreach (var testAssemblyDir in IterateTestAssemblyDirs())
            {
                var dllPath = BuildTestAssembly(testAssemblyDir);
                var testAssemblyName = Path.GetFileName(testAssemblyDir);
                OutputHelper.WriteLine($"\n\n --- Testing assembly: {testAssemblyName} at {dllPath}");

                var query = new AssemblyQuery(dllPath);
                var controllersMeta = query.FindControllers();
                RunAssemblyAndGetHTTPOutput(dllPath, "http://localhost:5000/api/allroutes", out TestUtils.RouteInfo[]? httpOutput);
                var serializationOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new RoutingAttributeConverter() }
                };

                OutputHelper.WriteLine($"Metadata under test:\n{JsonSerializer.Serialize(controllersMeta, serializationOptions)}");
                OutputHelper.WriteLine($"HTTP output expected:\n{JsonSerializer.Serialize(httpOutput, serializationOptions)}");
                Assert.NotNull(httpOutput);

                using var scope = new AssertionScope();



                // what should be there
                foreach (var route in httpOutput)
                {
                    // ignore conventional routes for now
                    if (route.ControllerMethod == null || route.ConventionalRoute)
                        continue;


                    var matchingController = controllersMeta.FirstOrDefault(c =>
                        c.Name == route.ControllerMethod?.Split(":")[0].Split(".").Last()
                    // || c.Actions.Any(m => m.Route == route.Route)
                    );

                    route.ExpectNoRoute.Should().Be(false, $"{route.Route} was marked with `ExpectNoRoute`");

                    if (route.ExpectedRoute != null)
                    {
                        route.Route.Should().Be(route.ExpectedRoute, $"Controller action {route.ControllerMethod ?? route.Method} was marked with `ExpectRoute` attribute");
                    }

                    AssertControllerMatchedReflectionMetadata(route, matchingController);
                }

                // what should not be there
                foreach (var controller in controllersMeta)
                {
                    var matchingRoute = httpOutput.FirstOrDefault(r => r.ControllerMethod?.Split(":")[0].Split(".").Last() == controller.Name);
                    if (matchingRoute == null)
                    {
                        AssertControllerMatchedReflectionMetadata(null, controller);
                    }
                    else
                    {
                        // check individual routes
                        foreach (var action in controller.Actions)
                        {

                            foreach (var route in action.Routes)
                            {
                                var matchingRouteActions = httpOutput.Where(r => r.Route == route);
                                if (!matchingRouteActions.Any())
                                {
                                    AssertionScope.Current.FailWith($"{controller.Namespace}::{controller.Name} - {route} was not expected to be routable");
                                }
                            }

                        }
                    }
                }

                // count for good measure
                var nonConventionalRoutes = httpOutput!.Count(r => !r.ConventionalRoute);
                controllersMeta.Sum(c => c.Actions.Sum(a => a.Routes.Count())).Should().Be(nonConventionalRoutes, "All non-conventional routes should be in the metadata");

            }
        }
    }
}