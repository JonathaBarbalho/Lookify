using Lookify.Providers;

namespace Lookify.Bank;

public sealed class BankLookifyProviderOptions(
        string baseAddress) : IProviderOptions {

    public bool IsEnabled { get; set; } = true;

    public string BaseAddress { get; init; } = baseAddress;

    public void UpdateEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
    }
}
