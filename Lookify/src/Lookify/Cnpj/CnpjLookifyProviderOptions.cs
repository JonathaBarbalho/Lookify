using Lookify.Providers;

namespace Lookify.Cnpj;

public sealed class CnpjLookifyProviderOptions(
        string baseAddress) : IProviderEnablement {

    public bool IsEnabled { get; set; } = true;

    public string BaseAddress { get; init; } = baseAddress;

    public void UpdateEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}
