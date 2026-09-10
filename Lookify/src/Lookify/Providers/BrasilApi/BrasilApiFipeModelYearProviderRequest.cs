using System.Text.Json.Serialization;
using Lookify.Fipe;

namespace Lookify.Providers.BrasilApi;

internal static class BrasilApiFipeModelYearProviderRequest {

    public static string ProviderName => "BrasilApi";

    public static string BaseAddress => "https://brasilapi.com.br/";

    public static async Task<List<FipeModelYearLookifyResultDto>> RequestAsync(
        FipeVehicleType vehicleType,
        string brandCode,
        string modelCode,
        int? referenceTable,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var vehicleTypeSegment = vehicleType.ToPathSegment();
        var fullAddress = $"{BaseAddress}api/fipe/anos/v1/{vehicleTypeSegment}/{brandCode}/{modelCode}";
        if (referenceTable is not null) {
            fullAddress += $"?tabela_referencia={referenceTable}";
        }

        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Fipe");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<BrasilApiFipeModelYearProviderResponse>>(
            providerName: ProviderName,
            identifier: modelCode,
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult()).ToList();
    }
}

internal sealed record class BrasilApiFipeModelYearProviderResponse {

    [property: JsonPropertyName("nome")]
    public string? Nome { get; init; }

    [property: JsonPropertyName("valor")]
    public string? Valor { get; init; }

    public FipeModelYearLookifyResultDto ToResult() =>
        new FipeModelYearLookifyResultDto {
            Code = Valor,
            Label = Nome
        };
}
