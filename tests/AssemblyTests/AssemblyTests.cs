using FluentAssertions;
using static AssemblyTests.AssemblyTestUtils;
using FluentAssertions.Execution;
using Xunit.Abstractions;
using System.Text.Json;
using Makspll.Pathfinder.Search;
using Makspll.Pathfinder.Serialization;
using TestUtils;

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

                // test we're not running on the port
                ExpectPortFree(5000);

                var query = new AssemblyQuery(dllPath);
                var controllersMeta = query.FindControllers();
                using var process = RunAssembly(dllPath);
                try
                {
                    var httpOutput = WaitUntillEndpointAndCall<RouteInfo[]>("http://localhost:5000/api/allroutes");

                    var serializationOptions = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Converters = { new RoutingAttributeConverter() }
                    };

                    // OutputHelper.WriteLine($"Metadata under test:\n{JsonSerializer.Serialize(controllersMeta, serializationOptions)}");
                    // OutputHelper.WriteLine($"HTTP output expected:\n{JsonSerializer.Serialize(httpOutput, serializationOptions)}");
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
                                    var matchingRouteActions = httpOutput.Where(r => r.Route == route.Path);
                                    if (!matchingRouteActions.Any())
                                    {
                                        AssertionScope.Current.FailWith($"{controller.Namespace}::{controller.Name} - {route.Path} was not expected to be routable");
                                    }
                                }

                            }
                        }
                    }

                    // count for good measure
                    var nonConventionalRoutes = httpOutput!.Count(r => !r.ConventionalRoute);
                    controllersMeta.Sum(c => c.Actions.Sum(a => a.Routes.Count())).Should().Be(nonConventionalRoutes, "All non-conventional routes should be in the metadata");


                    // check http methods against live responses

                    foreach (var controller in controllersMeta)
                    {
                        foreach (var action in controller.Actions)
                        {
                            var actionsWithSameRoute = controller.Actions.Where(x => x != action && x.Routes.Any()).GroupBy(a => a.Routes.First().Path).Where(g => g.Count() > 1);
                            var otherMethods = actionsWithSameRoute.SelectMany(g => g.SelectMany(v => v.Routes).SelectMany(x => x.Methods)).ToList();
                            AssertActionAllowedMethods(controller.Name, action, "http://localhost:5000", otherMethods);
                        }
                    }
                }
                finally
                {
                    process.Kill();
                    process.WaitForExit();
                }
            }
        }
    }
}