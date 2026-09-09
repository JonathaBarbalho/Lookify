using Lookify.Cep.Dto;
using Lookify.Cep.Options;
using Lookify.Cep.Providers.ViaCep;

namespace Lookify.Cep.Providers.BrasilApi;

internal class BrasilApiProviderRequest : IProviderRequest {

    public static string ProviderName => "BrasilApi";

    public static string BaseAddress => "https://brasilapi.com.br/";
    
    public static async Task<CepLookifyResult> RequestAsync(
        string zipCode,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}/api/cep/v2/{zipCode}";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<BrasilApiResponse>(
            providerName: ProviderName,
            zipCode,
            response,
            cancellationToken);

        return payload.ToResult();
    }
}