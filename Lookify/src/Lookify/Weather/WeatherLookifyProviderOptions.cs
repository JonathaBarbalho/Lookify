using Lookify.Providers;

namespace Lookify.Weather;

public sealed class WeatherLookifyProviderOptions(
        string baseAddress) : IProviderEnablement {

    public bool IsEnabled { get; set; } = true;

    public string BaseAddress { get; init; } = baseAddress;

    public void UpdateEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}
