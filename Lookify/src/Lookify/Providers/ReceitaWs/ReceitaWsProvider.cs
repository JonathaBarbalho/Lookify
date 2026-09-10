namespace Lookify.Providers.ReceitaWs;

internal sealed class ReceitaWsProvider : IProvider {

    public static string ProviderName => "ReceitaWs";

    public static string BaseAddress => "https://receitaws.com.br/";

    public List<IProviderService> Services { get; private set; } = new();

    public ReceitaWsProvider()
    {
        AddService(new ReceitaWsCnpjService {
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
