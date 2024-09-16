using System;
using CommandLine;

namespace Makspll.ReflectionUtils
{

    public enum OutputFormat
    {
        JSON,
        Text
    }

    public record Args
    {
        [Value(0, MetaName = "DLLGlobs", Required = true, HelpText = "Glob patterns for DLLs to analyze (e.g. *.dll or **/*.dll)")]
        public required IEnumerable<string> DLLGlobs { get; set; }

        [Option('o', "output-format", Default = OutputFormat.JSON, HelpText = "Output format", MetaValue = "<JSON|Text>")]
        public required OutputFormat OutputFormat { get; set; }

        [Option('d', "directory", Default = ".", HelpText = "Root directory to search for DLLs", MetaValue = "<directory>")]
        public required string Directory { get; set; }

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

}