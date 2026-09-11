namespace Lookify.Providers;

internal interface IProvider {

    public static abstract string ProviderName { get; }

    public static abstract string BaseAddress { get; }

    public List<IProviderService> Services { get; }

    public void AddService(IProviderService service);

    public void RemoveService(IProviderService service);
}
