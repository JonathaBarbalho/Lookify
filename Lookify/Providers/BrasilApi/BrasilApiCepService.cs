using System.Text.Json.Serialization;
using Lookify.Cep;
using Lookify.Providers.JsonConverters;

namespace Lookify.Providers.BrasilApi;

internal sealed class BrasilApiCepService : ICepProviderService {

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
        var fullAddress = $"{BaseAddress}api/cep/v2/{identifier}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<BrasilApiCepProviderResponse>(
            providerName: ProviderName,
            identifier,
            response,
            cancellationToken);

        return payload.ToResult();
    }
}

internal sealed record class BrasilApiCepProviderResponse {

    [property: JsonPropertyName("cep")]
    public string? Cep { get; init; }

    [property: JsonPropertyName("state")]
    public string? State { get; init; }

    [property: JsonPropertyName("city")]
    public string? City { get; init; }

    [property: JsonPropertyName("neighborhood")]
    public string? Neighborhood { get; init; }

    [property: JsonPropertyName("street")]
    public string? Street { get; init; }

    [property: JsonPropertyName("ibge")]
    public BrasilApiCepIbgeResponse? Ibge { get; init; }

    [property: JsonPropertyName("location")]
    public BrasilApiCepLocationResponse? Location { get; init; }

    public CepLookifyResultDto ToResult() =>
        new CepLookifyResultDto {
            ZipCode = Cep,
            Street = Street,
            Complement = null,
            Neighborhood = Neighborhood,
            City = City,
            State = State,
            IbgeCityCode = Ibge?.City,
            Latitude = Location?.Coordinates?.Latitude,
            Longitude = Location?.Coordinates?.Longitude
        };
}

internal sealed record class BrasilApiCepIbgeResponse {

    [property: JsonPropertyName("city")]
    public string? City { get; init; }

    [property: JsonPropertyName("state")]
    public string? State { get; init; }
}

internal sealed record class BrasilApiCepLocationResponse {

    [property: JsonPropertyName("coordinates")]
    public BrasilApiCepCoordinatesResponse? Coordinates { get; init; }
}

internal sealed record class BrasilApiCepCoordinatesResponse {

    [property: JsonPropertyName("longitude")]
    [property: JsonConverter(typeof(FlexibleDecimalJsonConverter))]
    public decimal? Longitude { get; init; }

    [property: JsonPropertyName("latitude")]
    [property: JsonConverter(typeof(FlexibleDecimalJsonConverter))]
    public decimal? Latitude { get; init; }
}
