﻿using System.Text.Json;
using ANSIConsole;
using Makspll.Pathfinder.Routing;
using Makspll.Pathfinder.Search;
using Makspll.Pathfinder.Serialization;
using Makspll.PathfinderApp;

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
    var controllers = query.FindAllControllers().ToList();

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
                var conventional = action.IsConventional ? "Conventional" : "Non-Conventional";
                Console.WriteLine($"  Action: {action.Name.Color(ConsoleColor.DarkMagenta)} - {conventional.Color(ConsoleColor.DarkGray)}");
                foreach (var route in action.Routes)
                {
                    string methods;
                    if (route.Methods.Count() == Enum.GetValues<HTTPMethod>().Length)
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
    Console.Write(JsonSerializer.Serialize(output, serializationOptions));
}
