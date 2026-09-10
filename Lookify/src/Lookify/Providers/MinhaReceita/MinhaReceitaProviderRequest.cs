using Lookify.Cnpj;

namespace Lookify.Providers.MinhaReceita;

internal sealed class MinhaReceitaProviderRequest : IProviderRequest<CnpjLookifyResultDto> {

    public static string ProviderName => "MinhaReceita";

    public static string BaseAddress => "https://minhareceita.org/";

    public static async Task<CnpjLookifyResultDto> RequestAsync(
        string identifier,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}{identifier}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<MinhaReceitaProviderResponse>(
            providerName: ProviderName,
            identifier,
            response,
            cancellationToken);

        return payload.ToResult();
    }
}
