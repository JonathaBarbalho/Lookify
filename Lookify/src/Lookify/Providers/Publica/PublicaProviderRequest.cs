using Lookify.Cnpj;

namespace Lookify.Providers.Publica;

internal sealed class PublicaProviderRequest : IProviderRequest<CnpjLookifyResultDto> {

    public static string ProviderName => "Publica";

    public static string BaseAddress => "https://publica.cnpj.ws/";

    public static async Task<CnpjLookifyResultDto> RequestAsync(
        string identifier,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}cnpj/{identifier}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<PublicaProviderResponse>(
            providerName: ProviderName,
            identifier,
            response,
            cancellationToken);

        return payload.ToResult();
    }
}
