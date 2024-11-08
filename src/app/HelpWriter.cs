using System.Text.RegularExpressions;
using ANSIConsole;

public class HelpWriter(Stream stream) : StreamWriter(stream)
{


    public override void Write(string? value)
    {
        if (value == null)
        {
            return;
        }

        var flagsRegex = new Regex(@"(-{1,2}[a-zA-Z0-9-]+)");
        var placeholdersRegex = new Regex(@"<[^>]+>");
        var headerRegex = new Regex(@"(Pathfinder.*|.*Maksymilian.*)");
        var positionalArgs = new Regex(@"([a-zA-Z0-9]+)\s+\(pos\.\s+\d+\)\s+");
        var helpText = new Regex(@"(?:.*?)(\s{4,}.*?)");

        // replace flags with bolded and colored flags
        value = flagsRegex.Replace(value, match =>
        {
            return match.Value.Color(ConsoleColor.DarkBlue).Bold().ToString();
        });

        // replace placeholders with italic and colored placeholders
        value = placeholdersRegex.Replace(value, match =>
        {
            return match.Value.Color(ConsoleColor.DarkRed).Italic().ToString();
        });

        // replace header with bolded and colored header
        value = headerRegex.Replace(value, match =>
        {
            return match.Value.Color(ConsoleColor.DarkGreen).Underlined().ToString();
        });

        // replace positional arguments with bolded and colored positional arguments
        value = positionalArgs.Replace(value, match =>
        {
            return match.Value.Color(ConsoleColor.Blue).Bold().ToString();
        });

        // replace help text with colored help text
        value = helpText.Replace(value, match =>
        {
            return match.Value.Color(ConsoleColor.Black).ToString();
        });

        base.Write(value);
    }



}