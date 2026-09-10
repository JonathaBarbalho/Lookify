namespace Lookify.Providers.PlacaFipe;

internal sealed class PlacaFipeProvider : IProvider {

    public static string ProviderName => "PlacaFipe";

    public static string BaseAddress => "https://api.placafipe.com.br/";

    public List<IProviderService> Services { get; private set; } = new();

    public PlacaFipeProvider()
    {
        AddService(new PlacaFipeVehiclePlateService {
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
