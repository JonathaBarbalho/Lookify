namespace Lookify.Providers.BrasilApi;

internal sealed class BrasilApiProvider : IProvider {

    public static string ProviderName => "BrasilApi";

    public static string BaseAddress => "https://brasilapi.com.br/";

    public List<IProviderService> Services { get; private set; } = new();

    public BrasilApiProvider()
    {
        AddService(new BrasilApiCepService {
            ProviderName = ProviderName,
            BaseAddress = BaseAddress
        });
        AddService(new BrasilApiCnpjService {
            ProviderName = ProviderName,
            BaseAddress = BaseAddress
        });
        AddService(new BrasilApiBankService {
            ProviderName = ProviderName,
            BaseAddress = BaseAddress
        });
        AddService(new BrasilApiHolidayService {
            ProviderName = ProviderName,
            BaseAddress = BaseAddress
        });
        AddService(new BrasilApiFipeService {
            ProviderName = ProviderName,
            BaseAddress = BaseAddress
        });
        AddService(new BrasilApiIbgeService {
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
