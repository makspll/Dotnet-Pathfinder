using FluentResults;

namespace Makspll.Pathfinder.Routing;
public class ConventionalRoute
{
    /**
     * The template for the conventional route i.e. 
     * `{area}/api/{controller}/{action}/{id?}/{custom:string}`
     */
    public required IEnumerable<RouteTemplatePart> Template { get; set; }

    public required Dictionary<string, string>? Defaults { get; set; }

    public string? GetPartDefaultValue(string partName)
    {
        var part = Template.FirstOrDefault(x => x.PartName == partName);
        if (part == null)
            return Defaults?.GetValueOrDefault(partName);

        return part.DefaultValue;
    }

    public RouteTemplatePart? Controller => Template.FirstOrDefault(x => x.PartName == "controller");
    public RouteTemplatePart? Action => Template.FirstOrDefault(x => x.PartName == "action");
    public RouteTemplatePart? Area => Template.FirstOrDefault(x => x.PartName == "area");
    public RouteTemplatePart? Id => Template.FirstOrDefault(x => x.PartName == "id");


    public string InstantiateTemplateWith(string? controller, string? action, string? area, bool fillInWithDefaults = false)
    {
        if (fillInWithDefaults)
            throw new NotImplementedException("Filling in with defaults is not yet implemented");

        var parts = new List<string>();
        foreach (var part in Template)
        {
            if (part.IsConstant)
                parts.Add(part.PartName);
            else
            {
                if (controller != null && part.PartName == "controller")
                    parts.Add(controller);
                else if (action != null && part.PartName == "action")
                    parts.Add(action);
                else if (area != null && part.PartName == "area")
                    parts.Add(area);
                else
                    parts.Add(part.DefaultValue ?? "");
            }
        }

        return '/' + string.Join('/', parts);
    }

    /**
    * Tries to parse a valid route template and returns a ConventionalRoute object or parse failure.
    */
    public static Result<ConventionalRoute> Parse(string route, Dictionary<string, string>? defaults)
    {

        var routeParts = route.Split('/');
        var templateParts = new List<RouteTemplatePart>();
        for (int i = 0; i < routeParts.Length; i++)
        {
            var routePart = routeParts[i];
            var templateRoutePart = RouteTemplatePart.Parse(routePart);

            if (templateRoutePart.IsFailed)
                return Result.Fail($"Failed to parse route part: {routePart}")
                        .WithErrors(templateRoutePart.Errors);
            else
                templateParts.Add(templateRoutePart.Value);
        }

        return new ConventionalRoute
        {
            Template = templateParts,
            Defaults = defaults
        };
    }


    public class RouteTemplatePart
    {
        public required string PartName { get; set; }

        public required bool IsOptional { get; set; }

        public required IEnumerable<string>? Constraints { get; set; }

        public required string? DefaultValue { get; set; }

        public required bool IsConstant { get; set; }


        /**
         * Parses a route part and returns a RoutePart object or parse failure.
         */
        public static Result<RouteTemplatePart> Parse(string part)
        {
            if (!part.StartsWith('{') || !part.EndsWith('}'))
            {
                return new RouteTemplatePart
                {
                    PartName = part,
                    IsOptional = false,
                    Constraints = null,
                    DefaultValue = null,
                    IsConstant = true
                };
            }

            var innerPart = part[1..^1];

            var isOptional = innerPart.EndsWith('?');
            var hasDefault = innerPart.Contains('=');

            if (isOptional && hasDefault)
            {
                return Result.Fail($"Route part cannot both be optional and have a default value: {part}");
            }

            string? defaultValue = null;
            if (hasDefault)
            {
                var parts = innerPart.Split('=');
                innerPart = parts[0];
                defaultValue = parts[1];
            }
            if (isOptional)
            {
                innerPart = innerPart[..^1];
            }


            string[] constraints = [];
            if (innerPart.Contains(':'))
            {

                var parts = innerPart.Split(':');
                innerPart = parts[0];
                constraints = parts[1..];
            }

            return new RouteTemplatePart
            {
                PartName = innerPart,
                IsOptional = isOptional,
                Constraints = constraints,
                DefaultValue = defaultValue,
                IsConstant = false
            };
        }

        public override string ToString()
        {
            if (IsConstant)
                return PartName;
            else
            {
                var optional = IsOptional ? "?" : "";
                var constraints = Constraints != null ? ":" + string.Join(':', Constraints) : "";
                var defaultValue = DefaultValue != null ? "=" + DefaultValue : "";
                return $"{{{PartName}{constraints}{defaultValue}{optional}}}";
            }
        }
    }
}
