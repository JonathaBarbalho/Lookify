using System.Text.Json.Serialization;
using Lookify.Fipe;

namespace Lookify.Providers.BrasilApi;

internal sealed partial class BrasilApiFipeService {

    public async Task<List<FipeBrandLookifyResultDto>> GetBrandsAsync(
        FipeVehicleType vehicleType,
        int? referenceTable,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var vehicleTypeSegment = vehicleType.ToPathSegment();
        var fullAddress = $"{BaseAddress}api/fipe/marcas/v1/{vehicleTypeSegment}";
        if (referenceTable is not null) {
            fullAddress += $"?tabela_referencia={referenceTable}";
        }

        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Fipe");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<BrasilApiFipeBrandProviderResponse>>(
            providerName: ProviderName,
            identifier: vehicleTypeSegment,
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult()).ToList();
    }
}

internal sealed record class BrasilApiFipeBrandProviderResponse {

    [property: JsonPropertyName("nome")]
    public string? Nome { get; init; }

    [property: JsonPropertyName("valor")]
    public string? Valor { get; init; }

    public FipeBrandLookifyResultDto ToResult() =>
        new FipeBrandLookifyResultDto {
            Code = Valor,
            Name = Nome
        };
}
