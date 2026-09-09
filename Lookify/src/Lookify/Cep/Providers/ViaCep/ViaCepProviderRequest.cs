using Lookify.Cep.Dto;
using Lookify.Cep.Providers.BrasilApi;

namespace Lookify.Cep.Providers.ViaCep;

internal class ViaCepProviderRequest : IProviderRequest {
    
    public static string ProviderName => "ViaCep";

    public static string BaseAddress => "https://viacep.com.br/";

    public static async Task<CepLookifyResult> RequestAsync(
        string zipCode,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}/ws/{zipCode}/json/";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<ViaCepResponse>(
            providerName: ProviderName,
            zipCode,
            response,
            cancellationToken);

        if (payload.Erro)
            throw new InvalidOperationException($"{ProviderName} não encontrou o CEP {zipCode}.");

        return payload.ToResult();
    }
}