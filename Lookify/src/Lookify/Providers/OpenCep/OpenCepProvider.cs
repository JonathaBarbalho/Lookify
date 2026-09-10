namespace Lookify.Providers.OpenCep;

internal sealed class OpenCepProvider : IProvider {

    public static string ProviderName => "OpenCep";

    public static string BaseAddress => "https://opencep.com/";

    public List<IProviderService> Services { get; private set; } = new();

    public OpenCepProvider()
    {
        AddService(new OpenCepCepService {
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
