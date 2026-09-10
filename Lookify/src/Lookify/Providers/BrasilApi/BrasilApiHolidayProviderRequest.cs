using System.Text.Json.Serialization;
using Lookify.Holiday;

namespace Lookify.Providers.BrasilApi;

internal static class BrasilApiHolidayProviderRequest {

    public static string ProviderName => "BrasilApi";

    public static string BaseAddress => "https://brasilapi.com.br/";

    public static async Task<List<HolidayLookifyResultDto>> RequestAsync(
        int year,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/feriados/v1/{year}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Holiday");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<BrasilApiHolidayProviderResponse>>(
            providerName: ProviderName,
            identifier: year.ToString(),
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult()).ToList();
    }
}

internal sealed record class BrasilApiHolidayProviderResponse {

    [property: JsonPropertyName("date")]
    public DateOnly? Date { get; init; }

    [property: JsonPropertyName("name")]
    public string? Name { get; init; }

    [property: JsonPropertyName("type")]
    public string? Type { get; init; }

    [property: JsonPropertyName("weekday")]
    public string? Weekday { get; init; }

    public HolidayLookifyResultDto ToResult() =>
        new HolidayLookifyResultDto {
            Date = Date,
            LocalName = Name,
            Type = Type,
            Weekday = Weekday
        };
}
