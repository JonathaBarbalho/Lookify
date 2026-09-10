using Lookify.Cnpj;

namespace Lookify.Providers.ReceitaWs;

internal sealed class ReceitaWsProviderRequest : IProviderRequest<CnpjLookifyResultDto> {

    public static string ProviderName => "ReceitaWs";

    public static string BaseAddress => "https://receitaws.com.br/";

    public static async Task<CnpjLookifyResultDto> RequestAsync(
        string identifier,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}v1/cnpj/{identifier}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<ReceitaWsProviderResponse>(
            providerName: ProviderName,
            identifier,
            response,
            cancellationToken);

        return payload.ToResult();
    }
}
