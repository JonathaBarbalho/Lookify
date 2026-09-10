using System.Text.Json.Serialization;
using Lookify.Fipe;

namespace Lookify.Providers.BrasilApi;

internal sealed partial class BrasilApiFipeService {

    public async Task<List<FipeModelYearLookifyResultDto>> GetModelYearsAsync(
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
