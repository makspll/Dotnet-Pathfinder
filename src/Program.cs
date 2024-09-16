using System.Text.Json;
using ANSIConsole;
using Makspll.ReflectionUtils;
using Makspll.ReflectionUtils.Routing;
using Makspll.ReflectionUtils.Search;
using Makspll.ReflectionUtils.Serialization;

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

var dlls = FileSearch.FindAllFiles(parsedArgs.DLLGlobs, parsedArgs.Directory).ToList();
var serializationOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    Converters = { new RoutingAttributeConverter() }
};
var output = new List<Assembly>();

foreach (var dll in dlls)
{
    var query = new AssemblyQuery(dll);
    var controllers = query.FindControllers();

    var assembly = new Assembly
    {
        Name = dll.Split('/').Last(),
        Path = dll,
        Controllers = controllers
    };
    output.Add(assembly);
}

if (parsedArgs.OutputFormat == OutputFormat.Text)
{

    foreach (var assembly in output)
    {
        if (output.Count > 1)
        {
            Console.WriteLine("---------------".Color(ConsoleColor.DarkYellow));
        }

        Console.WriteLine($"Assembly: {assembly.Name.Color(ConsoleColor.DarkGreen)}, Path: {assembly.Path.Color(ConsoleColor.DarkGreen).Underlined()}");
        foreach (var controller in assembly.Controllers)
        {
            Console.WriteLine($"Controller: {controller.Name.Color(ConsoleColor.DarkGreen)}");
            foreach (var action in controller.Actions)
            {
                foreach (var route in action.Routes)
                {
                    Console.WriteLine($"  - {action.Name.Color(ConsoleColor.DarkGray)} {route.Color(ConsoleColor.DarkCyan)}");
                }
            }
        }
    }
}
else
{
    Console.Write(JsonSerializer.Serialize(output, serializationOptions));
}

