using Lookify.Cep;

namespace Lookify.Providers.AwesomeApi;

internal sealed class AwesomeApiCepProviderRequest : IProviderRequest<CepLookifyResultDto> {

    public static string ProviderName => "AwesomeApi";

    public static string BaseAddress => "https://cep.awesomeapi.com.br/";

    public static async Task<CepLookifyResultDto> RequestAsync(
        string identifier,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}json/{identifier}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<AwesomeApiCepProviderResponse>(
            providerName: ProviderName,
            identifier,
            response,
            cancellationToken);

        return payload.ToResult();
    }
}
