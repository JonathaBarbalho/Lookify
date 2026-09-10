namespace Lookify.Providers;

internal abstract class ProviderBase {

    public List<IProviderService> Services { get; } = new();

    public void AddService(IProviderService service) =>
        Services.Add(service);

    public void RemoveService(IProviderService service) =>
        Services.Remove(service);

    protected void RegisterService<TService>(
        string providerName,
        string baseAddress) where TService : IProviderService, new() =>
        AddService(new TService {
            ProviderName = providerName,
            BaseAddress = baseAddress
        });
}
