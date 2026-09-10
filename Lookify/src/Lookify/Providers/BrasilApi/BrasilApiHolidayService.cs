using System.Text.Json.Serialization;
using Lookify.Holiday;

namespace Lookify.Providers.BrasilApi;

internal sealed class BrasilApiHolidayService : IHolidayProviderService {

    public string ProviderName { get; init; } = string.Empty;

    public string BaseAddress { get; init; } = string.Empty;

    public bool IsEnabled { get; private set; }

    public void UpdateEnabled(bool isEnabled) =>
        IsEnabled = isEnabled;

    public async Task<List<HolidayLookifyResultDto>> GetHolidaysAsync(
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
