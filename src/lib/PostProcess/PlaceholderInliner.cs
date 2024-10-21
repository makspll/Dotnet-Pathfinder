using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.Routing;

namespace Makspll.Pathfinder.PostProcess;

public class PlaceholderInliner(FrameworkVersion version)
{
    private readonly FrameworkVersion _version = version;

    public void InlinePlaceholders(IEnumerable<ControllerCandidate> controllers)
    {
        // not supported in .NET Framework
        if (_version == FrameworkVersion.DOTNET_FRAMEWORK)
            return;

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
                        route.Path = route.Path.Replace("[action]", action.ActionName(_version));
                    }
                }
            }
        }
    }
}