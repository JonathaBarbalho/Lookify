using System.Text.Json.Serialization;
using Lookify.Holiday;

namespace Lookify.Providers.NagerDate;

internal sealed class NagerDateHolidayService : IHolidayProviderService {

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
        var fullAddress = $"{BaseAddress}api/v3/PublicHolidays/{year}/BR";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<NagerDateHolidayProviderResponse>>(
            providerName: ProviderName,
            identifier: year.ToString(),
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult()).ToList();
    }
}

internal sealed record class NagerDateHolidayProviderResponse {

    [property: JsonPropertyName("date")]
    public DateOnly? Date { get; init; }

    [property: JsonPropertyName("name")]
    public string? Name { get; init; }

    [property: JsonPropertyName("localName")]
    public string? LocalName { get; init; }

    [property: JsonPropertyName("types")]
    public List<string> Types { get; init; } = [];

    public HolidayLookifyResultDto ToResult() =>
        new HolidayLookifyResultDto {
            Date = Date,
            Name = Name,
            LocalName = LocalName,
            Type = Types.Count > 0 ? string.Join(", ", Types) : null
        };
}
