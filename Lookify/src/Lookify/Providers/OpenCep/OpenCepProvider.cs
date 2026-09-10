namespace Lookify.Providers.OpenCep;

internal sealed class OpenCepProvider : ProviderBase, IProvider {

    public static string ProviderName => "OpenCep";

    public static string BaseAddress => "https://opencep.com/";

    public OpenCepProvider() =>
        RegisterService<OpenCepCepService>(ProviderName, BaseAddress);
}
