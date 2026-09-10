using Lookify.Cep;

namespace Lookify.Providers.OpenCep;

internal sealed class OpenCepProviderRequest : IProviderRequest<CepLookifyResultDto> {

    public static string ProviderName => "OpenCep";

    public static string BaseAddress => "https://opencep.com/";

    public static async Task<CepLookifyResultDto> RequestAsync(
        string identifier,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}v1/{identifier}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<OpenCepProviderResponse>(
            providerName: ProviderName,
            identifier,
            response,
            cancellationToken);

        return payload.ToResult();
    }
}
