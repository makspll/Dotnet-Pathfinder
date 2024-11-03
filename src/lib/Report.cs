using Makspll.Pathfinder;
using Makspll.Pathfinder.Reports;
using ANSIConsole;

namespace Makspll.Pathfinder;

public class Report(Pathfinder pathfinder, ReportKind kind, string? outputDirectory, string? additionalTemplatesDir)
{
    public void GenerateReport()
    {
        Console.WriteLine("Analyzing assemblies...".Color(ConsoleColor.Green));
        var assemblies = pathfinder.Analyze();
        Console.Write("Found: ".Color(ConsoleColor.Green));
        Console.WriteLine($"{string.Join(", ", assemblies.Select(x => x.Name))}".Color(ConsoleColor.Yellow));
        var generator = new ReportGenerator(kind, additionalTemplatesDir);
        generator.GenerateReport(assemblies, outputDirectory);
    }
}