namespace Lookify.Providers.OpenMeteo;

internal sealed class OpenMeteoProvider : IProvider {

    public static string ProviderName => "OpenMeteo";

    public static string BaseAddress => "https://api.open-meteo.com/";

    public List<IProviderService> Services { get; private set; } = new();

    public OpenMeteoProvider()
    {
        AddService(new OpenMeteoWeatherService {
            ProviderName = ProviderName,
            BaseAddress = BaseAddress
        });
    }

    public void AddService(IProviderService service)
    {
        Services.Add(service);
    }

    public void RemoveService(IProviderService service)
    {
        Services.Remove(service);
    }
}
