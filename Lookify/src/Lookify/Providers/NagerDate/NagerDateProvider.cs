namespace Lookify.Providers.NagerDate;

internal sealed class NagerDateProvider : IProvider {

    public static string ProviderName => "NagerDate";

    public static string BaseAddress => "https://date.nager.at/";

    public List<IProviderService> Services { get; private set; } = new();

    public NagerDateProvider()
    {
        AddService(new NagerDateHolidayService {
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
