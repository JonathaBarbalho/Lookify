using Lookify.Cep.Options;
using Lookify.Cep.Providers.BrasilApi;
using Lookify.Cep.Providers.ViaCep;

namespace Lookify;

public class LookifyOptions {

    /// <summary>
    /// Name of the software who will use this library. This is used to identify the software in the user agent string.
    /// </summary>
    public string UserAgent { get; set; } = "Lookify/1.0";

    public TimeSpan TimeOut { get; set; } = TimeSpan.FromMinutes(3);

    public List<CepLookifyProviderEnum> CepProviders { get; set; } = new() {
        CepLookifyProviderEnum.ViaCep,
        CepLookifyProviderEnum.BrasilApi
    };

    public CepLookifyProviderOptions ViaCep { get; set; } = new() {
        BaseAddress = ViaCepProviderRequest.BaseAddress
    };

    public CepLookifyProviderOptions BrasilApi { get; set; } = new() {
        BaseAddress = BrasilApiProviderRequest.BaseAddress
    };

    internal CepLookifyProviderOptions GetProviderOptions(
        CepLookifyProviderEnum provider)
    {
        return provider switch {
            CepLookifyProviderEnum.ViaCep => ViaCep,
            CepLookifyProviderEnum.BrasilApi => BrasilApi,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }
}
