using Lookify.Cep;

namespace Lookify.Providers.ViaCep;

internal sealed class ViaCepCepService : ICepProviderService {

    public string ProviderName { get; init; } = string.Empty;

    public string BaseAddress { get; init; } = string.Empty;

    public bool IsEnabled { get; private set; }

    public void UpdateEnabled(bool isEnabled) =>
        IsEnabled = isEnabled;

    public async Task<CepLookifyResultDto> RequestAsync(
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
