namespace Lookify.Providers.Publica;

internal sealed class PublicaProvider : IProvider {

    public static string ProviderName => "Publica";

    public static string BaseAddress => "https://publica.cnpj.ws/";

    public List<IProviderService> Services { get; private set; } = new();

    public PublicaProvider()
    {
        AddService(new PublicaCnpjService {
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
