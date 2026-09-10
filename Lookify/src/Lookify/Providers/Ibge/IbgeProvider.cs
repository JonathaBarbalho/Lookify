namespace Lookify.Providers.Ibge;

internal sealed class IbgeProvider : IProvider {

    public static string ProviderName => "Ibge";

    public static string BaseAddress => "https://servicodados.ibge.gov.br/";

    public List<IProviderService> Services { get; private set; } = new();

    public IbgeProvider()
    {
        AddService(new IbgeService {
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
