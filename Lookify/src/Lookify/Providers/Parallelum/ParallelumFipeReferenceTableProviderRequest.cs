using System.Text.Json.Serialization;
using Lookify.Fipe;

namespace Lookify.Providers.Parallelum;

internal static class ParallelumFipeReferenceTableProviderRequest {

    public static string ProviderName => "Parallelum";

    public static string BaseAddress => "https://fipe.parallelum.com.br/";

    public static async Task<List<FipeReferenceTableLookifyResultDto>> RequestAsync(
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/v2/references";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Fipe");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<List<ParallelumFipeReferenceTableProviderResponse>>(
            providerName: ProviderName,
            identifier: "references",
            response,
            cancellationToken);

        return payload.Select(item => item.ToResult()).ToList();
    }
}

internal sealed record class ParallelumFipeReferenceTableProviderResponse {

    [property: JsonPropertyName("code")]
    public int? Code { get; init; }

    [property: JsonPropertyName("month")]
    public string? Month { get; init; }

    public FipeReferenceTableLookifyResultDto ToResult() =>
        new FipeReferenceTableLookifyResultDto {
            Code = Code,
            Month = Month
        };
}
