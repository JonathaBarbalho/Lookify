namespace Lookify.Providers.Ibge;

internal sealed class IbgeProvider : ProviderBase, IProvider {

    public static string ProviderName => "Ibge";

    public static string BaseAddress => "https://servicodados.ibge.gov.br/";

    public IbgeProvider() =>
        RegisterService<IbgeService>(ProviderName, BaseAddress);
}
