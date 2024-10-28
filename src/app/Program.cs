using Makspll.Pathfinder;
using Makspll.Pathfinder.Routing;
using Makspll.PathfinderApp;

Args? parsedArgs = null;
try
{
    parsedArgs = InputParser.Parse(args);

}
catch (Exception e)
{
    Environment.Exit(1);
}

Pathfinder pathfinder = new(parsedArgs.DLLGlobs, parsedArgs.Directory, parsedArgs.Config);
var output = pathfinder.Analyze();

using var writer = new StreamWriter(Console.OpenStandardOutput());

Assembly.Serialize(output, parsedArgs.OutputFormat, writer);
