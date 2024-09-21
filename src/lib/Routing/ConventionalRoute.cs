using FluentResults;

namespace Makspll.Pathfinder.Routing;
public class ConventionalRoute
{
    /**
     * The template for the conventional route i.e. 
     * `{area}/api/{controller}/{action}/{id?}/{custom:string}`
     */
    public required IEnumerable<RouteTemplatePart> Template { get; set; }

    public required Dictionary<string, string> Defaults { get; set; }


    /**
    * Tries to parse a valid route template and returns a ConventionalRoute object or parse failure.
    */
    public static Result<ConventionalRoute>? Parse(string route, Dictionary<string, string> defaults)
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
                    DefaultValue = null
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
                DefaultValue = defaultValue
            };
        }
    }
}
