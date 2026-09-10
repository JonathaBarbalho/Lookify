namespace Lookify.Holiday;

public sealed class HolidayLookifyProviderOptions {
    public bool Enabled { get; set; } = true;
    public string BaseAddress { get; init; } = string.Empty;
}
