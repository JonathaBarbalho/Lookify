namespace Lookify.Cnpj;

public sealed class CnpjLookifyProviderOptions {
    public bool Enabled { get; set; } = true;
    public string BaseAddress { get; init; } = string.Empty;
}
