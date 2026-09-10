namespace Lookify.Bank;

public sealed class BankLookifyProviderOptions {
    public bool Enabled { get; set; } = true;
    public string BaseAddress { get; init; } = string.Empty;
}
