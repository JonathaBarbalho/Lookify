namespace Lookify.Weather;

public sealed class WeatherLookifyProviderOptions {
    public bool Enabled { get; set; } = true;
    public string BaseAddress { get; init; } = string.Empty;
}
