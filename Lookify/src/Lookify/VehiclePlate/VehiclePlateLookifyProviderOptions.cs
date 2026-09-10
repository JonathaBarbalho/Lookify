using Lookify.Providers;

namespace Lookify.VehiclePlate;

public sealed class VehiclePlateLookifyProviderOptions(
        string baseAddress) : IProviderOptions {

    public bool IsEnabled { get; set; } = true;

    public string BaseAddress { get; init; } = baseAddress;

    public string Token { get; set; } = string.Empty;

    public void UpdateEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}
