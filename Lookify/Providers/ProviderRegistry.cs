using Lookify.Providers.AwesomeApi;
using Lookify.Providers.BrasilApi;
using Lookify.Providers.Cptec;
using Lookify.Providers.Ibge;
using Lookify.Providers.MinhaReceita;
using Lookify.Providers.NagerDate;
using Lookify.Providers.OpenCep;
using Lookify.Providers.OpenMeteo;
using Lookify.Providers.Parallelum;
using Lookify.Providers.PlacaFipe;
using Lookify.Providers.Publica;
using Lookify.Providers.ReceitaWs;
using Lookify.Providers.ViaCep;

namespace Lookify.Providers;

internal static class ProviderRegistry {

    public static ViaCepProvider ViaCep { get; } = new();

    public static BrasilApiProvider BrasilApi { get; } = new();

    public static OpenCepProvider OpenCep { get; } = new();

    public static AwesomeApiProvider AwesomeApi { get; } = new();

    public static ReceitaWsProvider ReceitaWs { get; } = new();

    public static PublicaProvider Publica { get; } = new();

    public static MinhaReceitaProvider MinhaReceita { get; } = new();

    public static PlacaFipeProvider PlacaFipe { get; } = new();

    public static ParallelumProvider Parallelum { get; } = new();

    public static IbgeProvider Ibge { get; } = new();

    public static NagerDateProvider NagerDate { get; } = new();

    public static OpenMeteoProvider OpenMeteo { get; } = new();

    public static CptecProvider Cptec { get; } = new();
}
