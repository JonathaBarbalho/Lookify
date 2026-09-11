using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lookify.Providers.JsonConverters;

internal sealed class FlexibleDecimalJsonConverter : JsonConverter<decimal?> {

    public override decimal? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        return reader.TokenType switch {
            JsonTokenType.Number => reader.GetDecimal(),
            JsonTokenType.String => ParseDecimal(reader.GetString()),
            JsonTokenType.Null => null,
            _ => throw new JsonException($"Unable to convert {reader.TokenType} to decimal.")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        decimal? value,
        JsonSerializerOptions options)
    {
        if (value is null) {
            writer.WriteNullValue();
            return;
        }

        writer.WriteNumberValue(value.Value);
    }

    private static decimal? ParseDecimal(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        try {
            return decimal.Parse(value, CultureInfo.InvariantCulture);
        }
        catch (Exception exception) when (exception is FormatException or OverflowException) {
            throw new JsonException(
                $"Não foi possível converter \"{value}\" em decimal.",
                exception);
        }
    }
}
