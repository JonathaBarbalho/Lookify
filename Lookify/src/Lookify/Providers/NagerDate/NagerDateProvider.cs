namespace Lookify.Providers.NagerDate;

internal sealed class NagerDateProvider : ProviderBase, IProvider {

    public static string ProviderName => "NagerDate";

    public static string BaseAddress => "https://date.nager.at/";

    public NagerDateProvider() =>
        RegisterService<NagerDateHolidayService>(ProviderName, BaseAddress);
}
