using System.Text.Json.Serialization;
using Lookify.Ibge;

namespace Lookify.Providers.Ibge;

internal sealed partial class IbgeService {

    public async Task<List<IbgeStateLookifyResultDto>> GetStatesAsync(
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/v1/localidades/estados";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<IbgeStateProviderResponse>>(
            providerName: ProviderName,
            identifier: "estados",
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult()).ToList();
    }

    public async Task<IbgeStateLookifyResultDto> GetStateAsync(
        string uf,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/v1/localidades/estados/{uf}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<IbgeStateProviderResponse>(
            providerName: ProviderName,
            identifier: uf,
            response,
            cancellationToken);

        return payload.ToResult();
    }
}

internal sealed record class IbgeStateProviderResponse {

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

internal sealed record class IbgeRegiaoResponse {

    [property: JsonPropertyName("id")]
    public int? Id { get; init; }

    [property: JsonPropertyName("sigla")]
    public string? Sigla { get; init; }

    [property: JsonPropertyName("nome")]
    public string? Nome { get; init; }
}
