using System.Text;
using ANSIConsole;
using dnlib.DotNet;
using Newtonsoft.Json;

namespace Makspll.Pathfinder.Routing;

public class Assembly
{
    public required string Name { get; init; }

    public required string Path { get; init; }

    public required FrameworkVersion FrameworkVersion { get; init; }
    public required IEnumerable<Controller> Controllers { get; init; }

    public static void Serialize(IEnumerable<Assembly> assemblies, OutputFormat format, StreamWriter writer)
    {

        if (format == OutputFormat.Text)
        {
            foreach (var assembly in assemblies)
            {
                writer.WriteLine($"Assembly: {assembly.Name.Color(ConsoleColor.DarkGreen)}, Path: {assembly.Path.Color(ConsoleColor.DarkGreen).Underlined()} Type: {assembly.FrameworkVersion.ToString().Color(ConsoleColor.DarkGreen)}");
                foreach (var controller in assembly.Controllers)
                {
                    writer.WriteLine($"Controller: {controller.ClassName.Color(ConsoleColor.DarkGreen)}");
                    foreach (var action in controller.Actions)
                    {
                        var conventional = action.IsConventional ? "Conventional" : "Non-Conventional";
                        writer.WriteLine($"  Action: {action.MethodName.Color(ConsoleColor.DarkMagenta)} - {conventional.Color(ConsoleColor.DarkGray)}");
                        foreach (var route in action.Routes)
                        {
                            string methods;
                            if (route.Methods.Count == Enum.GetValues<HTTPMethod>().Length)
                            {
                                methods = "ALL";
                            }
                            else
                            {
                                methods = string.Join(',', route.Methods.Select(x => x.ToString()));
                            }

                            writer.WriteLine($"    {methods.Color(ConsoleColor.DarkGray)} '{route.Path.Color(ConsoleColor.DarkCyan)}'");
                        }
                    }
                }
            }

            if (assemblies.Count() > 1)
            {
                writer.WriteLine("---------------".Color(ConsoleColor.DarkYellow));
            }

        }
        else if (format == OutputFormat.JSON)
        {
            writer.Write(JsonConvert.SerializeObject(assemblies, Formatting.Indented));
        }

        writer.Flush();
    }
}