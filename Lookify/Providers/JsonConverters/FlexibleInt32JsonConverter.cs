using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lookify.Providers.JsonConverters;

internal sealed class FlexibleInt32JsonConverter : JsonConverter<int?> {

    public override int? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return reader.TokenType switch {
            JsonTokenType.Number => reader.GetInt32(),
            JsonTokenType.String => ParseInt32(reader.GetString()),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unable to convert {reader.TokenType} to int.")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        int? value,
        JsonSerializerOptions options)
    {
        if (value is null) {
            writer.WriteNullValue();
            return;
        }

        writer.WriteNumberValue(value.Value);
    }

    private static int? ParseInt32(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try {
            return int.Parse(value, CultureInfo.InvariantCulture);
        }
        catch (Exception exception) when (exception is FormatException or OverflowException) {
            throw new JsonException(
                $"Não foi possível converter \"{value}\" em int.",
                exception);
        }
    }
}
