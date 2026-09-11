using Lookify.Providers;

namespace Lookify.Fipe;

public sealed class FipeLookifyProviderOptions(
        string baseAddress) : IProviderEnablement {

    public bool IsEnabled { get; set; } = true;

    public string BaseAddress { get; init; } = baseAddress;

    public void UpdateEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}
