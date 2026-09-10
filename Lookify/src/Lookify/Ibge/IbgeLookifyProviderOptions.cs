namespace Lookify.Ibge;

public sealed class IbgeLookifyProviderOptions {
    public bool Enabled { get; set; } = true;
    public string BaseAddress { get; init; } = string.Empty;
}
