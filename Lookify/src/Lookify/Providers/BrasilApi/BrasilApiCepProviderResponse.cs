using System.Text.Json.Serialization;
using Lookify.Cep;
using Lookify.Providers.JsonConverters;

namespace Lookify.Providers.BrasilApi;

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
