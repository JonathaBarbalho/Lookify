using System.Text.Json.Serialization;
using Lookify.Ibge;

namespace Lookify.Providers.Ibge;

internal static class IbgeCityProviderRequest {

    public static string ProviderName => "Ibge";

    public static string BaseAddress => "https://servicodados.ibge.gov.br/";

    public static async Task<List<IbgeCityLookifyResultDto>> RequestByStateAsync(
        string uf,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/v1/localidades/estados/{uf}/municipios";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<IbgeCityProviderResponse>>(
            providerName: ProviderName,
            identifier: uf,
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult()).ToList();
    }

    public static async Task<List<IbgeCityLookifyResultDto>> RequestAllAsync(
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/v1/localidades/municipios";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<IbgeCityProviderResponse>>(
            providerName: ProviderName,
            identifier: "municipios",
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult()).ToList();
    }
}

internal sealed record class IbgeCityProviderResponse {

    [property: JsonPropertyName("id")]
    public int? Id { get; init; }

    [property: JsonPropertyName("nome")]
    public string? Nome { get; init; }

    [property: JsonPropertyName("microrregiao")]
    public IbgeMicrorregiaoResponse? Microrregiao { get; init; }

    public IbgeCityLookifyResultDto ToResult() =>
        new IbgeCityLookifyResultDto {
            Id = Id,
            Name = Nome,
            StateId = Microrregiao?.Mesorregiao?.Uf?.Id,
            StateUf = Microrregiao?.Mesorregiao?.Uf?.Sigla,
            StateName = Microrregiao?.Mesorregiao?.Uf?.Nome,
            RegionId = Microrregiao?.Mesorregiao?.Uf?.Regiao?.Id,
            RegionName = Microrregiao?.Mesorregiao?.Uf?.Regiao?.Nome,
            RegionAcronym = Microrregiao?.Mesorregiao?.Uf?.Regiao?.Sigla
        };
}

internal sealed record class IbgeMicrorregiaoResponse {

    [property: JsonPropertyName("mesorregiao")]
    public IbgeMesorregiaoResponse? Mesorregiao { get; init; }
}

internal sealed record class IbgeMesorregiaoResponse {

    [property: JsonPropertyName("UF")]
    public IbgeUfResponse? Uf { get; init; }
}

internal sealed record class IbgeUfResponse {

    [property: JsonPropertyName("id")]
    public int? Id { get; init; }

    [property: JsonPropertyName("sigla")]
    public string? Sigla { get; init; }

    [property: JsonPropertyName("nome")]
    public string? Nome { get; init; }

    [property: JsonPropertyName("regiao")]
    public IbgeRegiaoResponse? Regiao { get; init; }
}
