using System.Text.Json.Serialization;
using Lookify.Cep;
using Lookify.Providers.JsonConverters;

namespace Lookify.Providers.AwesomeApi;

internal sealed class AwesomeApiCepService : ICepProviderService {

    public string ProviderName { get; init; } = string.Empty;

    public string BaseAddress { get; init; } = string.Empty;

    public bool IsEnabled { get; private set; }

    public void UpdateEnabled(bool isEnabled) =>
        IsEnabled = isEnabled;

    public async Task<CepLookifyResultDto> RequestAsync(
        string identifier,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}json/{identifier}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<AwesomeApiCepProviderResponse>(
            providerName: ProviderName,
            identifier,
            response,
            cancellationToken);

        return payload.ToResult();
    }
}

internal sealed record class AwesomeApiCepProviderResponse {

    [property: JsonPropertyName("cep")]
    public string? Cep { get; init; }

    [property: JsonPropertyName("address")]
    public string? Address { get; init; }

    [property: JsonPropertyName("district")]
    public string? District { get; init; }

    [property: JsonPropertyName("city")]
    public string? City { get; init; }

    [property: JsonPropertyName("state")]
    public string? State { get; init; }

    [property: JsonPropertyName("city_ibge")]
    public string? CityIbge { get; init; }

    [property: JsonPropertyName("ddd")]
    public string? Ddd { get; init; }

    [property: JsonPropertyName("lat")]
    [property: JsonConverter(typeof(FlexibleDecimalJsonConverter))]
    public decimal? Lat { get; init; }

    [property: JsonPropertyName("lng")]
    [property: JsonConverter(typeof(FlexibleDecimalJsonConverter))]
    public decimal? Lng { get; init; }

    public CepLookifyResultDto ToResult() =>
        new CepLookifyResultDto {
            ZipCode = Cep,
            Street = Address,
            Complement = null,
            Neighborhood = District,
            City = City,
            State = State,
            IbgeCityCode = CityIbge,
            Latitude = Lat,
            Longitude = Lng,
            Ddd = Ddd
        };
}
