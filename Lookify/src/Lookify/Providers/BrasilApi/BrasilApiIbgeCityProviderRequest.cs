using System.Text.Json.Serialization;
using Lookify.Ibge;

namespace Lookify.Providers.BrasilApi;

internal static class BrasilApiIbgeCityProviderRequest {

    public static string ProviderName => "BrasilApi";

    public static string BaseAddress => "https://brasilapi.com.br/";

    public static async Task<List<IbgeCityLookifyResultDto>> RequestByStateAsync(
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
