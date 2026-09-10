namespace Lookify.Providers.BrasilApi;

internal sealed class BrasilApiProvider : ProviderBase, IProvider {

    public static string ProviderName => "BrasilApi";

    public static string BaseAddress => "https://brasilapi.com.br/";

    public BrasilApiProvider()
    {
        RegisterService<BrasilApiCepService>(ProviderName, BaseAddress);
        RegisterService<BrasilApiCnpjService>(ProviderName, BaseAddress);
        RegisterService<BrasilApiBankService>(ProviderName, BaseAddress);
        RegisterService<BrasilApiHolidayService>(ProviderName, BaseAddress);
        RegisterService<BrasilApiFipeService>(ProviderName, BaseAddress);
        RegisterService<BrasilApiIbgeService>(ProviderName, BaseAddress);
    }
}
