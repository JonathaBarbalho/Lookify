namespace Lookify.Providers.PlacaFipe;

internal sealed class PlacaFipeProvider : ProviderBase, IProvider {

    public static string ProviderName => "PlacaFipe";

    public static string BaseAddress => "https://api.placafipe.com.br/";

    public PlacaFipeProvider() =>
        RegisterService<PlacaFipeVehiclePlateService>(ProviderName, BaseAddress);
}
