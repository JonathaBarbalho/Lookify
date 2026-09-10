using System.Text.Json.Serialization;
using Lookify.Ibge;
using Lookify.Providers.Ibge;

namespace Lookify.Providers.BrasilApi;

internal static class BrasilApiIbgeStateProviderRequest {

    public static string ProviderName => "BrasilApi";

    public static string BaseAddress => "https://brasilapi.com.br/";

    public static async Task<List<IbgeStateLookifyResultDto>> RequestAllAsync(
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/ibge/uf/v1";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Ibge");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<BrasilApiIbgeStateProviderResponse>>(
            providerName: ProviderName,
            identifier: "uf",
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult()).ToList();
    }

    public static async Task<IbgeStateLookifyResultDto> RequestByUfAsync(
        string uf,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/ibge/uf/v1/{uf}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Ibge");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<BrasilApiIbgeStateProviderResponse>(
            providerName: ProviderName,
            identifier: uf,
            response,
            cancellationToken);

        return payload.ToResult();
    }
}

internal sealed record class BrasilApiIbgeStateProviderResponse {

    [property: JsonPropertyName("id")]
    public int? Id { get; init; }

    [property: JsonPropertyName("sigla")]
    public string? Sigla { get; init; }

    [property: JsonPropertyName("nome")]
    public string? Nome { get; init; }

    [property: JsonPropertyName("regiao")]
    public IbgeRegiaoResponse? Regiao { get; init; }

    public IbgeStateLookifyResultDto ToResult() =>
        new IbgeStateLookifyResultDto {
            Id = Id,
            Name = Nome,
            Uf = Sigla,
            RegionId = Regiao?.Id,
            RegionName = Regiao?.Nome,
            RegionAcronym = Regiao?.Sigla
        };
}
