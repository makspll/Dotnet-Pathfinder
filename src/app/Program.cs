using ANSIConsole;
using Makspll.Pathfinder;
using Makspll.Pathfinder.Routing;
using Makspll.Pathfinder.Search;
using Makspll.PathfinderApp;
using Newtonsoft.Json;

Args? parsedArgs = null;
try
{
    parsedArgs = InputParser.Parse(args);

}
catch (Exception e)
{
    Console.WriteLine(e.Message);
    Environment.Exit(1);
}

Pathfinder pathfinder = new(parsedArgs.DLLGlobs, parsedArgs.Directory, parsedArgs.Config);
var output = pathfinder.Analyze();

using var writer = new StreamWriter(Console.OpenStandardOutput());

Assembly.Serialize(output, parsedArgs.OutputFormat, writer);
