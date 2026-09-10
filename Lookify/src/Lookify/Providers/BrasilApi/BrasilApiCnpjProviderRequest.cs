using Lookify.Cnpj;

namespace Lookify.Providers.BrasilApi;

internal sealed class BrasilApiCnpjProviderRequest : IProviderRequest<CnpjLookifyResultDto> {

    public static string ProviderName => "BrasilApi";

    public static string BaseAddress => "https://brasilapi.com.br/";

    public static async Task<CnpjLookifyResultDto> RequestAsync(
        string identifier,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}api/cnpj/v1/{identifier}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}.Cnpj");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<BrasilApiCnpjProviderResponse>(
            providerName: ProviderName,
            identifier,
            response,
            cancellationToken);

        return payload.ToResult();
    }
}
