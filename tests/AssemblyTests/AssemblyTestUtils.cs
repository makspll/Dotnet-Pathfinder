using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using FluentAssertions.Execution;
using Makspll.Pathfinder.Routing;
using static AssemblyTests.AssemblyTestUtils;

namespace AssemblyTests
{
    public static class AssemblyTestUtils
    {
        public static string GetTestAssembliesPath()
        {
            // find "test_assemblies" directory by crawling up the directory tree
            var currentDir = Directory.GetCurrentDirectory();

            while (currentDir != null)
            {
                var testAssembliesPath = Path.Combine(currentDir, "test_assemblies");
                if (Directory.Exists(testAssembliesPath))
                {
                    return testAssembliesPath;
                }
                currentDir = Directory.GetParent(currentDir)?.FullName;
            }

            if (currentDir == null)
            {
                throw new Exception("Could not find test_assemblies directory");
            }

            return currentDir;
        }

        public static IEnumerable<string> IterateTestAssemblyDirs()
        {
            var testAssembliesPath = GetTestAssembliesPath();
            var testAssemblies = Directory.EnumerateDirectories(testAssembliesPath);
            for (var i = 0; i < testAssemblies.Count(); i++)
            {
                if (File.Exists(Path.Combine(testAssemblies.ElementAt(i), "ignore_in_tests")))
                    continue;

                yield return testAssemblies.ElementAt(i);
            }
        }

        /**
         * Build a test assembly and put the dll in the specified directory, return the path to the dll
         */
        public static string BuildTestAssembly(string testAssemblyDir)
        {
            var testAssemblyName = Path.GetFileName(testAssemblyDir);
            var testAssemblyCsproj = Directory.EnumerateFiles(testAssemblyDir, "*.csproj").First();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"build {testAssemblyCsproj}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                // get the error message
                var errorMessage = process.StandardError.ReadToEnd();
                var outputMessage = process.StandardOutput.ReadToEnd();
                throw new Exception($"Failed to build test assembly {testAssemblyDir}: {outputMessage}{errorMessage}");
            }

            // find the dll file
            var allDlls = Directory.EnumerateFiles(testAssemblyDir, "*.dll", SearchOption.AllDirectories).ToList();
            var dllPath = allDlls.Find(f => Path.GetFileName(f) == $"{testAssemblyName}.dll" && !f.Contains("obj"));

            if (dllPath == null)
            {
                throw new Exception($"Could not find the dll file for test assembly {testAssemblyName} in {testAssemblyDir} among: {string.Join(", ", allDlls)}");
            }

            return dllPath;
        }

        public static T? WaitUntillEndpointAndCall<T>(string url)
        {
            var timeout = DateTime.Now.AddSeconds(15);
            while (true)
            {
                if (DateTime.Now > timeout)
                {
                    throw new Exception("Timeout waiting for the endpoint to start");
                }

                try
                {
                    var client = new HttpClient();
                    var response = client.GetAsync(url).Result;
                    var textOutput = response.Content.ReadAsStringAsync().Result;
                    return JsonSerializer.Deserialize<T>(textOutput);
                }
                catch
                {
                    Thread.Sleep(100);
                }
            }
        }
        public static void ExpectPortFree(int port)
        {
            using TcpClient tcpClient = new();
            try
            {
                tcpClient.Connect("127.0.0.1", port);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.ConnectionRefused)
                {
                    throw;
                }
            }
        }

        public static Process RunAssembly(string dllPath, int port = 5000, bool forwardOutput = false)
        {

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"{dllPath} --urls http://localhost:{port}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();

            // Forward stdout and stderr continuously while waiting
            if (forwardOutput)
            {
                process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
                process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }

            return process;
        }

        public static void RunAssemblyAndGetHTTPOutput<T>(string dllPath, string url, out T? httpOutput)
        {
            var process = RunAssembly(dllPath);
            // wait for the process to start and call the `api/allroutes` endpoint
            httpOutput = WaitUntillEndpointAndCall<T>(url);
            // shut down the process
            process.Kill();
            process.WaitForExit();
        }

        public static string InstantiateRoute(string expected, string expectedController, string expectedAction, string expectedArea)
        {
            if (expected.Contains('{') || expected.Contains('}'))
            {
                var parsed = ConventionalRoute.Parse(expected, null).Value;
                var instantiated = parsed.InstantiateTemplateWith(expectedController, expectedAction, expectedArea, false);
                return instantiated;
            }
            else
            {
                return "/" + expected;
            }
        }

        /// <summary>
        /// Assert that the received controller matches the expected route, for an attribute based route (non-conventional)
        /// </summary>
        public static void AssertControllerMatchedAttributeRoute(TestUtils.RouteInfo? expected, Controller? received)
        {
            var scope = AssertionScope.Current;
            if (expected == null && received != null)
            {
                if (received.Actions.Any() && !received.Actions.All(x => x.IsConventional))
                {
                    scope.FailWith($"Controller {received.ClassName} is not expected to contain any non conventional actions");
                    return;
                }
                else
                {
                    return;
                }
            }

            var controller = $"{expected?.ControllerNamespace}::{expected?.ControllerClassName}";

            if (expected != null && received == null)
            {
                scope.FailWith($"Controller ${controller} is expected to be routable");
                return;
            }

            if (expected == null && received == null)
            {
                return;
            }

            // find the method that matches the expected route
            var receivedMethod = received!.Actions.FirstOrDefault(m => m.MethodName == expected!.ActionMethodName);

            if (receivedMethod == null)
            {
                scope.FailWith($"{received.Namespace}::{received.ClassName} - {expected!.ActionMethodName} route not retrieved");
                return;
            }

            if (receivedMethod.IsConventional)
            {
                scope.FailWith($"{received.Namespace}::{received.ClassName} - {expected!.ActionMethodName} route is conventional");
                return;
            }

            // the route should match one of the expected routes
            if (!receivedMethod.Routes.Any(r => r.Path == expected!.Routes.First()))
            {
                var allRoutesString = string.Join(", ", receivedMethod.Routes.Select(r => r.Path));
                scope.FailWith($"{received.Namespace}::{received.ClassName} - '[{allRoutesString}]' was expected to contain {expected!.Routes.First()}");
            }
            else
            {
                // check http methods
                var matchingAction = receivedMethod.Routes.First(r => r.Path == expected!.Routes.First());

                var expectedHttpMethods = expected!.HttpMethods;
                var receivedHttpMethods = matchingAction.Methods.Select(m => m.ToVerbString().ToUpper()).ToList();

                foreach (var method in Enum.GetNames<HTTPMethod>())
                {
                    if (expectedHttpMethods.Contains(method.ToUpper()) && !receivedHttpMethods.Contains(method.ToUpper()))
                    {
                        scope.FailWith($"{received.Namespace}::{received.ClassName} - {expected!.Routes.First()} was expected to support HTTP method: {method}, supported methods: {string.Join(", ", receivedHttpMethods)}");
                    }
                    else if (!expectedHttpMethods.Contains(method.ToUpper()) && receivedHttpMethods.Contains(method.ToUpper()))
                    {
                        scope.FailWith($"{received.Namespace}::{received.ClassName} - {expected!.Routes.First()} was not expected to support HTTP method: {method}, supported methods: {string.Join(", ", receivedHttpMethods)}");
                    }

                }
            }
        }
    }
}