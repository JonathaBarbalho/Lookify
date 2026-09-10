using System.Text.Json.Serialization;
using Lookify.Fipe;

namespace Lookify.Providers.BrasilApi;

internal static class BrasilApiFipeReferenceTableProviderRequest {

    public static string ProviderName => "BrasilApi";

    public static string BaseAddress => "https://brasilapi.com.br/";

    public static async Task<List<FipeReferenceTableLookifyResultDto>> RequestAsync(
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/fipe/tabelas/v1";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Fipe");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<BrasilApiFipeReferenceTableProviderResponse>>(
            providerName: ProviderName,
            identifier: "tabelas-referencia",
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult()).ToList();
    }
}

internal sealed record class BrasilApiFipeReferenceTableProviderResponse {

    [property: JsonPropertyName("codigo")]
    public int? Codigo { get; init; }

    [property: JsonPropertyName("mes")]
    public string? Mes { get; init; }

    public FipeReferenceTableLookifyResultDto ToResult() =>
        new FipeReferenceTableLookifyResultDto {
            Code = Codigo,
            Month = Mes
        };
}
