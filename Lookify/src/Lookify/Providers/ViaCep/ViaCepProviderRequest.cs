using Lookify.Cep;

namespace Lookify.Providers.ViaCep;

internal sealed class ViaCepProviderRequest : IProviderRequest<CepLookifyResultDto> {

    public static string ProviderName => "ViaCep";

    public static string BaseAddress => "https://viacep.com.br/";

    public static async Task<CepLookifyResultDto> RequestAsync(
        string identifier,
        IHttpClientFactory httpFactory,
        CancellationToken cancellationToken = default)
    {
        var fullAddress = $"{BaseAddress}ws/{identifier}/json/";
        var client = httpFactory.CreateClient($"Lookify.{ProviderName}");
        var response = await client.GetAsync(
            fullAddress,
            cancellationToken);
        var payload = await ProviderRequestDeserialization.ReadAndDeserializeAsync<ViaCepProviderResponse>(
            providerName: ProviderName,
            identifier,
            response,
            cancellationToken);

        if (payload.Erro)
            throw new InvalidOperationException($"{ProviderName} não encontrou o CEP {identifier}.");

        return payload.ToResult();
    }
}
