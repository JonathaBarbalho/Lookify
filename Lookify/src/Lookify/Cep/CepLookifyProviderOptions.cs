namespace Lookify.Cep;

public sealed class CepLookifyProviderOptions {

    public bool Enabled { get; set; } = true;

    public string BaseAddress { get; init; } = string.Empty;
}