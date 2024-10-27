using FluentAssertions;
using static AssemblyTests.AssemblyTestUtils;
using FluentAssertions.Execution;
using Xunit.Abstractions;
using System.Text.Json;
using Makspll.Pathfinder.Search;
using TestUtils;
using System.Diagnostics;
using Makspll.Pathfinder.Routing;
using System.Collections.Immutable;
using System.Text.RegularExpressions;


namespace AssemblyTests
{
    public partial class AssemblyTests : IDisposable
    {
        // Pass ITestOutputHelper into the test class, which xunit provides per-test
        public AssemblyTests(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        private ITestOutputHelper OutputHelper { get; }

        private Process? process = null;

        public void Dispose()
        {
            if (process != null)
            {
                try
                {
                    process.Kill(true);
                    process.WaitForExit(TimeSpan.FromSeconds(10));
                    process = null;
                }
                catch (Exception e)
                {
                    OutputHelper.WriteLine($"Failed to kill process: {e.Message}");
                }
            }
        }


        public static IEnumerable<object[]> AllTestAssemblyDirs()
        {
            foreach (var dir in IterateTestAssemblyDirs())
            {
                var dirname = Path.GetFileName(dir) ?? throw new Exception("Failed to get directory name");

                if (Environment.GetEnvironmentVariable("TEST_ASSEMBLY") is string testAssembly && testAssembly != dirname)
                    continue;

                yield return new object[] { dirname };
            }
        }

        public AssemblyQuery PrepareAssembly(string testAssemblyName)
        {
            var forwardOutput = Environment.GetEnvironmentVariable("FORWARD_OUTPUT") != null;
            if (process != null)
            {
                throw new Exception("Process is already running");
            }

            var testAssemblyDir = Path.Combine(GetTestAssembliesPath(), testAssemblyName);
            ExpectPortFree(5000);
            var dllPath = BuildTestAssembly(testAssemblyDir, forwardOutput: forwardOutput);
            var configFile = Path.Combine(testAssemblyDir, "pathfinder.json");
            var query = new AssemblyQuery(dllPath, AssemblyQuery.ParseConfig(new FileInfo(configFile)));
            process = RunTestAssembly(testAssemblyDir, forwardOutput: forwardOutput);
            var _ = WaitUntillEndpointAndCall<RouteInfo[]>("http://localhost:5000/api/attributeroutes") ?? throw new Exception("Failed to get route info");
            return query;
        }

        private static void RemoveRoutesWithPlaceholders(List<RouteInfo> routes)
        {
            var regex = PlaceholderRegex();
            foreach (var route in routes)
            {
                var toDelete = route.Routes.Where(r => regex.Match(r).Success).ToList();
                if (toDelete.Count == 0)
                    continue;
                route.Routes = route.Routes.Except(toDelete);
            }

            routes.RemoveAll(r => !r.Routes.Any());
        }

        [Theory]
        [MemberData(nameof(AllTestAssemblyDirs))]
        public void TestAttributeRoutes(string testAssemblyDir)
        {
            var query = PrepareAssembly(testAssemblyDir);
            var controllersMeta = query.FindAllControllers();

            var serializationOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
            };

            using var scope = new AssertionScope();
            var attributeRoutes = WaitUntillEndpointAndCall<RouteInfo[]>("http://localhost:5000/api/attributeroutes")?.ToList() ?? throw new Exception("Failed to get route info");

            // if any controller or action placeholders are present in the rout info, we leave those for the other tests
            RemoveRoutesWithPlaceholders(attributeRoutes);

            // what should be there
            foreach (var route in attributeRoutes)
            {

                var matchingController = controllersMeta.FirstOrDefault(c =>
                    c.ClassName == route.ControllerClassName
                );

                AssertControllerMatchedAttributeRoute(route, matchingController);
            }

            // what should not be there
            foreach (var controller in controllersMeta)
            {
                // will be tested otherwise
                // var matchingRoute = attributeRoutes.FirstOrDefault(r => r.ControllerClassName == controller.ClassName);
                // if (matchingRoute == null)
                // {
                //     AssertControllerMatchedAttributeRoute(null, controller);
                // }
                // else
                // {
                //     // check individual routes
                //     foreach (var action in controller.Actions)
                //     {
                //         // skip conventional routes for now, we will run another test later
                //         if (action.IsConventional)
                //             continue;

                //         foreach (var route in action.Routes)
                //         {
                //             var matchingRouteActions = attributeRoutes.Where(r => r.Routes.FirstOrDefault(x => x == route.Path) != null);
                //             if (!matchingRouteActions.Any())
                //             {
                //                 AssertionScope.Current.FailWith($"{controller.Namespace}::{controller.ClassName} - {route.Path} was not expected to be routable");
                //             }
                //         }

                //     }
                // }
            }

            // // count for good measure
            // var uniqueExpectedNonConventionalRoutes = attributeRoutes.SelectMany(x => x.Routes).Distinct().ToList();
            // var uniqueActualNonConventionalRoutes = controllersMeta.SelectMany(c => c.Actions).Where(a => !a.IsConventional).SelectMany(a => a.Routes).Select(x => x.Path).Distinct().ToList();
            // uniqueActualNonConventionalRoutes.Sort();
            // uniqueExpectedNonConventionalRoutes.Sort();
            // uniqueActualNonConventionalRoutes.Count().Should().Be(uniqueExpectedNonConventionalRoutes.Count(), "All non-conventional routes should be in the metadata");
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
                        var failedMethods = new List<HTTPMethod>();
                        foreach (var method in Enum.GetValues<HTTPMethod>())
                        {
                            var request = new HttpRequestMessage(method.ToHttpMethod(), $"http://localhost:5000{route.Path}");
                            try
                            {
                                var response = client.Send(request);
                                var expectSuccess = route.Methods.Contains(method);

                                if (!response.IsSuccessStatusCode && expectSuccess)
                                {
                                    failedMethods.Add(method);
                                }
                            }
                            catch
                            {
                                failedMethods.Add(method);
                            }
                        }
                        if (failedMethods.Count != 0)
                        {
                            var methods = string.Join(", ", failedMethods.Select(m => m.ToString()));
                            AssertionScope.Current.FailWith($"Failed to access {controller.Namespace}::{controller.ClassName}::{action.MethodName} - {route.Path} with [{methods}]");
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

        [GeneratedRegex(@"{controller.*?}|{action.*?}")]
        private static partial Regex PlaceholderRegex();
    }
}