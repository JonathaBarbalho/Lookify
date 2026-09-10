using Lookify.Providers;

namespace Lookify.Ibge;

public sealed class IbgeLookifyProviderOptions(
        string baseAddress) : IProviderEnablement {

    public bool IsEnabled { get; set; } = true;

    public string BaseAddress { get; init; } = baseAddress;

    public void UpdateEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}
