namespace Makspll.ReflectionUtils.Serialization;

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Makspll.ReflectionUtils.Routing;

public class RoutingAttributeConverter : JsonConverter<RoutingAttribute>
{
    public override RoutingAttribute Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, RoutingAttribute value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Name", value.Name);
        writer.WriteString("Route", value.Route());
        writer.WriteString("HttpMethod", value.HttpMethodOverride()?.ToString());
        writer.WriteEndObject();
    }
}