using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.PostProcess;

public class PlaceholderInliner
{
    public static void InlinePlaceholders(List<Controller> controllers)
    {
        foreach (var controller in controllers)
        {
            foreach (var action in controller.Actions)
            {
                foreach (var route in action.Routes)
                {
                    if (route.Path.Contains("[controller]"))
                    {
                        route.Path = route.Path.Replace("[controller]", controller.ControllerName);
                    }
                    if (route.Path.Contains("[action]"))
                    {
                        var actionNameOverride = action.Attributes.FirstOrDefault(x => x.ActionName() != null)?.ActionName();
                        route.Path = route.Path.Replace("[action]", actionNameOverride ?? action.MethodName);
                    }
                }
            }
        }
    }
}