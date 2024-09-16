using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using FluentAssertions.Execution;
using Makspll.ReflectionUtils;
using Makspll.ReflectionUtils.Routing;
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


        // public record RouteInfo
        // {
        //     [JsonPropertyName("method")]
        //     public string? Method { get; set; }
        //     [JsonPropertyName("routes")]
        //     public required string Route { get; set; }
        //     [JsonPropertyName("action")]
        //     public string? Action { get; set; }
        //     [JsonPropertyName("controllerMethod")]
        //     public string? ControllerMethod { get; set; }

        //     [JsonPropertyName("expectedRoutes")]
        //     public string? ExpectedRoute { get; set; }

        //     [JsonPropertyName("expectNoRoute")]
        //     public bool ExpectNoRoute { get; set; }

        //     [JsonPropertyName("conventionalRoute")]
        //     public bool ConventionalRoute { get; set; }
        // }
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

        public static void RunAssemblyAndGetHTTPOutput<T>(string dllPath, string url, out T? httpOutput)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"{dllPath}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            process.Start();

            // Forward stdout and stderr continuously while waiting
            process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            // wait for the process to start and call the `api/allroutes` endpoint
            httpOutput = WaitUntillEndpointAndCall<T>(url);
            // shut down the process
            process.Kill();
            process.WaitForExit();
        }

        public static void AssertControllerMatchedReflectionMetadata(TestUtils.RouteInfo? expected, Controller? received)
        {
            var scope = AssertionScope.Current;
            if (expected == null && received != null)
            {
                scope.FailWith($"Controller {received.Name} is not expected to be routable");
                return;
            }

            var controller = expected?.ControllerMethod?.Split(":")[0] ?? expected?.Action;

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
            var receivedMethod = received!.Actions.FirstOrDefault(m => m.Name == expected!.ControllerMethod?.Split(":")[1]);

            if (receivedMethod == null)
            {
                scope.FailWith($"{received.Namespace}::{received.Name} - {expected!.ControllerMethod} route not retrieved");
                return;
            }

            // the route should match one of the expected routes
            if (!receivedMethod.Routes.Contains(expected!.Route))
            {
                scope.FailWith($"{received.Namespace}::{received.Name} - {receivedMethod.Routes} was expected to contain {expected!.Route}");
            }


        }

    }
}