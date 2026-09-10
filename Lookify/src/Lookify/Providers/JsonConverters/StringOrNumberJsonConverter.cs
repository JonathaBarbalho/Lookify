using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lookify.Providers.JsonConverters;

internal sealed class StringOrNumberJsonConverter : JsonConverter<string?> {

    public override string? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return reader.TokenType switch {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number => reader.TryGetDecimal(out var value)
                ? value.ToString(CultureInfo.InvariantCulture)
                : reader.GetDouble().ToString(CultureInfo.InvariantCulture),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unable to convert {reader.TokenType} to string.")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        string? value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
