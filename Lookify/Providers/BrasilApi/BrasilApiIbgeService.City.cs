using System.Text.Json.Serialization;
using Lookify.Ibge;

namespace Lookify.Providers.BrasilApi;

internal sealed partial class BrasilApiIbgeService {

    public async Task<List<IbgeCityLookifyResultDto>> GetCitiesByStateAsync(
        string uf,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/ibge/municipios/v1/{uf}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Ibge");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<BrasilApiIbgeCityProviderResponse>>(
            providerName: ProviderName,
            identifier: uf,
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult(uf)).ToList();
    }

    public Task<List<IbgeCityLookifyResultDto>> GetAllCitiesAsync(
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException(
            $"Provider {ProviderName} não oferece listagem de todos os municípios de uma vez.");
}

internal sealed record class BrasilApiIbgeCityProviderResponse {

    [property: JsonPropertyName("nome")]
    public string? Nome { get; init; }

    [property: JsonPropertyName("codigo_ibge")]
    public string? CodigoIbge { get; init; }

    public IbgeCityLookifyResultDto ToResult(
        string uf) =>
        new IbgeCityLookifyResultDto {
            Id = int.TryParse(CodigoIbge, out var id) ? id : null,
            Name = Nome,
            StateUf = uf
        };
}
