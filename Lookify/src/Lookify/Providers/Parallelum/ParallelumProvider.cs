namespace Lookify.Providers.Parallelum;

internal sealed class ParallelumProvider : IProvider {

    public static string ProviderName => "Parallelum";

    public static string BaseAddress => "https://fipe.parallelum.com.br/";

    public List<IProviderService> Services { get; private set; } = new();

    public ParallelumProvider()
    {
        AddService(new ParallelumFipeService {
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
