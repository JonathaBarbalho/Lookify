using System.Text.Json.Serialization;
using Lookify.Fipe;

namespace Lookify.Providers.Parallelum;

internal sealed partial class ParallelumFipeService {

    public async Task<List<FipeBrandLookifyResultDto>> GetBrandsAsync(
        FipeVehicleType vehicleType,
        int? referenceTable,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var vehicleTypeSegment = vehicleType.ToParallelumPathSegment();
        var fullAddress = $"{BaseAddress}api/v2/{vehicleTypeSegment}/brands";
        if (referenceTable is not null) {
            fullAddress += $"?reference={referenceTable}";
        }

        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Fipe");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<ParallelumFipeBrandProviderResponse>>(
            providerName: ProviderName,
            identifier: vehicleTypeSegment,
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult()).ToList();
    }
}

internal sealed record class ParallelumFipeBrandProviderResponse {

    [property: JsonPropertyName("code")]
    public string? Code { get; init; }

    [property: JsonPropertyName("name")]
    public string? Name { get; init; }

    public FipeBrandLookifyResultDto ToResult() =>
        new FipeBrandLookifyResultDto {
            Code = Code,
            Name = Name
        };
}
