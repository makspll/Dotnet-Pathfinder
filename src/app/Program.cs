using ANSIConsole;
using Makspll.Pathfinder.Routing;
using Makspll.Pathfinder.Search;
using Makspll.PathfinderApp;
using Newtonsoft.Json;

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

var output = new List<Assembly>();

foreach (var dll in dlls)
{
    AssemblyQuery query;
    var configFileLocations = new List<string>
    {
        parsedArgs.Config,
        Path.Combine(parsedArgs.Directory, "pathfinder.json"),
        Path.Combine(Path.GetDirectoryName(dll) ?? dll,"..", "..","..","pathfinder.json")
    };

    var config = configFileLocations.Select(x => AssemblyQuery.ParseConfig(new FileInfo(x))).FirstOrDefault(x => x != null);
    if (config == null)
    {
        Console.WriteLine($"No 'pathfinder.json' file specified or found when processing dll: '{dll}'. Ignoring".Color(ConsoleColor.Red));
        continue;
    }

    query = new AssemblyQuery(dll, config);

    var controllers = query.FindAllControllers().ToList();

    var assembly = new Assembly
    {
        Name = dll.Split('/').Last(),
        Path = dll,
        Controllers = controllers,
        FrameworkVersion = query.DetectedFramework
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

        Console.WriteLine($"Assembly: {assembly.Name.Color(ConsoleColor.DarkGreen)}, Path: {assembly.Path.Color(ConsoleColor.DarkGreen).Underlined()} Type: {assembly.FrameworkVersion.ToString().Color(ConsoleColor.DarkGreen)}");
        foreach (var controller in assembly.Controllers)
        {
            Console.WriteLine($"Controller: {controller.ClassName.Color(ConsoleColor.DarkGreen)}");
            foreach (var action in controller.Actions)
            {
                var conventional = action.IsConventional ? "Conventional" : "Non-Conventional";
                Console.WriteLine($"  Action: {action.MethodName.Color(ConsoleColor.DarkMagenta)} - {conventional.Color(ConsoleColor.DarkGray)}");
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

                    Console.WriteLine($"    {methods.Color(ConsoleColor.DarkGray)} '{route.Path.Color(ConsoleColor.DarkCyan)}'");
                }
            }
        }
    }
}
else
{

    // write to stdout
    using var writer = new StreamWriter(Console.OpenStandardOutput());
    writer.Write(JsonConvert.SerializeObject(output, Formatting.Indented));
}

