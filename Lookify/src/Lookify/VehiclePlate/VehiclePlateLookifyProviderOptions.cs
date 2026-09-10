namespace Lookify.VehiclePlate;

public sealed class VehiclePlateLookifyProviderOptions {
    public bool Enabled { get; set; } = true;
    public string BaseAddress { get; init; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
