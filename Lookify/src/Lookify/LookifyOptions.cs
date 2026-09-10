using Lookify.Cep;
using Lookify.Cnpj;
using Lookify.Providers.BrasilApi;
using Lookify.Providers.Publica;
using Lookify.Providers.ReceitaWs;
using Lookify.Providers.ViaCep;

namespace Lookify;

public sealed class LookifyOptions {

    /// <summary>
    /// Name of the software who will use this library. This is used to identify the software in the user agent string.
    /// </summary>
    public string UserAgent { get; set; } = "Lookify/1.0";

    public TimeSpan TimeOut { get; set; } = TimeSpan.FromMinutes(3);

    #region CEP
    public CepLookifyProviderOptions ViaCep { get; set; } = new() {
        BaseAddress = ViaCepProviderRequest.BaseAddress
    };

    public CepLookifyProviderOptions BrasilApi { get; set; } = new() {
        BaseAddress = BrasilApiCepProviderRequest.BaseAddress
    };

    public List<CepLookifyProviderEnum> CepProviders { get; set; } = new() {
        CepLookifyProviderEnum.ViaCep,
        CepLookifyProviderEnum.BrasilApi
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
    #endregion

    #region CNPJ
    public CnpjLookifyProviderOptions CnpjBrasilApi { get; set; } = new() {
        BaseAddress = BrasilApiCnpjProviderRequest.BaseAddress
    };

    public CnpjLookifyProviderOptions CnpjReceitaWs { get; set; } = new() {
        BaseAddress = ReceitaWsProviderRequest.BaseAddress
    };

    public CnpjLookifyProviderOptions CnpjPublica { get; set; } = new() {
        BaseAddress = PublicaProviderRequest.BaseAddress
    };

    public List<CnpjLookifyProviderEnum> CnpjProviders { get; set; } = new() {
        CnpjLookifyProviderEnum.BrasilApi,
        CnpjLookifyProviderEnum.ReceitaWs,
        CnpjLookifyProviderEnum.Publica
    };

    internal CnpjLookifyProviderOptions GetProviderOptions(
        CnpjLookifyProviderEnum provider)
    {
        return provider switch {
            CnpjLookifyProviderEnum.BrasilApi => CnpjBrasilApi,
            CnpjLookifyProviderEnum.ReceitaWs => CnpjReceitaWs,
            CnpjLookifyProviderEnum.Publica => CnpjPublica,
            _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null)
        };
    }
    #endregion
}
