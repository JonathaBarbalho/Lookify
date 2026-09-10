namespace Lookify.Providers.Parallelum;

internal sealed class ParallelumProvider : ProviderBase, IProvider {

    public static string ProviderName => "Parallelum";

    public static string BaseAddress => "https://fipe.parallelum.com.br/";

    public ParallelumProvider() =>
        RegisterService<ParallelumFipeService>(ProviderName, BaseAddress);
}
