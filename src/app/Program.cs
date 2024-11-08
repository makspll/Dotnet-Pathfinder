using CommandLine;
using Makspll.Pathfinder;
using Makspll.Pathfinder.Routing;
using Makspll.PathfinderApp;

#if WINDOWS
ANSIConsole.ANSIInitializer.Init();
#else 
ANSIConsole.ANSIInitializer.Enabled = Environment.GetEnvironmentVariable("NO_COLOR") == null;
#endif 

if (Console.IsOutputRedirected)
{
    ANSIConsole.ANSIInitializer.Enabled = false;
}

var helpWriter = new HelpWriter(Console.OpenStandardOutput());

var parser = new Parser(config => config.HelpWriter = helpWriter);

var exitCode = parser.ParseArguments<AnalyzeOptions, ReportOptions>(args).MapResult(
    (AnalyzeOptions a) =>
    {
        var pathfinder = new Pathfinder(a.DLLGlobs, a.Directory, a.Config);
        var output = pathfinder.Analyze();
        using var writer = new StreamWriter(Console.OpenStandardOutput());
        Assembly.Serialize(output, a.OutputFormat, writer);
        return 0;
    },
    (ReportOptions r) =>
    {
        var pathfinder = new Pathfinder(r.DLLGlobs, r.Directory, r.Config);
        var report = new Report(pathfinder, r.GeneratedReportKind, r.OutputDirectory, r.AdditionalTemplatesDir);
        report.GenerateReport();
        return 0;
    }, _ => 1
);

helpWriter.Flush();
return exitCode;