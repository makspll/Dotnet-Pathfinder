using FluentAssertions;
using static AssemblyTests.AssemblyTestUtils;
using FluentAssertions.Execution;
using Xunit.Abstractions;
using System.Text.Json;
using Makspll.Pathfinder.Search;
using Makspll.Pathfinder.Serialization;
using TestUtils;
using System.Diagnostics;
using Makspll.Pathfinder.Routing;


namespace AssemblyTests
{
    public class AssemblyTests : IDisposable
    {
        // Pass ITestOutputHelper into the test class, which xunit provides per-test
        public AssemblyTests(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        private ITestOutputHelper OutputHelper { get; }

        private Process? process;

        public void Dispose()
        {
            if (process != null)
            {
                process.Kill();
                process.WaitForExit();
            }
        }

        public static IEnumerable<object[]> AllTestAssemblyDirs()
        {
            foreach (var dir in IterateTestAssemblyDirs())
            {
                yield return new object[] { dir };
            }
        }

        public AssemblyQuery PrepareAssembly(string testAssemblyDir)
        {
            ExpectPortFree(5000);
            var dllPath = BuildTestAssembly(testAssemblyDir);
            var configFile = Path.Combine(testAssemblyDir, "pathfinder.json");
            var query = new AssemblyQuery(dllPath, AssemblyQuery.ParseConfig(new FileInfo(configFile)) ?? []);
            process = RunAssembly(dllPath);
            var _ = WaitUntillEndpointAndCall<RouteInfo[]>("http://localhost:5000/api/attributeroutes") ?? throw new Exception("Failed to get route info");
            return query;
        }


        [Theory]
        [MemberData(nameof(AllTestAssemblyDirs))]
        public void TestNonConventionalRoutes(string testAssemblyDir)
        {
            var query = PrepareAssembly(testAssemblyDir);
            var controllersMeta = query.FindAllControllers();

            var serializationOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new RoutingAttributeConverter() }
            };

            using var scope = new AssertionScope();
            var attributeRoutes = WaitUntillEndpointAndCall<RouteInfo[]>("http://localhost:5000/api/attributeroutes") ?? throw new Exception("Failed to get route info");

            // what should be there
            foreach (var route in attributeRoutes)
            {

                var matchingController = controllersMeta.FirstOrDefault(c =>
                    c.ClassName == route.ControllerClassName
                );

                route.ExpectNoRoute.Should().Be(false, $"{route.Routes.First()} was marked with `ExpectNoRoute`");

                if (route.ExpectedRoute != null)
                {
                    route.Routes.First().Should().Be(route.ExpectedRoute, $"Controller action {route.Action} was marked with `ExpectRoute` attribute");
                }

                AssertControllerMatchedAttributeRoute(route, matchingController);
            }

            // what should not be there
            foreach (var controller in controllersMeta)
            {
                var matchingRoute = attributeRoutes.FirstOrDefault(r => r.ControllerClassName == controller.ClassName);
                if (matchingRoute == null)
                {
                    AssertControllerMatchedAttributeRoute(null, controller);
                }
                else
                {
                    // check individual routes
                    foreach (var action in controller.Actions)
                    {
                        // skip conventional routes for now, we will run another test later
                        if (action.IsConventional)
                            continue;

                        foreach (var route in action.Routes)
                        {
                            var matchingRouteActions = attributeRoutes.Where(r => r.Routes.First() == route.Path);
                            if (!matchingRouteActions.Any())
                            {
                                AssertionScope.Current.FailWith($"{controller.Namespace}::{controller.ClassName} - {route.Path} was not expected to be routable");
                            }
                        }

                    }
                }
            }

            // count for good measure
            var expectedNonConventionalRoutes = attributeRoutes!.Length;
            var actualNonConventionalRoutes = controllersMeta.Sum(c => c.Actions.Where(x => !x.IsConventional).Sum(a => a.Routes.Count()));
            actualNonConventionalRoutes.Should().Be(expectedNonConventionalRoutes, "All non-conventional routes should be in the metadata");

        }


        [Theory]
        [MemberData(nameof(AllTestAssemblyDirs))]
        public void TestAllReturnedRoutesAndMethodsAreAccessible(string testAssemblyDir)
        {
            var query = PrepareAssembly(testAssemblyDir);
            var controllersMeta = query.FindAllControllers();
            using var scope = new AssertionScope();

            var client = new HttpClient();

            foreach (var controller in controllersMeta)
            {
                foreach (var action in controller.Actions)
                {
                    foreach (var route in action.Routes)
                    {
                        foreach (var method in Enum.GetValues<HTTPMethod>())
                        {
                            var request = new HttpRequestMessage(method.ToHttpMethod(), $"http://localhost:5000{route.Path}");
                            try
                            {
                                var response = client.Send(request);
                                var expectSuccess = route.Methods.Contains(method);
                                if (!response.IsSuccessStatusCode && expectSuccess)
                                {
                                    AssertionScope.Current.FailWith($"Failed to access {controller.Namespace}::{controller.ClassName} - {route.Path} with {method}");
                                }
                            }
                            catch (Exception e)
                            {
                                AssertionScope.Current.FailWith($"Failed to access {controller.Namespace}::{controller.ClassName} - {route.Path} with {method} - {e.Message}");
                            }
                        }
                    }
                }
            }
        }

        [Theory]
        [MemberData(nameof(AllTestAssemblyDirs))]
        public void TestConventionalRouting(string testAssemblyDir)
        {
            var query = PrepareAssembly(testAssemblyDir);
            var controllersMeta = query.FindAllControllers();
            using var scope = new AssertionScope();

            var conventionalRoutes = WaitUntillEndpointAndCall<RouteInfo[]>("http://localhost:5000/api/conventionalroutes") ?? throw new Exception("Failed to get route info");

            // for conventional routes we check that the actions are marked as conventional
            // all routes when instantiate must match the returned routes

            foreach (var route in conventionalRoutes)
            {
                var matchingController = controllersMeta.FirstOrDefault(c =>
                    c.ClassName == route.ControllerClassName
                );

                if (matchingController == null)
                {
                    AssertionScope.Current.FailWith($"Controller {route.ControllerClassName} was not found in the metadata");
                    continue;
                }

                var matchingAction = matchingController.Actions.FirstOrDefault(a => a.MethodName == route.ActionMethodName);

                if (matchingAction == null)
                {
                    AssertionScope.Current.FailWith($"Action {route.ActionMethodName} was not found in the metadata for {route.ControllerClassName}");
                    continue;
                }

                if (!matchingAction.IsConventional)
                {
                    AssertionScope.Current.FailWith($"Action {route.ActionMethodName} for {route.ControllerClassName} was not marked as conventional");
                }
                // TODO: area
                foreach (var conventionalRoute in route.Routes)
                {
                    var instantiatedRoute = InstantiateRoute(conventionalRoute, route.ControllerName!, route.Action!, "{area}");
                    if (!matchingAction.Routes.Any(r => r.Path == instantiatedRoute))
                    {
                        AssertionScope.Current.FailWith($"Route {instantiatedRoute} was not found in the metadata for {route.ControllerClassName}::{route.ActionMethodName}. Found routes: {string.Join(", ", matchingAction.Routes.Select(r => r.Path))}");
                    }
                }

            }
        }

        [Theory]
        [MemberData(nameof(AllTestAssemblyDirs))]
        public void TestNoDuplicates(string testAssembly)
        {
            var query = PrepareAssembly(testAssembly);
            var controllersMeta = query.FindAllControllers();
            using var scope = new AssertionScope();

            var allControllers = controllersMeta.Select(c => $"{c.Namespace}{c.ClassName}").ToList();
            var duplicateControllers = allControllers.GroupBy(c => c).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

            duplicateControllers.Should().BeEmpty("No duplicate controllers should be present");

            foreach (var controller in controllersMeta)
            {
                var allActions = controller.Actions.Select(a => a.MethodName).ToList();
                var duplicateActions = allActions.GroupBy(a => a).Where(g => g.Count() > 1).Select(g => g.Key).ToList();

                duplicateActions.Should().BeEmpty($"No duplicate actions should be present in {controller.Namespace}::{controller.ClassName}");
            }

        }
    }
}