using Lookify.Providers;

namespace Lookify.Cep;

public sealed class CepLookifyProviderOptions(
        string baseAddress) : IProviderOptions {

    public bool IsEnabled { get; set; } = true;

    public string BaseAddress { get; init; } = baseAddress;

    public void UpdateEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}