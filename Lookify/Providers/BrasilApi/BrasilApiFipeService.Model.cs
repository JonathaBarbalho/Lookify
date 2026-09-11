using System.Text.Json.Serialization;
using Lookify.Fipe;

namespace Lookify.Providers.BrasilApi;

internal sealed partial class BrasilApiFipeService {

    public async Task<List<FipeModelLookifyResultDto>> GetModelsAsync(
        FipeVehicleType vehicleType,
        string brandCode,
        int? referenceTable,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var vehicleTypeSegment = vehicleType.ToPathSegment();
        var fullAddress = $"{BaseAddress}api/fipe/veiculos/v1/{vehicleTypeSegment}/{brandCode}";
        if (referenceTable is not null) {
            fullAddress += $"?tabela_referencia={referenceTable}";
        }

        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Fipe");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<BrasilApiFipeModelProviderResponse>>(
            providerName: ProviderName,
            identifier: brandCode,
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult()).ToList();
    }
}

internal sealed record class BrasilApiFipeModelProviderResponse {

    [property: JsonPropertyName("modelo")]
    public string? Modelo { get; init; }

    [property: JsonPropertyName("valor")]
    public string? Valor { get; init; }

    public FipeModelLookifyResultDto ToResult() =>
        new FipeModelLookifyResultDto {
            Code = Valor,
            Name = Modelo
        };
}
