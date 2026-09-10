using System.Text.Json.Serialization;
using Lookify.Fipe;

namespace Lookify.Providers.BrasilApi;

internal sealed partial class BrasilApiFipeService {

    public async Task<List<FipeReferenceTableLookifyResultDto>> GetReferenceTablesAsync(
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
