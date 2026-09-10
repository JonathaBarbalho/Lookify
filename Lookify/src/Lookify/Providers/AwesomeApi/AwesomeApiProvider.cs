namespace Lookify.Providers.AwesomeApi;

internal sealed class AwesomeApiProvider : IProvider {

    public static string ProviderName => "AwesomeApi";

    public static string BaseAddress => "https://cep.awesomeapi.com.br/";

    public List<IProviderService> Services { get; private set; } = new();

    public AwesomeApiProvider()
    {
        AddService(new AwesomeApiCepService {
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
