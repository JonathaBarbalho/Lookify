using System.Text.Json.Serialization;
using Lookify.Cep;

namespace Lookify.Providers.ViaCep;

internal sealed record class ViaCepProviderResponse {

    [property: JsonPropertyName("cep")]
    public string? Cep { get; init; }

    [property: JsonPropertyName("logradouro")]
    public string? Logradouro { get; init; }

    [property: JsonPropertyName("complemento")]
    public string? Complemento { get; init; }

    [property: JsonPropertyName("bairro")]
    public string? Bairro { get; init; }

    [property: JsonPropertyName("localidade")]
    public string? Localidade { get; init; }

    [property: JsonPropertyName("uf")]
    public string? Uf { get; init; }

    [property: JsonPropertyName("ibge")]
    public string? Ibge { get; init; }

    [property: JsonPropertyName("erro")]
    public bool Erro { get; init; }

    public CepLookifyResultDto ToResult() =>
        new CepLookifyResultDto {
            ZipCode = Cep,
            Street = Logradouro,
            Complement = Complemento,
            Neighborhood = Bairro,
            City = Localidade,
            State = Uf,
            IbgeCityCode = Ibge
        };
}
