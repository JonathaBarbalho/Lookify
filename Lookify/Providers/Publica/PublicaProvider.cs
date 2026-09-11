namespace Lookify.Providers.Publica;

internal sealed class PublicaProvider : ProviderBase, IProvider {

    public static string ProviderName => "Publica";

    public static string BaseAddress => "https://publica.cnpj.ws/";

    public PublicaProvider() =>
        RegisterService<PublicaCnpjService>(ProviderName, BaseAddress);
}
