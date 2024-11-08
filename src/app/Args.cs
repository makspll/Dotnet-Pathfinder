using System;
using CommandLine;
using Makspll.Pathfinder;
using Makspll.Pathfinder.Reports;

namespace Makspll.PathfinderApp;



class SharedOptions
{
    [Value(0, MetaName = "DLLGlobs", Required = true, HelpText = "Glob patterns for DLLs to analyze (e.g. *.dll or **/*.dll)")]
    public required IEnumerable<string> DLLGlobs { get; set; }

    [Option('d', "dll-dir", Default = ".", HelpText = "Root directory to search for DLLs", MetaValue = "<directory>")]
    public required string Directory { get; set; }

    [Option('c', "config", Default = "pathfinder.json", HelpText = "Path to config file. Should be able to find it automatically.", MetaValue = "<config file>")]
    public required string Config { get; set; }
}

[Verb("analyze", HelpText = "Analyze DLLs for routing information and output JSON or text")]
class AnalyzeOptions
{
    [Value(0, MetaName = "DLLGlobs", Required = true, HelpText = "Glob patterns for DLLs to analyze (e.g. *.dll or **/*.dll)")]
    public required IEnumerable<string> DLLGlobs { get; set; }

    [Option('d', "dll-dir", Default = ".", HelpText = "Root directory to search for DLLs", MetaValue = "<directory>")]
    public required string Directory { get; set; }

    [Option('c', "config", Default = "pathfinder.json", HelpText = "Path to config file. Should be able to find it automatically.", MetaValue = "<config file>")]
    public required string Config { get; set; }

    [Option('f', "format", Default = OutputFormat.JSON, HelpText = "Output format", MetaValue = "<JSON|Text>")]
    public required OutputFormat OutputFormat { get; set; }

}

[Verb("report", HelpText = "Analyze and also produce a report")]
class ReportOptions
{
    [Value(0, MetaName = "DLLGlobs", Required = true, HelpText = "Glob patterns for DLLs to analyze (e.g. *.dll or **/*.dll)")]
    public required IEnumerable<string> DLLGlobs { get; set; }

    [Option('d', "dll-dir", Default = ".", HelpText = "Root directory to search for DLLs", MetaValue = "<directory>")]
    public required string Directory { get; set; }

    [Option('c', "config", Default = "pathfinder.json", HelpText = "Path to config file. Should be able to find it automatically.", MetaValue = "<config file>")]
    public required string Config { get; set; }

    [Option('k', "kind", Default = ReportKind.Endpoint, HelpText = "Kind of report to generate. Does not generate reports if not provided", MetaValue = "<RawTemplates|Endpoint>")]
    public ReportKind GeneratedReportKind { get; set; }

    [Option('o', "output-dir", Default = ".", HelpText = "Directory to output the generated report. Will be suffixed with 'report/'", MetaValue = "<directory>")]
    public required string OutputDirectory { get; set; }

    [Option('t', "templates-dir", Default = "./templates", HelpText = "Directory containing additional templates for the generated report", MetaValue = "<directory>")]
    public required string AdditionalTemplatesDir { get; set; }
}
