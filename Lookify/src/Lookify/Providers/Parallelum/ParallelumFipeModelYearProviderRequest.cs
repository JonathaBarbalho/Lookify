using System.Text.Json.Serialization;
using Lookify.Fipe;

namespace Lookify.Providers.Parallelum;

internal static class ParallelumFipeModelYearProviderRequest {

    public static string ProviderName => "Parallelum";

    public static string BaseAddress => "https://fipe.parallelum.com.br/";

    public static async Task<List<FipeModelYearLookifyResultDto>> RequestAsync(
        FipeVehicleType vehicleType,
        string brandCode,
        string modelCode,
        int? referenceTable,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var vehicleTypeSegment = vehicleType.ToParallelumPathSegment();
        var fullAddress = $"{BaseAddress}api/v2/{vehicleTypeSegment}/brands/{brandCode}/models/{modelCode}/years";
        if (referenceTable is not null) {
            fullAddress += $"?reference={referenceTable}";
        }

        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Fipe");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<ParallelumFipeModelYearProviderResponse>>(
            providerName: ProviderName,
            identifier: modelCode,
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult()).ToList();
    }
}

internal sealed record class ParallelumFipeModelYearProviderResponse {

    [property: JsonPropertyName("code")]
    public string? Code { get; init; }

    [property: JsonPropertyName("name")]
    public string? Name { get; init; }

    public FipeModelYearLookifyResultDto ToResult() =>
        new FipeModelYearLookifyResultDto {
            Code = Code,
            Label = Name
        };
}
