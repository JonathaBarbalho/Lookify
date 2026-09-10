namespace Lookify.Providers.ReceitaWs;

internal sealed class ReceitaWsProvider : ProviderBase, IProvider {

    public static string ProviderName => "ReceitaWs";

    public static string BaseAddress => "https://receitaws.com.br/";

    public ReceitaWsProvider() =>
        RegisterService<ReceitaWsCnpjService>(ProviderName, BaseAddress);
}
