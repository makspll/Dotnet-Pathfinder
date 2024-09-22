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
            var query = new AssemblyQuery(dllPath);
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
                // ignore conventional routes for now
                if (route.ControllerMethod == null || route.ConventionalRoute)
                    continue;

                var matchingController = controllersMeta.FirstOrDefault(c =>
                    c.Name == route.ControllerMethod?.Split(":")[0].Split(".").Last()
                );

                route.ExpectNoRoute.Should().Be(false, $"{route.Route} was marked with `ExpectNoRoute`");

                if (route.ExpectedRoute != null)
                {
                    route.Route.Should().Be(route.ExpectedRoute, $"Controller action {route.ControllerMethod} was marked with `ExpectRoute` attribute");
                }

                AssertControllerMatchedReflectionMetadata(route, matchingController);
            }

            // what should not be there
            foreach (var controller in controllersMeta)
            {
                var matchingRoute = attributeRoutes.FirstOrDefault(r => r.ControllerMethod?.Split(":")[0].Split(".").Last() == controller.Name);
                if (matchingRoute == null)
                {
                    AssertControllerMatchedReflectionMetadata(null, controller);
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
                            var matchingRouteActions = attributeRoutes.Where(r => r.Route == route.Path);
                            if (!matchingRouteActions.Any())
                            {
                                AssertionScope.Current.FailWith($"{controller.Namespace}::{controller.Name} - {route.Path} was not expected to be routable");
                            }
                        }

                    }
                }
            }

            // count for good measure
            var expectedNonConventionalRoutes = attributeRoutes!.Count(r => !r.ConventionalRoute);
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
                                    AssertionScope.Current.FailWith($"Failed to access {controller.Namespace}::{controller.Name} - {route.Path} with {method}");
                                }
                            }
                            catch (Exception e)
                            {
                                AssertionScope.Current.FailWith($"Failed to access {controller.Namespace}::{controller.Name} - {route.Path} with {method} - {e.Message}");
                            }
                        }
                    }
                }
            }
        }
    }
}