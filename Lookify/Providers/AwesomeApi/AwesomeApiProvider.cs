namespace Lookify.Providers.AwesomeApi;

internal sealed class AwesomeApiProvider : ProviderBase, IProvider {

    public static string ProviderName => "AwesomeApi";

    public static string BaseAddress => "https://cep.awesomeapi.com.br/";

    public AwesomeApiProvider() =>
        RegisterService<AwesomeApiCepService>(ProviderName, BaseAddress);
}
