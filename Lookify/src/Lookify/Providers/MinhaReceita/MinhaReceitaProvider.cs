namespace Lookify.Providers.MinhaReceita;

internal sealed class MinhaReceitaProvider : IProvider {

    public static string ProviderName => "MinhaReceita";

    public static string BaseAddress => "https://minhareceita.org/";

    public List<IProviderService> Services { get; private set; } = new();

    public MinhaReceitaProvider()
    {
        AddService(new MinhaReceitaCnpjService {
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
