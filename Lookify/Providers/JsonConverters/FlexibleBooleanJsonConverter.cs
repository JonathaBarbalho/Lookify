using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lookify.Providers.JsonConverters;

internal sealed class FlexibleBooleanJsonConverter : JsonConverter<bool?> {

    public override bool? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return reader.TokenType switch {
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.Number => ReadNumber(reader),
            JsonTokenType.String => ReadString(reader.GetString()),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unable to convert {reader.TokenType} to boolean.")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        bool? value,
        JsonSerializerOptions options)
    {
        if (value is null) {
            writer.WriteNullValue();
            return;
        }

        writer.WriteBooleanValue(value.Value);
    }

    private static bool ReadNumber(
        Utf8JsonReader reader)
    {
        if (reader.TryGetInt32(out var intValue))
            return intValue != 0;

        if (reader.TryGetDecimal(out var decimalValue))
            return decimalValue != 0m;

        return reader.GetDouble() != 0d;
    }

    private static bool? ReadString(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = value.Trim().ToLowerInvariant();

        return normalized switch {
            "true" or "1" or "sim" or "s" or "yes" or "y" => true,
            "false" or "0" or "não" or "nao" or "n" or "no" => false,
            _ when bool.TryParse(normalized, out var parsedValue) => parsedValue,
            _ => throw new JsonException($"Unable to convert \"{value}\" to boolean.")
        };
    }
}
