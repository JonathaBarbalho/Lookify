using Lookify.Providers;

namespace Lookify.Holiday;

public sealed class HolidayLookifyProviderOptions(
        string baseAddress) : IProviderEnablement {

    public bool IsEnabled { get; set; } = true;

    public string BaseAddress { get; init; } = baseAddress;

    public void UpdateEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}
