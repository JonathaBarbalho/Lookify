namespace Lookify.Providers.Cptec;

internal sealed class CptecProvider : IProvider {

    public static string ProviderName => "Cptec";

    public static string BaseAddress => "https://brasilapi.com.br/";

    public List<IProviderService> Services { get; private set; } = new();

    public CptecProvider()
    {
        AddService(new CptecWeatherService {
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
