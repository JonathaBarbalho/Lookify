using System.Text.Json.Serialization;
using Lookify.Cep;

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

    public CepLookifyResultDto ToResult() =>
        new CepLookifyResultDto {
            ZipCode = Cep,
            Street = Street,
            Complement = null,
            Neighborhood = Neighborhood,
            City = City,
            State = State,
            IbgeCityCode = null
        };
}
