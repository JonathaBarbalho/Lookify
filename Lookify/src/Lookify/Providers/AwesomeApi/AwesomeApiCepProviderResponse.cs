using System.Text.Json.Serialization;
using Lookify.Cep;
using Lookify.Providers.JsonConverters;

namespace Lookify.Providers.AwesomeApi;

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
