namespace Lookify.Providers.Cptec;

internal sealed class CptecProvider : ProviderBase, IProvider {

    public static string ProviderName => "Cptec";

    public static string BaseAddress => "https://brasilapi.com.br/";

    public CptecProvider() =>
        RegisterService<CptecWeatherService>(ProviderName, BaseAddress);
}
