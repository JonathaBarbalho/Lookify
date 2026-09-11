namespace Lookify.Providers.OpenMeteo;

internal sealed class OpenMeteoProvider : ProviderBase, IProvider {

    public static string ProviderName => "OpenMeteo";

    public static string BaseAddress => "https://api.open-meteo.com/";

    public OpenMeteoProvider() =>
        RegisterService<OpenMeteoWeatherService>(ProviderName, BaseAddress);
}
