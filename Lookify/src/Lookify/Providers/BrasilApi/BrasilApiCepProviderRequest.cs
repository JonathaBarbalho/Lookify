using Lookify.Cep;

namespace Lookify.Providers.BrasilApi;

internal sealed class BrasilApiCepProviderRequest : IProviderRequest<CepLookifyResultDto> {

    public static string ProviderName => "BrasilApi";

    public static string BaseAddress => "https://brasilapi.com.br/";

    public static async Task<CepLookifyResultDto> RequestAsync(
        string identifier,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/cep/v2/{identifier}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<BrasilApiCepProviderResponse>(
            providerName: ProviderName,
            identifier,
            response,
            cancellationToken);

        return payload.ToResult();
    }
}
