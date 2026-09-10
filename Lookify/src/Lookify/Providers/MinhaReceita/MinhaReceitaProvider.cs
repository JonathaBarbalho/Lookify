namespace Lookify.Providers.MinhaReceita;

internal sealed class MinhaReceitaProvider : ProviderBase, IProvider {

    public static string ProviderName => "MinhaReceita";

    public static string BaseAddress => "https://minhareceita.org/";

    public MinhaReceitaProvider() =>
        RegisterService<MinhaReceitaCnpjService>(ProviderName, BaseAddress);
}
