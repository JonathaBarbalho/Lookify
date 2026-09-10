namespace Lookify.Providers.ViaCep;

internal sealed class ViaCepProvider : IProvider {

    public static string ProviderName => "ViaCep";

    public static string BaseAddress => "https://viacep.com.br/";

    public List<IProviderService> Services { get; private set; } = new();

    public ViaCepProvider()
    {
        AddService(new ViaCepCepService {
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
