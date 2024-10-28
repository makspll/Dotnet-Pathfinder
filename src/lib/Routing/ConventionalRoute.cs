using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using FluentResults;
using Makspll.Pathfinder.Intermediate;
using Makspll.Pathfinder.PostProcess;

namespace Makspll.Pathfinder.Routing;
public partial class ConventionalRoute
{

    public static readonly Dictionary<string, string> CONVENTIONAL_SPECIAL_ESCAPE_PLACEHOLDERS = new()
        {
            {"{{", PlaceholderInliner.LEFT_BRACE_ESCAPE_PLACEHOLDER},
            {"}}", PlaceholderInliner.RIGHT_BRACE_ESCAPE_PLACEHOLDER}
        };

    public required ConventionalRouteType? Type { get; set; }
    /**
     * The template for the conventional route i.e. 
     * `{area}/api/{controller}/{action}/{id?}/{custom:string}`
     */
    private IEnumerable<RouteTemplatePart> Template = [];

    private Dictionary<string, string>? Defaults;

    public string? GetPartDefaultValue(string partName)
    {
        var part = Template.FirstOrDefault(x => x.PartName == partName);
        if (part == null)
            return Defaults?.GetValueOrDefault(partName);

        return part.DefaultValue;
    }

    public RouteTemplatePart? ControllerPart => Template.FirstOrDefault(x => x.PartName == "controller");
    public RouteTemplatePart? ActionPart => Template.FirstOrDefault(x => x.PartName == "action");
    public RouteTemplatePart? AreaPart => Template.FirstOrDefault(x => x.PartName == "area");
    public RouteTemplatePart? IdPart => Template.FirstOrDefault(x => x.PartName == "id");


    /// <summary>
    /// Instantiates the template with the given controller, action and area, filling in with defaults if specified.
    /// Will include special characters symbolizing escaped braces like `{{ == Ѻ` and `}} == ѻ`
    /// </summary>
    public string InstantiateTemplateWith(string? controller, string? action, string? area, bool fillInWithDefaults = false)
    {
        if (fillInWithDefaults)
            throw new NotImplementedException("Filling in with defaults is not yet implemented");

        var route = new StringBuilder();
        foreach (var part in Template)
        {
            if (controller != null && part.PartName == "controller")
                route.Append(controller);
            else if (action != null && part.PartName == "action")
                route.Append(action);
            else if (area != null && part.PartName == "area")
                route.Append(area);
            else
                route.Append(part.ToString());

        }

        var output = route.ToString();


        if (!output.StartsWith('/'))
            output = '/' + output;

        if (output.EndsWith('/'))
            output = output[..^1];

        return output;
    }

    private static IEnumerable<(bool IsPlaceholder, string part)> IteratePartRanges(string part, Regex regex)
    {
        var matches = regex.Matches(part);

        if (matches.Count == 0)
        {
            yield return (false, part);
            yield break;
        }

        var lastIndexInclusive = 0;
        foreach (Match match in matches)
        {
            var unprocessedCharactersSinceLastMatch = part[lastIndexInclusive..match.Index];
            if (unprocessedCharactersSinceLastMatch.Length > 0)
                yield return (false, unprocessedCharactersSinceLastMatch);
            // also yield match
            yield return (true, match.Value);
            lastIndexInclusive = match.Index + match.Length;
        }

        var lastUnprocessedCharacters = part[lastIndexInclusive..];
        if (lastUnprocessedCharacters.Length > 0)
            yield return (false, lastUnprocessedCharacters);
    }

    /**
    * Tries to parse a valid route template and returns a ConventionalRoute object or parse failure.
    */
    public static Result<ConventionalRoute> Parse(string route, Dictionary<string, string>? defaults, ConventionalRouteType? type, FrameworkVersion version)
    {
        if (version != FrameworkVersion.DOTNET_FRAMEWORK)
        {
            foreach (var mapping in CONVENTIONAL_SPECIAL_ESCAPE_PLACEHOLDERS)
            {
                route = route.Replace(mapping.Key, mapping.Value);
            }
        }

        var templateParts = new List<RouteTemplatePart>();
        // the parts may be complex themselves, i.e. /{part}.{part2}/
        var regex = PlaceholderPartRegex();
        foreach (var (IsPlaceholder, part) in IteratePartRanges(route, regex))
        {

            if (IsPlaceholder)
            {
                var templateRoutePart = RouteTemplatePart.Parse(part);
                if (templateRoutePart.IsFailed)
                    return Result.Fail($"Failed to parse route part: {part}")
                            .WithErrors(templateRoutePart.Errors);
                templateParts.Add(templateRoutePart.Value);
            }
            else
                templateParts.Add(new RouteTemplatePart
                {
                    PartName = part,
                    IsOptional = false,
                    Constraints = null,
                    DefaultValue = null,
                    IsConstant = true
                });
        }

        return new ConventionalRoute
        {
            Type = type,
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
            // allow escaping of special characters

            if (!part.StartsWith('{') && !part.EndsWith('}'))
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
                var constraints = Constraints != null && Constraints.Any() ? ":" + string.Join(':', Constraints) : "";
                var defaultValue = DefaultValue != null ? "=" + DefaultValue : "";
                return $"{{{PartName}{constraints}{defaultValue}{optional}}}";
            }
        }
    }

    [GeneratedRegex(@"\{[^{}]*?\}")]
    private static partial Regex PlaceholderPartRegex();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConventionalRouteType
{
    MVC,
    API
}

public static class ConventionalRouteExtensions
{
    /// <summary>
    /// Converts a ConventionalRouteType to a ControllerKind
    /// </summary>
    public static ControllerKind? ToControllerKind(this ConventionalRouteType? type)
    {
        return type switch
        {
            ConventionalRouteType.MVC => ControllerKind.MVC,
            ConventionalRouteType.API => ControllerKind.API,
            _ => null
        };
    }
}