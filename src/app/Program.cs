using Makspll.Pathfinder;
using Makspll.Pathfinder.Routing;
using Makspll.PathfinderApp;

Args? parsedArgs = null;
try
{
    parsedArgs = InputParser.Parse(args);
}
catch (Exception)
{
    Environment.Exit(1);
}

#if WINDOWS
ANSIConsole.ANSIInitializer.Init();
#else 
ANSIConsole.ANSIInitializer.Enabled = Environment.GetEnvironmentVariable("NO_COLOR") == null;
#endif 

if (Console.IsOutputRedirected)
{
    ANSIConsole.ANSIInitializer.Enabled = false;
}

Pathfinder pathfinder = new(parsedArgs.DLLGlobs, parsedArgs.Directory, parsedArgs.Config);

if (parsedArgs.GeneratedReportKind != null)
{
    var report = new Report(pathfinder, parsedArgs.GeneratedReportKind.Value, parsedArgs.OutputDirectory, parsedArgs.AdditionalTemplatesDir);
    report.GenerateReport();
}
else
{
    var output = pathfinder.Analyze();
    using var writer = new StreamWriter(Console.OpenStandardOutput());
    Assembly.Serialize(output, parsedArgs.OutputFormat, writer);
}
