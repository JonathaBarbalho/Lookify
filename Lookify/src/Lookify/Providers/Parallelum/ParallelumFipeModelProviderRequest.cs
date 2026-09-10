using System.Text.Json.Serialization;
using Lookify.Fipe;

namespace Lookify.Providers.Parallelum;

internal static class ParallelumFipeModelProviderRequest {

    public static string ProviderName => "Parallelum";

    public static string BaseAddress => "https://fipe.parallelum.com.br/";

    public static async Task<List<FipeModelLookifyResultDto>> RequestAsync(
        FipeVehicleType vehicleType,
        string brandCode,
        int? referenceTable,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var vehicleTypeSegment = vehicleType.ToParallelumPathSegment();
        var fullAddress = $"{BaseAddress}api/v2/{vehicleTypeSegment}/brands/{brandCode}/models";
        if (referenceTable is not null) {
            fullAddress += $"?reference={referenceTable}";
        }

        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Fipe");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<ParallelumFipeModelProviderResponse>>(
            providerName: ProviderName,
            identifier: brandCode,
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult()).ToList();
    }
}

internal sealed record class ParallelumFipeModelProviderResponse {

    [property: JsonPropertyName("code")]
    public string? Code { get; init; }

    [property: JsonPropertyName("name")]
    public string? Name { get; init; }

    public FipeModelLookifyResultDto ToResult() =>
        new FipeModelLookifyResultDto {
            Code = Code,
            Name = Name
        };
}
