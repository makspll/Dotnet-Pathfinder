using System;
using CommandLine;
using Makspll.Pathfinder;
using Makspll.Pathfinder.Reports;

namespace Makspll.PathfinderApp;




public record Args
{
    [Value(0, MetaName = "DLLGlobs", Required = true, HelpText = "Glob patterns for DLLs to analyze (e.g. *.dll or **/*.dll)")]
    public required IEnumerable<string> DLLGlobs { get; set; }

    [Option('o', "output-format", Default = OutputFormat.JSON, HelpText = "Output format", MetaValue = "<JSON|Text>")]
    public required OutputFormat OutputFormat { get; set; }

    [Option('d', "directory", Default = ".", HelpText = "Root directory to search for DLLs", MetaValue = "<directory>")]
    public required string Directory { get; set; }

    [Option('c', "config", Default = "pathfinder.json", HelpText = "Path to config file", MetaValue = "<config file>")]
    public required string Config { get; set; }

    [Option('r', "report-kind", Default = null, HelpText = "Kind of report to generate. Does not generate reports if not provided", MetaValue = "<RawTemplates|Endpoint>")]
    public ReportKind? GeneratedReportKind { get; set; }

    [Option('t', "templates-dir", Default = null, HelpText = "Directory containing additional templates for the generated report", MetaValue = "<directory>")]
    public string? AdditionalTemplatesDir { get; set; }

    [Option('O', "output-directory", Default = null, HelpText = "Directory to output the generated report. Will be suffixed with 'report/'", MetaValue = "<directory>")]
    public string? OutputDirectory { get; set; }
}

public static class InputParser
{
    public static Args Parse(string[] args)
    {

        var parsed = Parser.Default.ParseArguments<Args>(args);
        return parsed.MapResult(
            (Args a) => a,
            (IEnumerable<Error> errors) =>
            {
                var errorMessage = "Error parsing arguments" + Environment.NewLine + string.Join(Environment.NewLine, errors.Select(e => e.ToString()));
                throw new Exception(errorMessage);
            }
        );
    }
}

