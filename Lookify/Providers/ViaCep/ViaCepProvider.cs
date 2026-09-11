namespace Lookify.Providers.ViaCep;

internal sealed class ViaCepProvider : ProviderBase, IProvider {

    public static string ProviderName => "ViaCep";

    public static string BaseAddress => "https://viacep.com.br/";

    public ViaCepProvider() =>
        RegisterService<ViaCepCepService>(ProviderName, BaseAddress);
}
